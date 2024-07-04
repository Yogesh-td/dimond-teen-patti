using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.ColyseusStates;
using TeenPatti.Gameplay;
using TeenPatti.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Gameplay_Local : Base_Screen
    {
        [Header("Buttons")]
        [SerializeField] Button btn_pack;
        [SerializeField] Button btn_sideshow;
        [SerializeField] Button btn_show;
        [SerializeField] Button btn_bet;
        [SerializeField] Button btn_add_bet;
        [SerializeField] Button btn_minus_bet;

        [Header("Text components")]
        [SerializeField] TextMeshProUGUI txt_balance;
        [SerializeField] TextMeshProUGUI txt_bet;

        [Header("Other components")]
        [SerializeField] GameObject obj_buttons_panel;
        [SerializeField] Animation anim_buttons_panel;

        RoundPlayerStates roundStates_local;

        int current_bet_amount;
        bool isBetDoubled;

        int anim_balance;


        private void Start()
        {
            btn_pack.onClick.AddListener(OnClick_Pack);
            btn_sideshow.onClick.AddListener(OnClick_Sideshow);
            btn_show.onClick.AddListener(OnClick_Show);
            btn_bet.onClick.AddListener(OnClick_Bet);

            btn_add_bet.onClick.AddListener(OnClick_AddBet);
            btn_minus_bet.onClick.AddListener(OnClick_MinusBet);
        }
        public override void Show()
        {
            Update_Animated_Balance(AppManager.Instance.PlayerData.wallet_balance);
            base.Show();
        }


        public void Initialize_LocalPlayer(RoundPlayerStates behaviourRoundStatest)
        {
            roundStates_local = behaviourRoundStatest;
            roundStates_local.OnBalanceChange((newBalance, lastBalance) =>
            {
                Update_Balance();
            });
            roundStates_local.OnCurrent_bet_amountChange((newBalance, lastBalance) =>
            {
                current_bet_amount = roundStates_local.current_bet_amount;
                isBetDoubled = false;

                Update_Bet();
                Handle_AmountManipulateButtons();
            });
            roundStates_local.OnIsbetlimitreachedChange((newValue, lastValue) =>
            {
                if (!newValue)
                    return;

                Handle_AmountManipulateButtons();
            });
            roundStates_local.OnCansideshowChange((newValue, lastValue) =>
            {
                btn_sideshow.interactable = newValue;
            });
        }
        public void Show_Turn_Panel()
        {
            btn_sideshow.interactable = roundStates_local.cansideshow;

            current_bet_amount = roundStates_local.current_bet_amount;
            isBetDoubled = false;

            Update_Bet();
            Handle_AmountManipulateButtons();

            obj_buttons_panel.SetActive(true);
            anim_buttons_panel.Play();

#if UNITY_ANDROID || UNITY_IOS
            if (CoreSettings.Instance.Vibration)
                Handheld.Vibrate();
#endif
        }
        public void Hide_Turn_Panel()
        {
            obj_buttons_panel.SetActive(false);
        }



        private void OnClick_Pack()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.PACK);
            TPRoomManager.Instance.Send_Local_Pack();
            Hide_Turn_Panel();
        }
        private void OnClick_Sideshow()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.SHOW);
            TPRoomManager.Instance.Send_Sideshow_Request();
            Hide_Turn_Panel();
        }
        private void OnClick_Show()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.SHOW);
            TPRoomManager.Instance.Send_Local_Show();
            Hide_Turn_Panel();
        }
        private void OnClick_Bet()
        {
            TPRoomManager.Instance.Send_Local_Bet(isBetDoubled);
            Hide_Turn_Panel();
        }
        private void OnClick_AddBet()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.ADD_BET);
            isBetDoubled = true;

            Update_Bet();
            Handle_AmountManipulateButtons();
        }
        private void OnClick_MinusBet()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.SUBSTRACT_BET);
            isBetDoubled = false;

            Update_Bet();
            Handle_AmountManipulateButtons();
        }



        private void Update_Balance()
        {
            if (iTween.Count(this.gameObject) > 0)
                iTween.Stop(this.gameObject);

            iTween.ValueTo(this.gameObject, iTween.Hash
                (
                    "from", anim_balance,
                    "to", roundStates_local.balance,
                    "time", CoreSettings.Instance.Card_ThrowTime,
                    "easetype", iTween.EaseType.easeOutQuad,
                    "onupdate", nameof(Update_Animated_Balance),
                    "oncomplete", nameof(Complete_Animated_Balance)
                ));
        }
        private void Update_Animated_Balance(int value)
        {
            anim_balance = value;
            txt_balance.text = string.Format("<size=30>BALANCE -</size>\n<color=yellow>₹ {0}",
                anim_balance.ToString("n0"));
        }
        private void Complete_Animated_Balance()
        {
            ITABLESETTINGS selected_table = AppManager.Instance.Get_Table(TPRoomManager.Instance.State.table_id);
            if (anim_balance >= selected_table.boot_amount)
                return;

            ScreenManager.Instance.Show_Warning("Your BALANCE is LOW to play in this table\nPlease recharge your wallet!", "Go to Lobby!", () =>
            {
                SceneChangeManager.Instance.Show_Loading();
                TPRoomManager.Instance.Send_Local_RequestToLeaveTable(true);
            });
        }
        private void Update_Bet()
        {
            txt_bet.text = string.Format("{0}: <color=yellow>{1}",
                roundStates_local.isblind ? "BLIND" : "CHAAL",
                (current_bet_amount * (isBetDoubled ? 2 : 1)).To_KiloFormat());
        }
        private void Handle_AmountManipulateButtons()
        {
            if (roundStates_local.isbetlimitreached)
            {
                btn_add_bet.interactable = false;
                btn_minus_bet.interactable = false;
            }
            else
            {
                if (isBetDoubled)
                {
                    btn_add_bet.interactable = false;
                    btn_minus_bet.interactable = true;
                }
                else
                {
                    btn_add_bet.interactable = true;
                    btn_minus_bet.interactable = false;
                }
            }


            if (TPRoomManager.Instance.State.current_round.can_show)
            {
                btn_sideshow.gameObject.SetActive(false);
                btn_show.gameObject.SetActive(true);
            }
            else
            {
                btn_sideshow.gameObject.SetActive(true);
                btn_show.gameObject.SetActive(false);
            }
        }
    }
}