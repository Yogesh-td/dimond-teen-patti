using TeenPatti.App;
using TeenPatti.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_AboutUs : Base_Screen
    {
        [Space]
        [SerializeField] TextMeshProUGUI txt_content;
        [SerializeField] Button btn_close;
        [SerializeField] Button btn_contactus;

        [Space]
        [SerializeField] ScrollRect scroll_content;

        private void Start()
        {
            btn_close.onClick.AddListener(OnClick_Close);
            btn_contactus.onClick.AddListener(OnClick_ContactUs);
        }

        public override void Show()
        {
            txt_content.text = txt_content.text.
                Replace("~APP_NAME~", Application.productName).
                Replace("~SUPPORT_URL~", AppManager.Instance.GameSettings.support_url).
                Replace("~SUPPORT_EMAIL_URL~", "mailto:" + AppManager.Instance.GameSettings.support_email).
                Replace("~SUPPORT_EMAIL~", AppManager.Instance.GameSettings.support_email).
                Replace("~WHATSAPP_LINK~", Get_Whatsapp_Link()).
                Replace("~WHATSAPP_NUMBER~", AppManager.Instance.GameSettings.whatsapp_number);

            base.Show();
            txt_content.transform.parent.localPosition = new Vector3(txt_content.transform.parent.localPosition.x, 0);
        }


        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
        private void OnClick_ContactUs()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            Application.OpenURL(Get_Whatsapp_Link());
        }

        private string Get_Whatsapp_Link()
        {
            string msg = string.Format("App: {0}\nID: {1}\nMsg: ", Application.productName, AppManager.Instance.PlayerData.social_id);
            msg = msg.Replace(" ", "%20").Replace("\n", "%0A");

            return string.Format("https://wa.me/{0}?text={1}", AppManager.Instance.GameSettings.whatsapp_number.Replace("+", ""), msg);
        }
    }
}