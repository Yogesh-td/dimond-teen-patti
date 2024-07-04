using TeenPatti.App;
using TeenPatti.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;


namespace TeenPatti.Screens
{
    public class Screen_Withdrawal : Base_Screen
    {
        [Space]
        [SerializeField] TextMeshProUGUI txt_current;
        [SerializeField] TMP_InputField inpt_amount;

        [Space]
        [SerializeField] Button btn_cancel;
        [SerializeField] Button btn_close;
        [SerializeField] Button btn_withdraw;

        private void Start()
        {
            btn_cancel.onClick.AddListener(OnClick_Cancel);
            btn_close.onClick.AddListener(OnClick_Close);
            btn_withdraw.onClick.AddListener(OnClick_Withdraw);
        }


        public override void Show()
        {
            inpt_amount.text = string.Empty;
            txt_current.text = "₹ " + AppManager.Instance.PlayerData.wallet_balance.ToString("n0");
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
        private void OnClick_Withdraw()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            if (string.IsNullOrEmpty(inpt_amount.text))
            {
                UnityNativeToastsHelper.ShowShortText("invalid amount format");
                return;
            }

            AppManager.Instance.Withdrawal_Amount(int.Parse(inpt_amount.text),
            (success, msg) =>
            {
                UnityNativeToastsHelper.ShowShortText(msg);

                if (success)
                    OnClick_Cancel();
            });
        }
    }
}