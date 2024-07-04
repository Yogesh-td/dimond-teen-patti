using Colyseus;
using GameDevWare.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System;
using TeenPatti.App;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TeenPatti.Helpers;
using NativeWebSocket;
using TeenPatti.ColyseusKeys;

namespace TeenPatti.Lobby
{
    public class TPLobbyManager : ColyseusManager<TPLobbyManager>
    {
        public event Action Event_OnLobby_Joined;
        public event Action<string> Event_OnLobby_JoinedFailed;
        public event Action Event_OnLobby_Disconnected;

        ColyseusRoom<dynamic> current_lobby;
        IRoom_MetadataEvents rooms_metadata_event_observer;

        public bool IsConnected => current_lobby != null;

        internal void Initialize(string endpoint, IRoom_MetadataEvents rooms_observer)
        {
            rooms_metadata_event_observer = rooms_observer;
            CreateClient(endpoint);
        }

        #region Join, Leave methods
        public async UniTaskVoid JoinLobby()
        {
            try
            {
                Dictionary<string, object> options = new Dictionary<string, object>()
                {
                    { PLAYER_INFO_KEYS.access_token, ApiManager.Instance.access_token },
                };

                current_lobby = await client.JoinOrCreate<dynamic>("Lobby", options).AsUniTask();
                Debug.Log(string.Format("Joined Lobby: {0}", JsonUtility.ToJson(current_lobby.ReconnectionToken).To_Color(Color.red)));

                Register_ConnectionHandlers();
                Register_MessageCallbacks();

                Event_OnLobby_Joined?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
                Debug.Log(exception.InnerException.Message);
                Event_OnLobby_JoinedFailed?.Invoke(exception.InnerException.Message);
            }
        }
        public async UniTask LeaveLobby(bool consented = true)
        {
            if (current_lobby == null)
                return;

            await current_lobby?.Leave(consented);
        }
        #endregion


        #region Connection Events
        private void Register_ConnectionHandlers()
        {
            current_lobby.OnError += Lobby_OnError;
            current_lobby.OnLeave += Lobby_OnLeave;
        }
        private void Deregister_ConnectionHandlers()
        {
            current_lobby.OnError -= Lobby_OnError;
            current_lobby.OnLeave -= Lobby_OnLeave;
        }
        private void Lobby_OnLeave(int code)
        {
            Debug.Log("Room left! Code: " + ((WebSocketCloseCode)code).ToString());
            Deregister_ConnectionHandlers();

            current_lobby = null;
            Event_OnLobby_Disconnected?.Invoke();
        }
        private void Lobby_OnError(int code, string message)
        {
            Debug.LogFormat("Room error, Code: {0}, Message: {1}", code.ToString().To_Color(Color.red), message.To_Color(Color.red));
        }
        #endregion


        #region Message Handlers
        private void Register_MessageCallbacks()
        {
            current_lobby.OnMessage<List<TPRoomAvailableData>>("rooms", received_rooms =>
            {
                rooms_metadata_event_observer?.OnRoom_Fetched(received_rooms);
            });
            current_lobby.OnMessage<object[]>("+", received_roomInfo =>
            {
                TPRoomAvailableData recievedRoomData = new TPRoomAvailableData();
                RoomMetadata_MapFields(ref recievedRoomData, typeof(TPRoomAvailableData).GetFields(), (IndexedDictionary<string, object>)received_roomInfo[received_roomInfo.Length - 1]);

                rooms_metadata_event_observer?.OnRoom_Added(recievedRoomData);
            });
            current_lobby.OnMessage<string>("-", received_roomId =>
            {
                rooms_metadata_event_observer?.OnRoom_Removed(received_roomId);
            });
            current_lobby.OnMessage<int>(SOCKET_EVENTS.PlayerBalanceUpdated, new_balance =>
            {
                AppManager.Instance.Update_PlayerData(new_balance);
            });
        }
        private T RoomMetadata_MapFields<T>(ref T obj, FieldInfo[] fields, IndexedDictionary<string, object> data)
        {
            foreach (FieldInfo field in fields)
            {
                if (data.ContainsKey(field.Name))
                {
                    data.TryGetValue(field.Name, out object value);
                    if (value is IndexedDictionary<string, object> indexedDictionary)
                    {
                        object fieldValue = Activator.CreateInstance(field.FieldType);
                        field.SetValue(obj, RoomMetadata_MapFields(ref fieldValue, field.FieldType.GetFields(), indexedDictionary));
                    }
                    else
                    {
                        field.SetValue(obj, value);
                    }
                }
            }

            return obj;
        }
        #endregion


        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            LeaveLobby(false).Forget();
        }
    }
}