using Global.Helpers;
using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TeenPatti.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Menu : Base_Screen
    {
        [Header("Profile Sections")]
        [SerializeField] Image img_profile_avatar;
        [SerializeField] TextMeshProUGUI txt_profile_username;
        [SerializeField] Button btn_profile_avatar;

        [Header("Wallet")]
        [SerializeField] TextMeshProUGUI txt_wallet_balance;
        [SerializeField] Button btn_wallet_add;

        [Header("Game Buttons")]
        [SerializeField] Button btn_variation;
        [SerializeField] Button btn_diamond;
        [SerializeField] Button btn_playnow;
        [SerializeField] Button btn_tournament;
        [SerializeField] Button btn_private;

        [Header("Other Buttons")]
        [SerializeField] Button btn_aboutus;
        [SerializeField] Button btn_history;
        [SerializeField] Button btn_settings;
        [SerializeField] Button btn_leaderboard;
        [SerializeField] Button btn_referearn;
        [SerializeField] Button btn_shop;
        [SerializeField] Button btn_withdrawal;

        [Header("Extras")]
        [SerializeField] TextMeshProUGUI txt_appversion;

        private void Start()
        {
            btn_profile_avatar.onClick.AddListener(OnClick_Profile);
            btn_wallet_add.onClick.AddListener(OnClick_Wallet);

            btn_variation.onClick.AddListener(OnClick_Variation);
            btn_diamond.onClick.AddListener(OnClick_Diamond);
            btn_playnow.onClick.AddListener(OnClick_Playnow);
            btn_tournament.onClick.AddListener(OnClick_Tournament);
            btn_private.onClick.AddListener(OnClick_Private);

            btn_aboutus.onClick.AddListener(OnClick_AboutUs);
            btn_history.onClick.AddListener(OnClick_History);
            btn_settings.onClick.AddListener(OnClick_Settings);
            btn_leaderboard.onClick.AddListener(OnClick_Leaderboard);
            btn_referearn.onClick.AddListener(OnClick_ReferEarn);
            btn_shop.onClick.AddListener(OnClick_Shop);
            btn_withdrawal.onClick.AddListener(OnClick_Withdrawal);

            txt_appversion.text = string.Format("App-version: {0}", Application.version);
            AudioManager.Instance.Play_Music(MUSICS.BACKGROUND_IDLE);
        }


        public override void Show()
        {
            AppManager.Instance.Event_OnPlayerDataUpdated += Instance_Event_OnPlayerDataUpdated;
            AppManager.Instance.DeeplinkSettings.OnDeeplink_RoomCode_Received += Check_And_Process_Deeplink;
            TPLobbyManager.Instance.Event_OnLobby_Joined += Instance_Event_OnLobby_Joined;
            TPLobbyManager.Instance.Event_OnLobby_JoinedFailed += Instance_Event_OnLobby_JoinedFailed;
            TPLobbyManager.Instance.Event_OnLobby_Disconnected += Instance_Event_OnLobby_Disconnected;

            Fetch_User_Profile();
        }


        public override void Hide()
        {
            AppManager.Instance.Event_OnPlayerDataUpdated -= Instance_Event_OnPlayerDataUpdated;
            AppManager.Instance.DeeplinkSettings.OnDeeplink_RoomCode_Received -= Check_And_Process_Deeplink;
            TPLobbyManager.Instance.Event_OnLobby_Joined -= Instance_Event_OnLobby_Joined;
            TPLobbyManager.Instance.Event_OnLobby_JoinedFailed -= Instance_Event_OnLobby_JoinedFailed;
            TPLobbyManager.Instance.Event_OnLobby_Disconnected -= Instance_Event_OnLobby_Disconnected;

            base.Hide();
        }
        private void OnDestroy()
        {
            AppManager.Instance.Event_OnPlayerDataUpdated -= Instance_Event_OnPlayerDataUpdated;
            AppManager.Instance.DeeplinkSettings.OnDeeplink_RoomCode_Received -= Check_And_Process_Deeplink;
            TPLobbyManager.Instance.Event_OnLobby_Joined -= Instance_Event_OnLobby_Joined;
            TPLobbyManager.Instance.Event_OnLobby_JoinedFailed -= Instance_Event_OnLobby_JoinedFailed;
            TPLobbyManager.Instance.Event_OnLobby_Disconnected -= Instance_Event_OnLobby_Disconnected;
        }


        private void Fetch_User_Profile()
        {
            AppManager.Instance.Get_Profile((isSuccess, error) =>
            {
                if (isSuccess)
                {
                    base.Show();
                    if (TPLobbyManager.Instance.IsConnected)
                    {
                        if (!string.IsNullOrEmpty(AppManager.Instance.PlayerData.last_room_id))
                            Check_Last_Room_And_Join();
                        else if (!string.IsNullOrEmpty(AppManager.Instance.DeeplinkSettings.DeepLink_RoomCode))
                            Check_And_Process_Deeplink(AppManager.Instance.DeeplinkSettings.DeepLink_RoomCode);
                        else
                            SceneChangeManager.Instance.Hide_Loading();
                    }
                    else
                        TPLobbyManager.Instance.JoinLobby().Forget();
                }
                else
                    Fetch_User_Profile();
            });
        }
        private void Check_Last_Room_And_Join()
        {
            TPRoomManager.Instance.RejoinRoom(AppManager.Instance.PlayerData.last_room_id, AppManager.Instance.PlayerData.last_room_token,
            () =>
            {
                SceneChangeManager.Instance.ChangeScene(SCENES.Game, false);
            },
            (error) =>
            {
                SceneChangeManager.Instance.Hide_Loading();
                ScreenManager.Instance.Show_Warning("Room Join Failed\n" + error, "Okay", () =>
                {
                    SceneChangeManager.Instance.Show_Loading();
                    Fetch_User_Profile();
                }, false);
            }).Forget();
        }
        private void Check_And_Process_Deeplink(string roomcode)
        {
            if (string.IsNullOrEmpty(roomcode))
                return;

            SceneChangeManager.Instance.Show_Loading();
            Timer.Schedule(this, 1f, () =>
            {
                AppManager.Instance.DeeplinkSettings.Deeplink_Processed();
                TPRoomManager.Instance.JoinRoom(roomcode.ToLower().Replace(" ", ""),
                 () =>
                 {
                     SceneChangeManager.Instance.ChangeScene(SCENES.Game, false);
                 },
                 (error) =>
                 {
                     SceneChangeManager.Instance.Hide_Loading();
                     ScreenManager.Instance.Show_Warning("Joining room with code failed\n" + error, "Okay", null, false);
                 }).Forget();
            });
        }




        private void Instance_Event_OnPlayerDataUpdated()
        {
            Update_Profile_UI();
        }
        private void Instance_Event_OnLobby_JoinedFailed(string error)
        {
            SceneChangeManager.Instance.Hide_Loading();
            ScreenManager.Instance.Show_Warning("Cannot join lobby!\nPlease check your internet connection and try again!", "Re-Connect",
                () =>
                {
                    SceneChangeManager.Instance.Show_Loading();
                    TPLobbyManager.Instance.JoinLobby().Forget();
                }, false);
        }
        private void Instance_Event_OnLobby_Disconnected()
        {
            SceneChangeManager.Instance.Hide_Loading();
            ScreenManager.Instance.Show_Warning("Lobby Disconnected!\n\nPlease check your internet connection and try again!", "Re-Connect",
                () =>
                {
                    SceneChangeManager.Instance.Show_Loading();
                    TPLobbyManager.Instance.JoinLobby().Forget();
                }, false);
        }
        private void Instance_Event_OnLobby_Joined()
        {
            if (!string.IsNullOrEmpty(AppManager.Instance.PlayerData.last_room_id))
                Check_Last_Room_And_Join();
            else if (!string.IsNullOrEmpty(AppManager.Instance.DeeplinkSettings.DeepLink_RoomCode))
                Check_And_Process_Deeplink(AppManager.Instance.DeeplinkSettings.DeepLink_RoomCode);
            else
                SceneChangeManager.Instance.Hide_Loading();
        }
        private void Update_Profile_UI()
        {
            txt_wallet_balance.text = "₹ " + AppManager.Instance.PlayerData.wallet_balance.ToString("n0");
            img_profile_avatar.sprite = AppManager.Instance.AvatarSettings.Get_Avatar(AppManager.Instance.PlayerData.avatar_index);
            txt_profile_username.text = string.Format("{0}\n<size=25><color=white>ID: #{1}</color></size>",
                AppManager.Instance.PlayerData.username,
                AppManager.Instance.PlayerData.social_id);
        }



        private void OnClick_Profile()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.EDIT_PROFILE);
        }
        private void OnClick_Wallet()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.SHOP);
        }


        private void OnClick_Variation()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.VARIATION_PLAY);
        }
        private void OnClick_Diamond()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.DIAMOND_PLAY);
        }
        private void OnClick_Playnow()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.CLASSIC_PLAY);
        }
        private void OnClick_Tournament()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.Show_Warning("No tournaments available at this time!\nPlease come back later! Thanks!", "Okay", null);
        }
        private void OnClick_Private()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.PRIVATE_PLAY);
        }


        private void OnClick_AboutUs()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.ABOUT_US);
        }
        private void OnClick_History()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.HISTORY);
        }
        private void OnClick_Settings()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.SETTINGS);
        }
        private void OnClick_Leaderboard()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.LEADERBOARD);
        }
        private void OnClick_ReferEarn()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.SHARE_AND_EARN);
        }
        private void OnClick_Shop()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.SHOP);
        }
        private void OnClick_Withdrawal()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.WITHDRAWAL);
        }
    }
}