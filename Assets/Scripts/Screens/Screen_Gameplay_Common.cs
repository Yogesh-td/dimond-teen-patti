using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.Encryption;
using TeenPatti.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Gameplay_Common : Base_Screen
    {
        [Space]
        [SerializeField] TextMeshProUGUI txt_mode;

        [Space]
        [SerializeField] Button btn_back;
        [SerializeField] Button btn_info;
        [SerializeField] Button btn_chat;
        [SerializeField] Button btn_settings;

        [Space]
        [SerializeField] TextMeshProUGUI txt_roomcode;
        [SerializeField] Button btn_invite;

        private void Start()
        {
            btn_back.onClick.AddListener(OnClick_Back);
            btn_info.onClick.AddListener(OnClick_Info);
            btn_chat.onClick.AddListener(OnClick_Chat);
            btn_settings.onClick.AddListener(OnClick_Settings);
            btn_invite.onClick.AddListener(OnClick_Invite);

            txt_mode.text = TPRoomManager.Instance.GameType.ToString();

            string room_private_key = ColyseusRoomManager.Instance.Get_Metadata(TPRoomManager.Instance.RoomId).metadata.private_key;
            if (!string.IsNullOrEmpty(room_private_key))
            {
                txt_roomcode.text = room_private_key;
                txt_roomcode.transform.parent.gameObject.SetActive(true);
                btn_invite.gameObject.SetActive(true);
            }
        }



        private void OnClick_Back()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.GAME_MENU);
        }
        private void OnClick_Info()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.TABLE_INFO);
        }
        private void OnClick_Chat()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.CHAT);
        }
        private void OnClick_Settings()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.SETTINGS);
        }
        private void OnClick_Invite()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            string title = "Play TeenPatti with me!";
            string message = string.Format("Hi, I am challenging you to play {0} with me on {1} Table\nRoomCode: {2}\nLink: {3}",
                Application.productName,
                TPRoomManager.Instance.GameType.ToString(),
                txt_roomcode.text,
                CoreSettings.Instance.DeepLink + "roomcode?" + AESCryptography.Encrypt(txt_roomcode.text));

            Debug.Log(CoreSettings.Instance.DeepLink + "roomcode?" + AESCryptography.Encrypt(txt_roomcode.text));

            new NativeShare().SetTitle(title).SetText(message).Share();
        }
    }
}