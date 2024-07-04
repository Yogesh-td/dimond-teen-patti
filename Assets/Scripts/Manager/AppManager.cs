using Global.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TeenPatti.App.Settings;
using TeenPatti.Avatar.Settings;
using TeenPatti.Chats;
using TeenPatti.Deeplink;
using TeenPatti.IAP;
using TeenPatti.Notifications;
using TeenPatti.Player;
using UnityEngine;

namespace TeenPatti.App
{
    public class AppManager : Singleton<AppManager>
    {
        public event Action Event_OnPlayerDataUpdated;
        public event Action Event_OnPlayerLoggedOut;

        private GAMESETTINGS game_settings;
        private List<TABLESETTING> game_tables;
        private List<IAPPRODUCT> game_iaps;
        private PLAYERDATA player_data;

        [Space]
        [SerializeField] DeckSettings deck_settings;
        [SerializeField] DeeplinkSettings deeplink_settings;
        [SerializeField] IAPSettings iap_settings;
        [SerializeField] NotificationSettings notification_settings;
        [SerializeField] AvatarSettings avatar_settings;
        [SerializeField] ChatSettings chat_settings;

        [Header("LOGS")]
        [SerializeField] bool disable_logs;


        public IGAMESETTINGS GameSettings => game_settings;
        public List<ITABLESETTINGS> GameTables => game_tables.Select(x => (ITABLESETTINGS)x).ToList();
        public IPLAYERDATA PlayerData => player_data;

        public DeckSettings DeckSettings => deck_settings;
        public DeeplinkSettings DeeplinkSettings => deeplink_settings;
        public IAPSettings IAPSettings => iap_settings;
        public NotificationSettings NotificationSettings => notification_settings;
        public AvatarSettings AvatarSettings => avatar_settings;
        public ChatSettings Chat_Settings => chat_settings;


        public override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;

            if (disable_logs)
            {
                Debug.unityLogger.logEnabled = false;
                Debug.ClearDeveloperConsole();
            }
        }
        private void Start()
        {
            deeplink_settings.Initialize();

            //Debug.Log(AESCryptography.Encrypt(JsonConvert.SerializeObject(new { user_id = "admin", password = "admin123" })));
        }

        #region Game and Table settings
        public void Get_GameSettings(Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_GetGameSettings(settings =>
            {
                game_settings = settings;
                isSucess.Invoke(true, null);

                notification_settings.Initialize(game_settings.notification_msgs);
            },
            error =>
            {
                isSucess.Invoke(false, error);
            });
        }
        public void Get_GameTables(Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_GetGameTables(tables =>
            {
                game_tables = tables;
                isSucess.Invoke(true, null);
            },
            error =>
            {
                isSucess.Invoke(false, error);
            });
        }
        public void Get_GameIaps(Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_GetGameIaps(iaps =>
            {
                game_iaps = iaps;
                iap_settings.Set_Available_Products(game_iaps.Select(x => (IIAPPRODUCT)x).ToList());

                isSucess.Invoke(true, null);
            },
            error =>
            {
                isSucess.Invoke(false, error);
            });
        }
        #endregion


        #region Player usability methods
        public void Get_LoginAccess(string social_id, string password, Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_Login(social_id, password,
            () =>
            {
                isSucess?.Invoke(true, "User LoggedIn Successfully!");
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Get_Profile(Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_GetProfile(
            (player) =>
            {
                player_data = player;
                isSucess?.Invoke(true, "Got user data successfully!");

                Event_OnPlayerDataUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Update_PlayerData(string username, int avatar_index, Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_UpdateProfile(username, avatar_index,
            (player) =>
            {
                player_data = player;
                isSucess?.Invoke(true, "Profile updated successfully!");

                Event_OnPlayerDataUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Update_Password(string current_password, string new_password, Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_UpdatePassword(current_password, new_password,
            (success_msg) =>
            {
                isSucess?.Invoke(true, success_msg);
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Update_PlayerData(int balance)
        {
            player_data.wallet_balance = balance;
            Event_OnPlayerDataUpdated?.Invoke();
        }
        public void Redeem_Referal(string refer_code, Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_RedeemRefer(refer_code,
            (player) =>
            {
                player_data = player;
                isSucess?.Invoke(true, "Refer updated successfully!");

                Event_OnPlayerDataUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Validate_Purchase(string product_id, string trx_id, Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_ValidatePurchase(game_iaps.Find(x => x.product_id == product_id)._id, trx_id,
            (player) =>
            {
                player_data = player;
                isSucess?.Invoke(true, "Purchase successfull!");

                Event_OnPlayerDataUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Withdrawal_Amount(int amount, Action<bool, string> isSucess)
        {
            ApiManager.Instance.Api_Withdraw(amount,
            (player) =>
            {
                player_data.wallet_balance = player.wallet_balance;
                isSucess?.Invoke(true, "Withdrawal successfully! We are processing your payment shortly");

                Event_OnPlayerDataUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.Log(error);
                isSucess?.Invoke(false, error);
            });
        }
        public void Logout()
        {
            Event_OnPlayerLoggedOut?.Invoke();
            game_settings = null;
            player_data = null;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            SceneChangeManager.Instance.ChangeScene(SCENES.Login, true);
        }
        #endregion


        #region Gameplay usability methods
        public ITABLESETTINGS Get_Table(string table_id)
        {
            return GameTables.Find(x => x._id == table_id);
        }
        #endregion
    }
}