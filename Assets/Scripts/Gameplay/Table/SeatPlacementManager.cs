using Colyseus.Schema;
using Global.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using TeenPatti.ColyseusStates;
using UnityEngine;

namespace TeenPatti.Gameplay
{
    public class SeatPlacementManager : MonoBehaviour
    {
        [System.Serializable]
        public struct Seats
        {
            public PlayerBehaviour[] player_behaviours;
            public int localSeatIndex;
        }

        [Header("Cuurent Table Seats")]
        [SerializeField] List<Seats> all_seats;
        [SerializeField] Seats current_seats;

        [Header("Seat Indexing")]
        [SerializeField] List<string> player_seats;

        [Header("Script Refrences")]
        [SerializeField] TableDealerManager tableDealerManager;


        public void Initialize()
        {
            foreach (var player in all_seats[0].player_behaviours)
                player.gameObject.SetActive(false);

            SetupCurrentPlayers();
            TPRoomManager.Instance.Event_OnPlayer_Seat_Changed += OnPlayerSeat_AddedOrRemoved;
        }
        private void OnDestroy()
        {
            TPRoomManager.Instance.Event_OnPlayer_Seat_Changed -= OnPlayerSeat_AddedOrRemoved;
        }

        private void SetupCurrentPlayers()
        {
            player_seats = new List<string>();
            TPRoomManager.Instance.State.all_seats.ForEach((playerSessionId) =>
            {
                player_seats.Add(playerSessionId);
            });
            current_seats = all_seats.Find(x => x.player_behaviours.Length == player_seats.Count);
            foreach (var item in current_seats.player_behaviours)
                item.gameObject.SetActive(true);

            int placableSeatIndex = current_seats.localSeatIndex;
            int playerPositonIndex = player_seats.FindIndex(x => x == TPRoomManager.Instance.SessionId);

            //local player
            Place_Player(placableSeatIndex, player_seats[playerPositonIndex], TPRoomManager.Instance.State.all_players[player_seats[playerPositonIndex]]);

            //other player
            for (int i = 1; i < player_seats.Count; i++)
            {
                int updatedplacableSeatIndex = placableSeatIndex + i;
                int updatedplayerPositonIndex = playerPositonIndex + i;

                if (updatedplacableSeatIndex >= player_seats.Count)
                    updatedplacableSeatIndex = updatedplacableSeatIndex - player_seats.Count;

                if (updatedplayerPositonIndex >= player_seats.Count)
                    updatedplayerPositonIndex = updatedplayerPositonIndex - player_seats.Count;

                if (!string.IsNullOrEmpty(player_seats[updatedplayerPositonIndex]))
                    Place_Player(updatedplacableSeatIndex, player_seats[updatedplayerPositonIndex], TPRoomManager.Instance.State.all_players[player_seats[updatedplayerPositonIndex]]);
            }

            GameManager.Instance.Initialize();
            Timer.Schedule(this, 1f, TPRoomManager.Instance.Send_Local_Ready);

            Update_Table_Status();
        }
        private void OnPlayerSeat_AddedOrRemoved(int index, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                //player removed
                System.Array.Find(current_seats.player_behaviours, x => x.SessionId == player_seats[index]).RemovePlayer();
                player_seats[index] = "";
            }
            else
            {
                //player added
                player_seats[index] = value;
                int placableSeatIndex = current_seats.localSeatIndex;
                int playerPositonIndex = player_seats.FindIndex(x => x == TPRoomManager.Instance.SessionId);

                for (int i = 1; i < player_seats.Count; i++)
                {
                    int updatedplacableSeatIndex = placableSeatIndex + i;
                    int updatedplayerPositonIndex = playerPositonIndex + i;

                    if (updatedplacableSeatIndex >= player_seats.Count)
                        updatedplacableSeatIndex = updatedplacableSeatIndex - player_seats.Count;

                    if (updatedplayerPositonIndex >= player_seats.Count)
                        updatedplayerPositonIndex = updatedplayerPositonIndex - player_seats.Count;

                    if (updatedplayerPositonIndex == index)
                        Place_Player(updatedplacableSeatIndex, player_seats[updatedplayerPositonIndex], TPRoomManager.Instance.State.all_players[player_seats[updatedplayerPositonIndex]]);
                }
            }

            Update_Table_Status();
        }



        private void Place_Player(int seatIndex, string session_id, PlayerStates states)
        {
            current_seats.player_behaviours[seatIndex].FillPlayer(session_id, states);
        }
        private void Update_Table_Status()
        {
            List<string> players = player_seats.FindAll(x => !string.IsNullOrEmpty(x));
            tableDealerManager.Update_Status(players.Count <= 1 ? 
                "Waiting for other players" :
                "Starting round!\nPlease wait for few seconds");
        }



        public PlayerBehaviour[] Get_Table_PlayerBehaviours()
        {
            return System.Array.FindAll(current_seats.player_behaviours, x => !string.IsNullOrEmpty(x.SessionId));
        }
        public PlayerBehaviour Get_Local_PlayerBehaviour()
        {
            return current_seats.player_behaviours[current_seats.localSeatIndex];
        }
        public PlayerBehaviour Get_Turn_PlayerBehaviour()
        {
            return System.Array.Find(current_seats.player_behaviours, x => x.IsTurn);
        }
    }
}