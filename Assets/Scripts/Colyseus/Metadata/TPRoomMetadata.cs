using Colyseus;
using System;

namespace TeenPatti.Lobby
{
    [System.Serializable]
    public class TPRoomAvailableData : ColyseusRoomAvailable
    {
        public TPRoomMetadata metadata;
    }

    [System.Serializable]
    public class TPRoomMetadata
    {
        public string table_id;

        public UInt32 current_users;
        public UInt32 max_users;

        public string private_key;
    }
}