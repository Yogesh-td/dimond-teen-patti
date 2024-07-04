using System;
using System.Collections.Generic;
using System.Linq;
using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Variation : Base_Screen
    {
        [Space]
        [SerializeField] bool only_diamond_tables;

        [Space]
        [SerializeField] Toggle[] toggles_gametypes;

        [Space]
        [SerializeField] Slider slider_amount;
        [SerializeField] Image slider_filler;
        [SerializeField] TextMeshProUGUI table_amount_prefab_prefab;
        [SerializeField] Transform table_amount_parent;

        [Space]
        [SerializeField] TextMeshProUGUI txt_boot_amount;
        [SerializeField] TextMeshProUGUI txt_max_blind;
        [SerializeField] TextMeshProUGUI txt_max_bet;
        [SerializeField] TextMeshProUGUI txt_pot_limit;
        [SerializeField] TextMeshProUGUI txt_online_players;

        [Space]
        [SerializeField] Button btn_cancel;
        [SerializeField] Button btn_join;
        [SerializeField] Button btn_close;

        int selected_toggle_index;
        ITABLESETTINGS selected_table;

        List<ITABLESETTINGS> available_tables;

        List<Toggle> available_gametypes;


        private void Start()
        {
            btn_cancel.onClick.AddListener(OnClick_Cancel);
            btn_join.onClick.AddListener(OnClick_Join);
            btn_close.onClick.AddListener(OnClick_Close);

            slider_amount.onValueChanged.AddListener(OnChanged_Slider);
            foreach (var item in toggles_gametypes)
                item.onValueChanged.AddListener(OnToggle_Changed_Gametype);
        }



        public override void Show()
        {
            available_tables = AppManager.Instance.GameTables.FindAll(x => x.is_delux == only_diamond_tables);
            foreach (var item in available_tables)
            {
                TextMeshProUGUI amount = Instantiate(table_amount_prefab_prefab, table_amount_parent);
                amount.text = "₹ " + item.boot_amount.ToString("n0");

                RectTransform amount_rect = amount.GetComponent<RectTransform>();
                amount_rect.sizeDelta = new Vector2(980 / available_tables.Count, amount_rect.sizeDelta.y);
            }
            table_amount_parent.GetComponent<HorizontalLayoutGroup>().spacing = (980 / available_tables.Count) / (available_tables.Count - 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(table_amount_parent.GetComponent<RectTransform>());

            slider_amount.maxValue = available_tables.Count - 1;
            slider_amount.value = 0;
            OnChanged_Slider(0);


            base.Show();

            toggles_gametypes[0].isOn = true;
            available_gametypes = new List<Toggle>();
            foreach (var item in toggles_gametypes)
            {
                if (AppManager.Instance.GameSettings.game_variations.FindIndex(x => x.mode == item.gameObject.name && x.is_active) != -1)
                {
                    available_gametypes.Add(item);
                    item.gameObject.SetActive(true);
                }
                else
                    item.gameObject.SetActive(false);
            }

            base.Show();

            available_gametypes[0].isOn = true;
            OnToggle_Changed_Gametype(true);

            ColyseusRoomManager.Instance.Event_Room_Updated += Instance_Event_Room_Updated;
        }
        public override void Hide()
        {
            ColyseusRoomManager.Instance.Event_Room_Updated -= Instance_Event_Room_Updated;

            for (int i = 0; i < table_amount_parent.childCount; i++)
                Destroy(table_amount_parent.GetChild(i).gameObject);

            base.Hide();
        }



        private void Instance_Event_Room_Updated(GAME_TYPE game_type, string table_id)
        {
            if (game_type != (GAME_TYPE)(selected_toggle_index + 1))
                return;

            txt_online_players.text = "" + (int)ColyseusRoomManager.Instance.Get_Total_Metadata((GAME_TYPE)(selected_toggle_index + 1), selected_table._id).Sum(x => { return x.metadata.current_users; });
        }



        private void OnToggle_Changed_Gametype(bool value)
        {
            if (!value)
                return;

            selected_toggle_index = Array.FindIndex(toggles_gametypes, x => x.isOn);

            foreach (var item in available_gametypes)
            {
                Color item_color = item.targetGraphic.color;
                if (item == toggles_gametypes[selected_toggle_index])
                    item_color.a = 1;
                else
                    item_color.a = 0.5f;

                item.targetGraphic.color = item_color;
            }

            OnChanged_Slider(slider_amount.value);
        }
        private void OnChanged_Slider(float index)
        {
            slider_filler.fillAmount = index / slider_amount.maxValue;

            selected_table = available_tables[(int)index];
            if (selected_table == null)
                return;

            txt_boot_amount.text = "₹ " + selected_table.boot_amount.ToString("n0");
            txt_max_blind.text = selected_table.max_blind.ToString("n0");
            txt_max_bet.text = "₹ " + selected_table.max_bet.ToString("n0");
            txt_pot_limit.text = "₹ " + selected_table.pot_limit.ToString("n0");
            txt_online_players.text = "" + (int)ColyseusRoomManager.Instance.Get_Total_Metadata((GAME_TYPE)(selected_toggle_index + 1), selected_table._id).Sum(x => { return x.metadata.current_users; });

            if ((GAME_TYPE)(selected_toggle_index + 1) == GAME_TYPE.POTBLIND)
                txt_max_blind.text = "Always Blind";

            AudioManager.Instance.Play_Sound(SOUNDS.SLIDER_CHANGE);
        }




        private void OnClick_Cancel()
        {
            OnClick_Close();
        }
        private void OnClick_Join()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);

            SceneChangeManager.Instance.Show_Loading();
            TPRoomManager.Instance.JoinRoom((GAME_TYPE)(selected_toggle_index + 1), selected_table._id,
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
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}