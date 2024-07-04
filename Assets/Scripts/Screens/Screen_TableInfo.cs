using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_TableInfo : Base_Screen
    {
        [Space]
        [SerializeField] TextMeshProUGUI txt_game_mode;
        [SerializeField] TextMeshProUGUI txt_boot_amount;
        [SerializeField] TextMeshProUGUI txt_max_blind;
        [SerializeField] TextMeshProUGUI txt_max_bet;
        [SerializeField] TextMeshProUGUI txt_pot_limit;

        [Space]
        [SerializeField] GameObject[] objs_rules;

        [Space]
        [SerializeField] Button btn_close;

        [Space]
        [SerializeField] RectTransform[] layouts_to_refresh;

        private void Start()
        {
            btn_close.onClick.AddListener(OnClick_Close);
        }


        public override void Show()
        {
            ITABLESETTINGS selected_table = AppManager.Instance.Get_Table(TPRoomManager.Instance.State.table_id);

            txt_game_mode.text = TPRoomManager.Instance.GameType.ToString();
            txt_boot_amount.text = "₹ " + selected_table.boot_amount.ToString("n0");
            txt_max_blind.text = selected_table.max_blind.ToString("n0");
            txt_max_bet.text = "₹ " + selected_table.max_bet.ToString("n0");
            txt_pot_limit.text = "₹ " + selected_table.pot_limit.ToString("n0");

            if (TPRoomManager.Instance.GameType == GAME_TYPE.POTBLIND)
                txt_max_blind.text = "Always Blind";

            foreach (var item in objs_rules)
                item.gameObject.SetActive(false);

            objs_rules[(int)TPRoomManager.Instance.GameType].SetActive(true);

            base.Show();

            foreach (var item in layouts_to_refresh)
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        }


        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}