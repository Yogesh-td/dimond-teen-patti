using System;
using TeenPatti.Encryption;
using UnityEngine;

namespace TeenPatti.Deeplink
{
    [CreateAssetMenu(fileName = "DeeplinkSettings", menuName = "Scriptables/DeeplinkSettings", order = 1)]
    public class DeeplinkSettings : ScriptableObject
    {
        public event Action<string> OnDeeplink_RoomCode_Received;

        [Space]
        [SerializeField] string deeplink_process_url;
        [SerializeField] string deeplink_room_code;

        public string DeepLink_RoomCode => deeplink_room_code;


        public void Initialize()
        {
            Application.deepLinkActivated += Application_deepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
                Application_deepLinkActivated(Application.absoluteURL);

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(deeplink_process_url))
                Application_deepLinkActivated(deeplink_process_url);
#endif
        }

        private void Application_deepLinkActivated(string url)
        {
            string[] values = url.Split("roomcode?");
            if (values.Length < 2)
                return;

            deeplink_room_code = AESCryptography.Decrypt(values[1]);
            OnDeeplink_RoomCode_Received?.Invoke(deeplink_room_code);
        }


        public void Deeplink_Processed()
        {
            deeplink_room_code = string.Empty;
        }
    }
}