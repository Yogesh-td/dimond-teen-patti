using Cysharp.Threading.Tasks;
using Global.Helpers;
using System;
using System.Collections.Generic;
using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.ColyseusStates;
using TeenPatti.Lobby;
using TeenPatti.Screens;
using UnityEngine;

namespace TeenPatti.Gameplay
{
    public class GameManager : Singleton<GameManager>, ISocket_GameEvents
    {
        [Header("Round Manager")]
        [SerializeField] RoundManager round_manager;

        [Header("Prefabs")]
        [SerializeField] Item_CoinAmount prefab_coinamount_prefab;
        [SerializeField] Item_AnimCard prefab_animcard_prefab;

        [Header("Script References")]
        [SerializeField] SeatPlacementManager seatPlacementManager;
        [SerializeField] TableDealerManager tableDealerManager;

        [Header("Screen Refrences")]
        [SerializeField] Screen_Gameplay_Local screen_gameplaylocal;

        public PlayerBehaviour[] All_Table_Players => seatPlacementManager.Get_Table_PlayerBehaviours();
        public sbyte Current_Round_Status => round_manager.round_status;


        private Dictionary<string, object> last_room_params;


        public void Initialize()
        {
            TPRoomManager.Instance.Set_GameEvent_Observer(this);
            round_manager = new RoundManager(TPRoomManager.Instance.State.current_round, tableDealerManager);

            AudioManager.Instance.Play_Music(MUSICS.BACKGROUND_GAMEPLAY);
        }


        public async UniTask Collect_BootAmount(List<PlayerBehaviour> roundPlayers, int boot_amount)
        {
            tableDealerManager.Handle_Status_Visibility(false);
            tableDealerManager.Handle_Status_PotAmount(true);

            AudioManager.Instance.Play_Sound(SOUNDS.BOOT_COLLECTING);

            foreach (PlayerBehaviour player in roundPlayers)
                player.SendToDealer_BootAmount(boot_amount, prefab_coinamount_prefab.GetComponent<Item_CoinAmount>(), tableDealerManager.PotAmount_WorldPosition);

            await UniTask.Delay((int)(CoreSettings.Instance.Card_ThrowTime * 2000));
        }
        public async UniTask Distribute_Cards(List<PlayerBehaviour> roundPlayers, string joker_data)
        {
            for (int i = 0; i < 3; i++)
            {
                Sprite card_img = AppManager.Instance.DeckSettings.Card_Back_Sprite;
                if (TPRoomManager.Instance.GameType == App.GAME_TYPE.JOKER && i == 0)
                    card_img = AppManager.Instance.DeckSettings.Card_Joker_Sprite;

                foreach (var item in roundPlayers)
                {
                    AudioManager.Instance.Play_Sound(SOUNDS.CARD_RECEIVE);
                    item.ReceiveFromDealer_Card(i, card_img, prefab_animcard_prefab, tableDealerManager.Deck_WorldPosition);
                    await UniTask.Delay(200);
                }
            }

            if (!string.IsNullOrEmpty(joker_data))
            {
                AudioManager.Instance.Play_Sound(SOUNDS.CARD_RECEIVE);
                tableDealerManager.Receive_Joker_Card(joker_data, prefab_animcard_prefab);
                await UniTask.Delay(200);
            }

            await UniTask.Delay((int)(CoreSettings.Instance.Card_ThrowTime * 1000));
        }


        public void Request_Leave_Table()
        {
            ScreenManager.Instance.Show_Warning("Are you sure wants to leave this table?", "Leave", () =>
            {
                SceneChangeManager.Instance.Show_Loading();
                TPRoomManager.Instance.Send_Local_RequestToLeaveTable(true);
            });
        }
        public void Request_Switch_Table()
        {
            last_room_params = new Dictionary<string, object>
            {
                { "gametype", TPRoomManager.Instance.GameType },
                { "tableid", TPRoomManager.Instance.State.table_id },
                { "roomid", TPRoomManager.Instance.RoomId }
            };

            Action switchTable_PreAction = () =>
            {
                SceneChangeManager.Instance.Show_Loading();
                TPRoomManager.Instance.Send_Local_RequestToLeaveTable(false);
            };

            if (round_manager.local_in_round)
                ScreenManager.Instance.Show_Warning("Are you sure wants to switch table?\nAs you will lose your bet amount in this round!", "Switch", switchTable_PreAction);
            else
                switchTable_PreAction.Invoke();
        }



        #region ISocket Event Listners
        public void On_PlayerPlacedBet(string sessionId, int betAmount)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BET);

