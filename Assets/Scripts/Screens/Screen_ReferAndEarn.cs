using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;

namespace TeenPatti.Screens
{
    public class Screen_ReferAndEarn : Base_Screen
    {
        [Header("Objects")]
        [SerializeField] GameObject obj_redeem;

        [Header("Refer objects")]
        [SerializeField] TextMeshProUGUI txt_refer_amounts;
        [SerializeField] TextMeshProUGUI txt_refer_code;
        [SerializeField] Button btn_copycode;
        [SerializeField] Button btn_share;

        [Header("Redeem objects")]
        [SerializeField] TextMeshProUGUI txt_redeem_amount;
        [SerializeField] TMP_InputField inpt_refer_code;
        [SerializeField] Button btn_redeem;

        [Space]
        [SerializeField] RectTransform[] layouts_to_refresh;

        [Space]
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_copycode.onClick.AddListener(OnClick_CopyCode);
            btn_share.onClick.AddListener(OnClick_Share);
            btn_redeem.onClick.AddListener(OnClick_Redeem);
            btn_close.onClick.AddListener(OnClick_Close);
        }



        public override void Show()
        {
            txt_refer_code.text = AppManager.Instance.PlayerData.refer_code;
            txt_refer_amounts.text = string.Format("you will get: <color=white>₹ {0}</color>\nyour friend will get: <color=white>₹ {1}</color>",
                AppManager.Instance.GameSettings.referer_coins.ToString("n0"),
                AppManager.Instance.GameSettings.refered_coins.ToString("n0"));
            txt_redeem_amount.text = string.Format("Have referral code, Use it to get\n<color=white>₹ {0}",
                AppManager.Instance.GameSettings.referer_coins.ToString("n0"));

            inpt_refer_code.text = string.Empty;
            obj_redeem.SetActive(string.IsNullOrEmpty(AppManager.Instance.PlayerData.refered_by));

            base.Show();

            foreach (var item in layouts_to_refresh)
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        }


        private void OnClick_CopyCode()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            GUIUtility.systemCopyBuffer = AppManager.Instance.PlayerData.refer_code;
        }
        private void OnClick_Share()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            string title = "Trending TeenPatti Game";
            string message = string.Format("Hi, Play {0} with me and earn alot money!\nUser my referral code - {1}\nand you will get ₹ {2} and ₹ {3} as a joining bonus immediately\n\nDownload the app: {4}",
                Application.productName,
                AppManager.Instance.PlayerData.refer_code,
                AppManager.Instance.GameSettings.refered_coins.ToString("n0"),
                AppManager.Instance.GameSettings.user_register_coins.ToString("n0"),
                CoreSettings.Instance.AppLink);

            new NativeShare().SetTitle(title).SetText(message).Share();
        }
        private void OnClick_Redeem()
        {
            if (string.IsNullOrEmpty(inpt_refer_code.text))
                return;

            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            AppManager.Instance.Redeem_Referal(inpt_refer_code.text, (success, error) =>
            {
                if (success)
                {
                    obj_redeem.SetActive(false);
                    foreach (var item in layouts_to_refresh)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                }

                UnityNativeToastsHelper.ShowShortText(error);
            });
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}