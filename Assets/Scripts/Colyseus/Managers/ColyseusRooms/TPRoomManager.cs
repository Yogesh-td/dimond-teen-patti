using Colyseus;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TeenPatti.App;
using TeenPatti.ColyseusKeys;
using TeenPatti.ColyseusStates;
using TeenPatti.Encryption;
using TeenPatti.Helpers;
using TeenPatti.Lobby;
using UnityEngine;

namespace TeenPatti.Gameplay
{
    public class TPRoomManager : ColyseusManager<TPRoomManager>
    {
        public event Action<int, string> Event_OnPlayer_Seat_Changed;

        ColyseusRoom<TPRoomStates> room;
        ISocket_GameEvents gameevent_observer;

        public GAME_TYPE GameType => (GAME_TYPE) Enum.Parse(typeof(GAME_TYPE), room.Name);

        public TPRoomStates State => room.State;
        public string SessionId => room.SessionId;
        public string RoomId => room.RoomId;
        public bool IsConnected => room != null;


        internal void Initialize(string endpoint)
        {
            CreateClient(endpoint);
        }


        #region Create, join, leave methods
        private Dictionary<string, object> Get_Room_Options(string table_id, string private_key)
        {
            return new Dictionary<string, object>()
                {
                    { PLAYER_INFO_KEYS.table_id, table_id },
                    { PLAYER_INFO_KEYS.access_token, ApiManager.Instance.access_token },
                    { PLAYER_INFO_KEYS.social_id, AppManager.Instance.PlayerData.social_id },
                    { PLAYER_INFO_KEYS.username, AppManager.Instance.PlayerData.username },
                    { PLAYER_INFO_KEYS.avatar_index, AppManager.Instance.PlayerData.avatar_index },
                    { METADATA_KEYS.private_key, string.IsNullOrEmpty(private_key) ? "" : AESCryptography.Encrypt(private_key) }
                };
        }
        public async UniTaskVoid JoinRoom(GAME_TYPE game_type, string table_id, Action success, Action<string> fail)
        {
            List<TPRoomAvailableData> available_rooms = ColyseusRoomManager.Instance.Get_Total_Metadata(game_type, table_id);
            available_rooms = available_rooms.FindAll(x => x.metadata.current_users < x.metadata.max_users);

            if (available_rooms.Count == 0)
                CreateRoom(game_type, table_id, false, success, fail).Forget();
            else
            {
                TPRoomAvailableData toJoinRoom = available_rooms[UnityEngine.Random.Range(0, available_rooms.Count)];
                try
                {
                    Dictionary<string, object> options = Get_Room_Options(table_id, "");

                    room = await client.JoinById<TPRoomStates>(toJoinRoom.roomId, options).AsUniTask();

                    Debug.LogFormat("Room Joined: {0}", JsonUtility.ToJson(room.ReconnectionToken).To_Color(Color.red));

                    Register_ConnectionHandlers();
                    Register_SeatHandler();
                    Register_MessageHandlers();

                    success?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.InnerException.Message);

                    string[] errors = exception.InnerException.Message.Split(":");
                    if (int.Parse(errors[0]) == 402 || exception.InnerException.Message.Contains("not found"))
                        CreateRoom(game_type, table_id, false, success, fail).Forget();
                    else
                        fail?.Invoke(errors[1]);
                }
            }
        }
        public async UniTaskVoid JoinRoom(string room_code, Action success, Action<string> fail)
        {
            List<TPRoomAvailableData> available_rooms = ColyseusRoomManager.Instance.Get_Private_Metadata();
            available_rooms = available_rooms.FindAll(x => x.metadata.current_users < x.metadata.max_users);

            if (available_rooms.Count == 0)
                fail.Invoke("No room found with this code!");
            else
            {
                TPRoomAvailableData toJoinRoom = available_rooms.Find(x => x.metadata.private_key == room_code);
                if(toJoinRoom == null)
                {
                    fail.Invoke("No room found with this code!");
                    return;
                }
                
                try
                {
                    Dictionary<string, object> options = Get_Room_Options(toJoinRoom.metadata.table_id, room_code);

                    room = await client.JoinById<TPRoomStates>(toJoinRoom.roomId, options).AsUniTask();

                    Debug.LogFormat("Room Joined: {0}", JsonUtility.ToJson(room.ReconnectionToken).To_Color(Color.red));

                    Register_ConnectionHandlers();
                    Register_SeatHandler();
                    Register_MessageHandlers();

                    success?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.InnerException.Message);

                    string[] errors = exception.InnerException.Message.Split(":");
                    fail?.Invoke(errors[1]);
                }
            }
        }
        public async UniTaskVoid CreateRoom(GAME_TYPE game_type, string table_id, bool is_private, Action success, Action<string> fail)
        {
            try
            {
                Dictionary<string, object> options = Get_Room_Options(table_id, is_private ? AESCryptography.Generate_Random_Key(6).ToLower() : "");          
                room = await client.Create<TPRoomStates>(game_type.ToString(), options).AsUniTask();

                Debug.LogFormat("Room Created: {0}", JsonUtility.ToJson(room.ReconnectionToken).To_Color(Color.red));

                Register_ConnectionHandlers();
                Register_SeatHandler();
                Register_MessageHandlers();

                success?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.Log(exception.InnerException?.Message);
                fail?.Invoke(exception.InnerException?.Message.Split(":")[1]);
            }
        }
        public async UniTaskVoid SwitchRoom(GAME_TYPE game_type, string table_id, string last_room_id, Action success, Action<string> fail)
        {
            List<TPRoomAvailableData> available_rooms = ColyseusRoomManager.Instance.Get_Total_Metadata(game_type, table_id);
            available_rooms = available_rooms.FindAll(x => x.metadata.current_users < x.metadata.max_users && x.roomId != last_room_id);

            if (available_rooms.Count == 0)
                CreateRoom(game_type, table_id, false, success, fail).Forget();
            else
            {
                TPRoomAvailableData toJoinRoom = available_rooms[UnityEngine.Random.Range(0, available_rooms.Count)];
                try
                {
                    Dictionary<string, object> options = Get_Room_Options(table_id, "");

                    room = await client.JoinById<TPRoomStates>(toJoinRoom.roomId, options).AsUniTask();

                    Debug.LogFormat("Room Joined: {0}", JsonUtility.ToJson(room.ReconnectionToken).To_Color(Color.red));

                    Register_ConnectionHandlers();
                    Register_SeatHandler();
                    Register_MessageHandlers();

                    success?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.InnerException.Message);

                    string[] errors = exception.InnerException.Message.Split(":");
                    if (int.Parse(errors[0]) == 402)
                        CreateRoom(game_type, table_id, false, success, fail).Forget();
                    else
                        fail?.Invoke(errors[1]);
                }
            }
        }
        public async UniTaskVoid RejoinRoom(string room_id, string reconnection_token, Action success, Action<string> fail)
        {
            try
            {
                ReconnectionToken token = new ReconnectionToken()
                {
                    RoomId = room_id,
                    Token = reconnection_token
                };

                room = await client.Reconnect<TPRoomStates>(token).AsUniTask();

                Debug.LogFormat("Room Reconnected: {0}", JsonUtility.ToJson(room.ReconnectionToken).To_Color(Color.red));

                Register_ConnectionHandlers();
                Register_SeatHandler();
                Register_MessageHandlers();

                success?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.Log(exception.InnerException?.Message);
                fail?.Invoke(exception.InnerException.Message.Contains(":") ? exception.InnerException?.Message.Split(":")[1] : exception.InnerException.Message);
            }
        }
        public async UniTask LeaveRoom(bool consented = true)
        {
            if (room == null)
                return;

            await room.Leave(consented).AsUniTask();
        }
        #endregion


        #region Connection Events
        private void Register_ConnectionHandlers()
        {
            room.OnError += Room_OnError;
            room.OnLeave += Room_OnLeave;
        }
        private void Deregister_ConnectionHandlers()
        {
            room.OnError -= Room_OnError;
            room.OnLeave -= Room_OnLeave;
        }
        private void Room_OnLeave(int code)
        {
            Debug.Log("Room left! Code: " + code);
            Deregister_ConnectionHandlers();

            room = null;

            if (code == SOCKET_EVENTS.PlayerKicked)
                OnMessage_LeaveRooms();
            else if (code == SOCKET_EVENTS.PlayerSwitchedTable)
                OnMessage_SwitchedTable();
        }
        private void Room_OnError(int code, string message)
        {
            Debug.LogFormat("Room error, Code: {0}, Message: {1}", code.ToString().To_Color(Color.red), message.To_Color(Color.red));
        }
        #endregion


        #region Seat Reservation Methods
        private void Register_SeatHandler()
        {
            room.State.all_seats.OnChange((index, value) =>
            {
                Event_OnPlayer_Seat_Changed?.Invoke(index, value);
            });
        }
        #endregion


        #region Message event Handler
        private void Register_MessageHandlers()
        {
            room.OnMessage<PLAYER_BET_DATA>(SOCKET_EVENTS.PlaceBet, OnMessage_PlayerPlacedBet);
            room.OnMessage<PLAYER_CARD_DATA>(SOCKET_EVENTS.RequestToSeeCards, OnMessage_LocalCardsReceived);
            room.OnMessage<ROUND_WIN_DATA>(SOCKET_EVENTS.DeclareWinner, OnMessage_DeclareWinner);
            room.OnMessage<SIDESHOW_DATA>(SOCKET_EVENTS.Sideshow_Request, OnMessage_Sideshow_Requested);
            room.OnMessage<PLAYER_CARD_DATA[]>(SOCKET_EVENTS.Sideshow_Accept, OnMessage_Sideshow_Accepted);
            room.OnMessage<dynamic>(SOCKET_EVENTS.Sideshow_Finished, OnMessage_Sideshow_Finished);
            room.OnMessage<int>(SOCKET_EVENTS.RequestTurnRemainingTime, OnMessage_TurnElapsedTimeReceived);
            room.OnMessage<PLAYER_CHAT_DATA>(SOCKET_EVENTS.PlayerChat, OnMessage_ChatReceived);
            room.OnMessage<PLAYER_CHAT_DATA>(SOCKET_EVENTS.PlayerGift, OnMessage_GiftReceived);
        }


        private void OnMessage_PlayerPlacedBet(PLAYER_BET_DATA betdata)
        {
            gameevent_observer?.On_PlayerPlacedBet(betdata.sessionId, betdata.betAmount);
        }
        private void OnMessage_LocalCardsReceived(PLAYER_CARD_DATA cardData)
        {
            gameevent_observer?.On_PlayerReceivedCards(cardData.sessionId, cardData.cards);
        }
        private void OnMessage_DeclareWinner(ROUND_WIN_DATA roundData)
        {
            gameevent_observer?.On_WinnerAnnounce(roundData);
        }
        private void OnMessage_Sideshow_Requested(SIDESHOW_DATA sideshowdData)
        {
            gameevent_observer?.On_SideshowRequested(sideshowdData);
        }
        private void OnMessage_Sideshow_Accepted(PLAYER_CARD_DATA[] cardData)
        {
            gameevent_observer?.On_SideshowAccepted(cardData);
        }
        private void OnMessage_Sideshow_Finished(dynamic dynamic)
        {
            gameevent_observer?.On_SideshowFinished();
        }
        private void OnMessage_TurnElapsedTimeReceived(int elapsed_time)
        {
            gameevent_observer?.On_TurnElapsedTimeReceived(elapsed_time);
        }
        private void OnMessage_ChatReceived(PLAYER_CHAT_DATA chat_data)
        {
            gameevent_observer?.On_ChatReceived(chat_data.sessionId, chat_data.message);
        }
        private void OnMessage_GiftReceived(PLAYER_CHAT_DATA msg_data)
        {
            string[] splitted_data = msg_data.message.Split(":");
            gameevent_observer?.On_GiftReceived(splitted_data[0], int.Parse(splitted_data[1]));
        }
        private void OnMessage_LeaveRooms()
        {
            gameevent_observer?.On_LeaveRooms();
        }
        private void OnMessage_SwitchedTable()
        {
            gameevent_observer?.On_SwitchRoom();
        }
        #endregion


        #region public methods for Sending events and setters
        public void Set_GameEvent_Observer(ISocket_GameEvents observer)
        {
            gameevent_observer = observer;
        }
        public void Send_Local_Ready()
        {
            room?.Send(SOCKET_EVENTS.PlayerReady);
        }
        public void Send_Local_Bet(bool isBetDoubled)
        {
            room?.Send(SOCKET_EVENTS.PlaceBet, isBetDoubled);
        }
        public void Send_Local_RequestToSeeCards()
        {
            room?.Send(SOCKET_EVENTS.RequestToSeeCards);
        }
        public void Send_Local_Pack()
        {
            room?.Send(SOCKET_EVENTS.PlayerPack);
        }
        public void Send_Local_Show()
        {
            room?.Send(SOCKET_EVENTS.PlayerShow);
        }
        public void Send_Sideshow_Request()
        {
            room?.Send(SOCKET_EVENTS.Sideshow_Request);
        }
        public void Send_Sideshow_Responce(bool isAccepted)
        {
            if (isAccepted)
                room?.Send(SOCKET_EVENTS.Sideshow_Accept);
            else
                room?.Send(SOCKET_EVENTS.Sideshow_Reject);
        }
        public void Send_Local_RequestToLeaveTable(bool backToMenu)
        {
            room?.Send(SOCKET_EVENTS.RequestToLeaveTable, backToMenu);
        }
        public void Send_Local_RequestTurnRemainingTime()
        {
            room?.Send(SOCKET_EVENTS.RequestTurnRemainingTime);
        }
        public void Send_Local_Chat(string msg)
        {
            room?.Send(SOCKET_EVENTS.PlayerChat, msg);
        }
        public void Send_Local_Gift(int gif_index, string receiver_session_id)
        {
            room?.Send(SOCKET_EVENTS.PlayerGift, new { gif_index = gif_index, receiver_session_id = receiver_session_id });
        }
        #endregion



        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            LeaveRoom(false).Forget();
        }
    }


    public interface ISocket_GameEvents
    {
        void On_PlayerPlacedBet(string sessionId, int betAmount);
        void On_PlayerReceivedCards(string sessionId, CARD_DATA[] cards);
        UniTaskVoid On_WinnerAnnounce(ROUND_WIN_DATA round_win_data);
        void On_SideshowRequested(SIDESHOW_DATA sideshowdData);
        void On_SideshowAccepted(PLAYER_CARD_DATA[] cardData);
        void On_SideshowFinished();
        void On_TurnElapsedTimeReceived(int elapsed_time);
        void On_ChatReceived(string sessionId, string msg);
        void On_GiftReceived(string receiver_sessionId, int gif_index);
        UniTaskVoid On_LeaveRooms();
        void On_SwitchRoom();
    }


    [System.Serializable]
    public class PLAYER_CHAT_DATA
    {
        public string sessionId;
        public string message;
    }
    [System.Serializable]
    public class PLAYER_BET_DATA
    {
        public string sessionId;
        public int betAmount;
    }
    [System.Serializable]
    public class PLAYER_CARD_DATA
    {
        public string sessionId;
        public CARD_DATA[] cards;
    }
    [System.Serializable]
    public class CARD_DATA
    {
        public int c_type;
        public int c_number;

        public Sprite Get_Sprite()
        {
            return AppManager.Instance.DeckSettings.Get_Card_Sprite(this.c_type, this.c_number);
        }
    }
    [System.Serializable]
    public class ROUND_WIN_DATA
    {
        public string[] winners;
        public int winners_reason;
        public string winners_handtype;

        public int pot_amount;
        public PLAYER_CARD_DATA[] players_cards;
    }
    [System.Serializable]
    public class SIDESHOW_DATA
    {
        public string sender_sid;
        public string receiver_sid;
        public int remaining_time;
    }
}