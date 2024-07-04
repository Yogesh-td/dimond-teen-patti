using TeenPatti.App;
using TeenPatti.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;

namespace TeenPatti.Screens
{
    public class Screen_ChangePassword : Base_Screen
    {
        [Space]
        [SerializeField] TMP_InputField inpt_current;
        [SerializeField] TMP_InputField inpt_new;

        [Space]
        [SerializeField] Button btn_cancel;
        [SerializeField] Button btn_close;
        [SerializeField] Button btn_update;

        private void Start()
        {
            btn_cancel.onClick.AddListener(OnClick_Cancel);
            btn_close.onClick.AddListener(OnClick_Close);
            btn_update.onClick.AddListener(OnClick_Update);
        }


        public override void Show()
        {
            inpt_current.text = string.Empty;
            inpt_new.text = string.Empty;
            base.Show();
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
        private void OnClick_Update()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            if (string.IsNullOrEmpty(inpt_current.text))
            {
                UnityNativeToastsHelper.ShowShortText("invalid current_password format");
                return;
            }
            if (string.IsNullOrEmpty(inpt_new.text))
            {
                UnityNativeToastsHelper.ShowShortText("invalid new_password format");
                return;
            }

            AppManager.Instance.Update_Password(inpt_current.text, inpt_new.text, (success, msg) =>
            {
                UnityNativeToastsHelper.ShowShortText(msg);

                if (success)
                    OnClick_Cancel();
            });
        }
    }
}