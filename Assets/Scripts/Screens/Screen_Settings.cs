using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Settings : Base_Screen
    {
        [Space]
        [SerializeField] Toggle toggle_sound;
        [SerializeField] Toggle toggle_music;
        [SerializeField] Toggle toggle_vibration;
        [SerializeField] Toggle toggle_notification;

        [Space]
        [SerializeField] Button btn_close;
        [SerializeField] Button btn_changepassword;
        [SerializeField] Button btn_logout;

        private void Start()
        {
            toggle_sound.onValueChanged.AddListener(OnChanged_Sound);
            toggle_music.onValueChanged.AddListener(OnChanged_Music);
            toggle_vibration.onValueChanged.AddListener(OnChanged_Vibration);
            toggle_notification.onValueChanged.AddListener(OnChanged_Notification);

            btn_close.onClick.AddListener(OnClick_Close);
            btn_changepassword.onClick.AddListener(OnClick_ChangePassword);
            btn_logout.onClick.AddListener(OnClick_Logout);
        }


        public override void Show()
        {
            toggle_sound.isOn = CoreSettings.Instance.Sound;
            toggle_music.isOn = CoreSettings.Instance.Music;
            toggle_vibration.isOn = CoreSettings.Instance.Vibration;
            toggle_notification.isOn = CoreSettings.Instance.Notification;

            base.Show();
        }

        private void OnChanged_Sound(bool arg0)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            CoreSettings.Instance.Sound = arg0;
        }
        private void OnChanged_Music(bool arg0)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            CoreSettings.Instance.Music = arg0;
        }
        private void OnChanged_Vibration(bool arg0)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            CoreSettings.Instance.Vibration = arg0;
        }
        private void OnChanged_Notification(bool arg0)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);

            if (arg0 && !AppManager.Instance.NotificationSettings.Has_Permission())
                AppManager.Instance.NotificationSettings.Request_Permission();

            CoreSettings.Instance.Notification = arg0;
        }

        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
        private void OnClick_ChangePassword()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.SwitchScreen(SCREEN_TYPE.CHANGE_PASSWORD, this.screen_type);
        }
        private void OnClick_Logout()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.Show_Warning("Are you sure wants to logout?", "Yes", () =>
            {
                AppManager.Instance.Logout();
            },
            true);
        }
    }
}