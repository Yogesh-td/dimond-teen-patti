using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.Encryption;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;

namespace TeenPatti.Screens
{
    public class Screen_Login : Base_Screen
    {
        [Space]
        [SerializeField] TMP_InputField inpt_id;
        [SerializeField] TMP_InputField inpt_password;
        [SerializeField] Button btn_login;

        private void Start()
        {
            btn_login.onClick.AddListener(OnClick_Login);
        }

        public override void Show()
        {
            if (!string.IsNullOrEmpty(CoreSettings.Instance.SocialID) && !string.IsNullOrEmpty(CoreSettings.Instance.Password))
            {
                SceneChangeManager.Instance.Show_Loading();
                AppManager.Instance.Get_LoginAccess(CoreSettings.Instance.SocialID, AESCryptography.Decrypt(CoreSettings.Instance.Password),
                (sucess, msg) =>
                {
                    if (sucess)
                        SceneChangeManager.Instance.ChangeScene(SCENES.Menu, false);
                    else
                    {
                        PlayerPrefs.DeleteAll();
                        PlayerPrefs.Save();

                        SceneChangeManager.Instance.Hide_Loading();
                        base.Show();
                        ((Screen_Splash)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.SPLASH)).Toggle_IsSmall(true);
                    }
                });
            }
            else
            {
                base.Show();
                ((Screen_Splash)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.SPLASH)).Toggle_IsSmall(true);
            }
        }

        private void OnClick_Login()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);

            if (string.IsNullOrEmpty(inpt_id.text) || string.IsNullOrEmpty(inpt_password.text))
            {
                UnityNativeToastsHelper.ShowShortText("Please fill id and password!");
                return;
            }

            SceneChangeManager.Instance.Show_Loading();
            AppManager.Instance.Get_LoginAccess(inpt_id.text, inpt_password.text,
            (sucess, msg) =>
            {
                if (sucess)
                {
                    CoreSettings.Instance.SocialID = inpt_id.text;
                    CoreSettings.Instance.Password = AESCryptography.Encrypt(inpt_password.text);
                    SceneChangeManager.Instance.ChangeScene(SCENES.Menu, false);
                }
                else
                {
                    SceneChangeManager.Instance.Hide_Loading();
                    UnityNativeToastsHelper.ShowShortText(msg);
                }
            });
        }
    }
}