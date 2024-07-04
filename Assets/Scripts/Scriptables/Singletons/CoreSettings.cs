using Global.Helpers;
using TeenPatti.Audios;
using UnityEngine;

namespace TeenPatti.App.Settings
{

    [CreateAssetMenu(fileName = "CoreSettings", menuName = "Scriptables/CoreSettings", order = 1)]
    public class CoreSettings : SingletonScriptable<CoreSettings>
    {
        private const string KEY_SocialId = "SID";
        private const string KEY_Password = "PSWD";
        private const string KEY_Sound = "SND";
        private const string KEY_Music = "MSC";
        private const string KEY_Vibration = "VBR";
        private const string KEY_Notification = "NTF";

        [Header("Screen Settings")]
        [SerializeField] float screen_SwitchTime;
        [SerializeField] iTween.EaseType screen_SwitchAnimation;

        [Header("Gameplay Settings")]
        [SerializeField] float card_throwTime;

        [Header("Server URLS")]
        [SerializeField] string[] gameserver_urls;
        [SerializeField] string[] api_urls;



        public float Screen_SwitchTime { get => screen_SwitchTime; }
        public iTween.EaseType Screen_SwitchAnimation { get => screen_SwitchAnimation; }


        public float Card_ThrowTime { get => card_throwTime; }


        public string GameServer_URL { get => gameserver_urls[0]; }
        public string Api_URL { get => api_urls[0]; }



        public string AppLink
        {
            get
            {
                return string.Format("https://play.google.com/store/apps/details?id={0}", Application.identifier);
            }
        }
        public string DeepLink
        {
            get
            {
                return string.Format("https://diamondteenpatti.com/");
            }
        }


        public string SocialID
        {
            get { return PlayerPrefs.GetString(KEY_SocialId, string.Empty); }
            set { PlayerPrefs.SetString(KEY_SocialId, value); }
        }
        public string Password
        {
            get { return PlayerPrefs.GetString(KEY_Password, string.Empty); }
            set { PlayerPrefs.SetString(KEY_Password, value); }
        }
        public bool Sound
        {
            get { return PlayerPrefs.GetInt(KEY_Sound, 1) == 1 ? true : false; }
            set
            {
                PlayerPrefs.SetInt(KEY_Sound, value ? 1 : 0);
                AudioManager.Instance.Set_Sound_Status(value);
            }
        }
        public bool Music
        {
            get { return PlayerPrefs.GetInt(KEY_Music, 1) == 1 ? true : false; }
            set
            {
                PlayerPrefs.SetInt(KEY_Music, value ? 1 : 0);
                AudioManager.Instance.Set_Music_Status(value);
            }
        }
        public bool Vibration
        {
            get { return PlayerPrefs.GetInt(KEY_Vibration, 1) == 1 ? true : false; }
            set { PlayerPrefs.SetInt(KEY_Vibration, value ? 1 : 0); }
        }
        public bool Notification
        {
            get { return !AppManager.Instance.NotificationSettings.Has_Permission() ? false : PlayerPrefs.GetInt(KEY_Notification, 1) == 1 ? true : false; }
            set
            {
                PlayerPrefs.SetInt(KEY_Notification, value ? 1 : 0);
                if (value)
                    AppManager.Instance.NotificationSettings.Schedule_Notification();
                else
                    AppManager.Instance.NotificationSettings.Cancel_Notifications();
            }
        }
    }
}