            PlayerBehaviour player = System.Array.Find(All_Table_Players, x => x.SessionId == sessionId);
            player?.SendToDealer_BootAmount(betAmount, prefab_coinamount_prefab, tableDealerManager.PotAmount_WorldPosition);
        }
        public void On_PlayerReceivedCards(string sessionId, CARD_DATA[] cards)
        {
            PlayerBehaviour player = System.Array.Find(All_Table_Players, x => x.SessionId == sessionId);
            player?.Received_Cards(cards, round_manager.Get_Joker_Card());
        }
        public async UniTaskVoid On_WinnerAnnounce(ROUND_WIN_DATA round_win_data)
        {
            screen_gameplaylocal.Hide_Turn_Panel();

            foreach (var item in All_Table_Players)
                item.Round_Ends();

            if (round_win_data.winners_reason == WIN_REASON.PLAYER_PACKED)
                tableDealerManager.Update_Status(string.Format("Player win because of\nother player PACKED!"));
            else if (round_win_data.winners_reason == WIN_REASON.OTHER_LEFT)
                tableDealerManager.Update_Status(string.Format("Player win because of\nother player LEFT TABLE!"));
            else
                tableDealerManager.Update_Status(string.Format("Player{0} wins because of\n<color=yellow>{1}</color> cards!", round_win_data.winners.Length > 1 ? "s" : "", round_win_data.winners_handtype));

            tableDealerManager.Handle_Status_Visibility(true);
            tableDealerManager.Handle_Status_PotAmount(false);

            foreach (var item in round_win_data.players_cards)
            {
                PlayerBehaviour player = Array.Find(All_Table_Players, x => x.SessionId == item.sessionId);
                player.Received_Cards(item.cards, round_manager.Get_Joker_Card()).Forget();
            }

            await UniTask.Delay((int)(CoreSettings.Instance.Card_ThrowTime * 1000));

            List<Vector3> coins_destinations = new List<Vector3>();
            foreach (var sid in round_win_data.winners)
            {
                PlayerBehaviour winner = Array.Find(All_Table_Players, x => x.SessionId == sid);
                if (winner != null)
                {
                    winner.ReceiveFromDealer_Coins(round_win_data.pot_amount / round_win_data.winners.Length, prefab_coinamount_prefab, tableDealerManager.PotAmount_WorldPosition);
                    coins_destinations.Add(winner.Throw_Position);
                }
            }
        }
        public void On_SideshowRequested(SIDESHOW_DATA sideshow_data)
        {
            round_manager.On_Round_Sideshow_Processing();
            screen_gameplaylocal.Hide_Turn_Panel();

            if (sideshow_data.receiver_sid == seatPlacementManager.Get_Local_PlayerBehaviour().SessionId)
            {
                Screen_SideshowReceived screen_ssReceived = (Screen_SideshowReceived)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.SIDESHOW_RECEIVED);
                screen_ssReceived.Update_Details(Array.Find(All_Table_Players, x => x.SessionId == sideshow_data.sender_sid).Username, sideshow_data.remaining_time / 1000f);

                ScreenManager.Instance.ShowScreen(SCREEN_TYPE.SIDESHOW_RECEIVED);
            }

            tableDealerManager.Sideshow_Effect_Show(
                Array.Find(All_Table_Players, x => x.SessionId == sideshow_data.sender_sid).transform.position,
                Array.Find(All_Table_Players, x => x.SessionId == sideshow_data.receiver_sid).transform.position);
        }
        public void On_SideshowAccepted(PLAYER_CARD_DATA[] cardData)
        {
            foreach (var item in cardData)
            {
                PlayerBehaviour player = Array.Find(All_Table_Players, x => x.SessionId == item.sessionId);
                player.Received_Cards(item.cards, round_manager.Get_Joker_Card()).Forget();
            }
        }
        public void On_SideshowFinished()
        {
            tableDealerManager.Sideshow_Effect_Hide();
        }
        public void On_TurnElapsedTimeReceived(int elapsed_time)
        {
            var turn_player = seatPlacementManager.Get_Turn_PlayerBehaviour();
            if (turn_player == null)
                return;

            turn_player.Force_UpdateTurnTime(elapsed_time);
        }
        public void On_ChatReceived(string sessionId, string msg)
        {
            PlayerBehaviour player = Array.Find(All_Table_Players, x => x.SessionId == sessionId);
            if (player == null)
                return;

            player.Received_Chat(msg);
        }
        public void On_GiftReceived(string receiver_sessionId, int gif_index)
        {
            PlayerBehaviour player = Array.Find(All_Table_Players, x => x.SessionId == receiver_sessionId);
            if (player == null)
                return;

            player.Received_Gift(gif_index);
        }
        public async UniTaskVoid On_LeaveRooms()
        {
            TPLobbyManager.Instance.LeaveLobby(true).Forget();
            while (TPLobbyManager.Instance.IsConnected)
                await UniTask.Delay(100);

            SceneChangeManager.Instance.ChangeScene(SCENES.Menu, false);
        }
        public void On_SwitchRoom()
        {
            TPRoomManager.Instance.SwitchRoom((GAME_TYPE)last_room_params["gametype"], (string)last_room_params["tableid"], (string)last_room_params["roomid"],
            () =>
            {
                SceneChangeManager.Instance.ChangeScene(SCENES.Game, false);
            },
            (error) =>
            {
                SceneChangeManager.Instance.Hide_Loading();
                ScreenManager.Instance.Show_Warning("Room switch Failed\n" + error, "Go To Lobby", () => On_LeaveRooms().Forget(), false);
            }).Forget();
        }
        #endregion
    }
}