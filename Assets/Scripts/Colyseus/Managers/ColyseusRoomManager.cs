using Global.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TeenPatti.App.Settings;
using TeenPatti.Gameplay;
using TeenPatti.Lobby;
using UnityEngine;

namespace TeenPatti.App
{
    public class ColyseusRoomManager : Singleton<ColyseusRoomManager>, IRoom_MetadataEvents
    {
        public event Action<GAME_TYPE, string> Event_Room_Updated;

        [Header("Room Types")]
        [SerializeField] TPLobbyManager tplobby_room;
        [SerializeField] TPRoomManager tp_room;

        [Header("Rooms Metadata")]
        [SerializeField] List<TPRoomAvailableData> rooms_available_data;


        private void Start()
        {
            rooms_available_data = new List<TPRoomAvailableData>();

            tp_room.Initialize(CoreSettings.Instance.GameServer_URL);
            tplobby_room.Initialize(CoreSettings.Instance.GameServer_URL, this);
        }


        public void OnRoom_Fetched(List<TPRoomAvailableData> rooms)
        {
            rooms_available_data = rooms;
        }
        public void OnRoom_Added(TPRoomAvailableData room)
        {
            int roomIndex = rooms_available_data.FindIndex(x => x.roomId == room.roomId);
            if(roomIndex == -1)
            {
                rooms_available_data.Add(room);
                Event_Room_Updated?.Invoke((GAME_TYPE)Enum.Parse(typeof(GAME_TYPE), room.name, true), room.metadata.table_id);
            }
            else
            {
                rooms_available_data[roomIndex] = room;
                Event_Room_Updated?.Invoke((GAME_TYPE)Enum.Parse(typeof(GAME_TYPE), rooms_available_data[roomIndex].name, true), rooms_available_data[roomIndex].metadata.table_id);
            }
        }
        public void OnRoom_Removed(string roomId)
        {
            int roomIndex = rooms_available_data.FindIndex(x => x.roomId == roomId);
            if (roomIndex != -1)
            {
                rooms_available_data[roomIndex].metadata.current_users = 0;
                Event_Room_Updated?.Invoke((GAME_TYPE)Enum.Parse(typeof(GAME_TYPE), rooms_available_data[roomIndex].name, true), rooms_available_data[roomIndex].metadata.table_id);
                rooms_available_data.RemoveAt(roomIndex);
            }
        }


        public List<TPRoomAvailableData> Get_Total_Metadata(GAME_TYPE game_type, string table_id)
        {
            return rooms_available_data.FindAll(x => x.name == game_type.ToString() && x.metadata.table_id == table_id);
        }
        public List<TPRoomAvailableData> Get_Private_Metadata()
        {
            return rooms_available_data.FindAll(x => !string.IsNullOrEmpty(x.metadata.private_key));
        }
        public TPRoomAvailableData Get_Metadata(string room_id)
        {
            return rooms_available_data.Find(x => x.roomId == room_id);
        }
    }

    public interface IRoom_MetadataEvents
    {
        void OnRoom_Fetched(List<TPRoomAvailableData> rooms);
        void OnRoom_Added(TPRoomAvailableData room);
        void OnRoom_Removed(string roomId);
    }
}