using System;
using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;

namespace TeenPatti.Screens
{
    public class Screen_Chat : Base_Screen
    {
        [Space]
        [SerializeField] Button fastMessage_prefab;
        [SerializeField] Transform fastMessage_parent;

        [Space]
        [SerializeField] TMP_InputField inpt_message;
        [SerializeField] Button btn_send;
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_send.onClick.AddListener(OnClick_Send);
            btn_close.onClick.AddListener(OnClick_Close);

            inpt_message.onValueChanged.AddListener(OnInputValue_Changed);

            foreach (var item in AppManager.Instance.Chat_Settings.Chat_Messages)
            {
                Button fastchat = Instantiate(fastMessage_prefab, fastMessage_parent);
                fastchat.GetComponentInChildren<TextMeshProUGUI>().text = item;
                fastchat.gameObject.SetActive(true);

                fastchat.onClick.AddListener(() => { Send_Chat(item); });
            }
        }

        public override void Show()
        {
            inpt_message.text = string.Empty;
            base.Show();
        }


        private void Send_Chat(string msg)
        {
            TPRoomManager.Instance.Send_Local_Chat(msg);
            OnClick_Close();
        }


        private void OnInputValue_Changed(string msg)
        {
            if (msg.Length > AppManager.Instance.Chat_Settings.Chat_MaxLength)
                inpt_message.text = msg.Substring(0, AppManager.Instance.Chat_Settings.Chat_MaxLength);
        }
        private void OnClick_Send()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            if (string.IsNullOrEmpty(inpt_message.text))
            {
                UnityNativeToastsHelper.ShowShortText("Invalid message cannot be sent!");
                return;
            }
            if (inpt_message.text.Length > AppManager.Instance.Chat_Settings.Chat_MaxLength)
            {
                UnityNativeToastsHelper.ShowShortText("Message characters length must be <= " + AppManager.Instance.Chat_Settings.Chat_MaxLength);
                return;
            }

            Send_Chat(inpt_message.text);
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}