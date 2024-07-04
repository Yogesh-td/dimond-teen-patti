using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeenPatti.Player
{
    [Serializable]
    public class PLAYERDATA : IPLAYERDATA
    {
        public string social_id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string user_type { get; set; }

        public int avatar_index { get; set; }
        public string refer_code { get; set; }
        public string refered_by { get; set; }

        public int wallet_balance { get; set; }

        public string last_room_id { get; set; }
        public string last_room_token { get; set; }
    }
    public interface IPLAYERDATA
    {
        string social_id { get; }
        string email { get; }
        string username { get; }
        string user_type { get; }

        int avatar_index { get; }
        string refer_code { get; }
        string refered_by { get; }

        int wallet_balance { get; }

        string last_room_id { get; }
        string last_room_token { get; }
    }

    [Serializable]
    public class PLAYER_TRANSACTION
    {
        public int amount;
        public string transaction_reason;
        public string transaction_type;
        public string status;
        public string message;
        public DateTime created_at;
        public DateTime modified_at;
    }


    [Serializable]
    public class LEADERBOARD_PLAYERS_DATA
    {
        public LEADERBOARD_PLAYER local_player;
        public List<LEADERBOARD_PLAYER> other_players;
    }
    [Serializable]
    public class LEADERBOARD_PLAYER
    {
        public int rank;
        public string username;
        public int avatar_index;
        public int total_winnings;
        public bool is_local;
    }
}

namespace TeenPatti.App
{
    [Serializable]
    public class GAMESETTINGS : IGAMESETTINGS
    {
        public int user_register_coins { get; set; }
        public int referer_coins { get; set; }
        public int refered_coins { get; set; }

        public string support_url { get; set; }
        public string support_email { get; set; }
        public string whatsapp_number { get; set; }

        public List<ModeVariation> game_variations { get; set; }
        public List<NotificationMsg> notification_msgs { get; set; }

        public string app_version { get; set; }
    }
    public interface IGAMESETTINGS
    {
        int user_register_coins { get; }
        int referer_coins { get; }
        int refered_coins { get; }

        string support_url { get; }
        string support_email { get; }
        string whatsapp_number { get; }

        List<ModeVariation> game_variations { get; }
        List<NotificationMsg> notification_msgs { get; }

        string app_version { get; }
    }
    [Serializable]
    public class NotificationMsg
    {
        public string title { get; set; }
        public string desc { get; set; }
    }

    [Serializable]
    public class ModeVariation
    {
        public string mode { get; set; }
        public bool is_active { get; set; }
    }

    [Serializable]
    public class TABLESETTING : ITABLESETTINGS
    {
        public string _id { get; set; }
        public int boot_amount { get; set; }
        public int max_blind { get; set; }
        public int max_bet { get; set; }
        public int pot_limit { get; set; }
        public bool is_delux { get; set; }
    }
    public interface ITABLESETTINGS
    {
        string _id { get; }
        int boot_amount { get; }
        int max_blind { get; }
        int max_bet { get; }
        int pot_limit { get; }
        bool is_delux { get; }
    }
}

namespace TeenPatti.IAP
{
    [Serializable]
    public class IAPPRODUCT : IIAPPRODUCT
    {
        public string _id { get; set; }
        public string product_id { get; set; }
        public int coins { get; set; }
        public int price { get; set; }
    }
    public interface IIAPPRODUCT
    {
        string _id { get; }
        string product_id { get; }
        int coins { get; }
        int price { get; }
    }
}

namespace TeenPatti.Helpers
{
    [System.Serializable]
    public class GIF_IMAGE
    {
        public Sprite[] all_sprites;
        public int fps;
    }
}

namespace TeenPatti.Notifications
{
    public interface INOTIFICATION_HANDLER
    {
        bool Has_Permission();
        void Request_Permission();
        void Register_Channel();
        void Register_Notification(string title, string desc, DateTime firetime);
        void Cancel_Notifications();
    }
}