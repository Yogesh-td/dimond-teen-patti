using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityNative.Toasts.Example;
using TeenPatti.App;
using TeenPatti.Gameplay;
using TeenPatti.Audios;

namespace TeenPatti.Screens
{
    public class Screen_PrivateTable : Base_Screen
    {
        [Space]
        [SerializeField] TMP_InputField inpt_room_code;
        [SerializeField] Button btn_joinroom;
        [SerializeField] Button btn_createroom;
        [SerializeField] Button btn_cancel;
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_joinroom.onClick.AddListener(OnClick_JoinRoom);
            btn_createroom.onClick.AddListener(OnClick_CreateRoom);
            btn_cancel.onClick.AddListener(OnClick_Cancel);
            btn_close.onClick.AddListener(OnClick_Close);
        }

        public override void Show()
        {
            inpt_room_code.text = string.Empty;
            base.Show();
        }

        private void OnClick_JoinRoom()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            if (string.IsNullOrEmpty(inpt_room_code.text))
            {
                UnityNativeToastsHelper.ShowShortText("Invalid room code");
                return;
            }

            SceneChangeManager.Instance.Show_Loading();
            TPRoomManager.Instance.JoinRoom(inpt_room_code.text.ToLower().Replace(" ", ""),
                () =>
                {
                    SceneChangeManager.Instance.ChangeScene(SCENES.Game, false);
                },
                (error) =>
                {
                    SceneChangeManager.Instance.Hide_Loading();
                    ScreenManager.Instance.Show_Warning("Room Join Failed\n" + error, "Okay", null, false);
                }).Forget();
        }
        private void OnClick_CreateRoom()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.SwitchScreen(SCREEN_TYPE.PRIVATE_PLAY_CREATE, this.screen_type);
        }
        private void OnClick_Cancel()
        {
            OnClick_Close();
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}