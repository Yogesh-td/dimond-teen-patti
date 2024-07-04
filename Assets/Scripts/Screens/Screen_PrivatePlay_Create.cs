using System;
using System.Collections.Generic;
using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_PrivatePlay_Create : Base_Screen
    {
        [Space]
        [SerializeField] TMP_Dropdown dropdown_game_type_selection;
        [SerializeField] GameObject obj_variation;
        [SerializeField] GameObject obj_diamond;

        [Space]
        [SerializeField] Toggle[] toggles_variation_gametypes;
        [SerializeField] Toggle[] toggles_diamond_gametypes;

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

        [Space]
        [SerializeField] Button btn_cancel;
        [SerializeField] Button btn_create;
        [SerializeField] Button btn_close;


        int selected_toggle_index;
        ITABLESETTINGS selected_table;

        List<ITABLESETTINGS> available_tables;

        List<Toggle> available_gametypes_variation;
        List<Toggle> available_gametypes_diamond;

        private void Start()
        {
            dropdown_game_type_selection.onValueChanged.AddListener(OnGameTypeSelection_Changed);

            btn_cancel.onClick.AddListener(OnClick_Cancel);
            btn_create.onClick.AddListener(OnClick_Create);
            btn_close.onClick.AddListener(OnClick_Close);

            slider_amount.onValueChanged.AddListener(OnChanged_Slider);
            foreach (var item in toggles_variation_gametypes)
                item.onValueChanged.AddListener(OnToggle_Changed_Gametype_Variation);
            foreach (var item in toggles_diamond_gametypes)
                item.onValueChanged.AddListener(OnToggle_Changed_Gametype_Diamond);
        }
        public override void Show()
        {
            dropdown_game_type_selection.value = 0;
            base.Show();
            OnGameTypeSelection_Changed(0);
        }
        private void SetValues_Game_Type_Selection(List<ITABLESETTINGS> tables, int game_type_index)
        {
            available_tables = tables;

            for (int i = 0; i < table_amount_parent.childCount; i++)
                Destroy(table_amount_parent.GetChild(i).gameObject);

            foreach (var item in tables)
            {
                TextMeshProUGUI amount = Instantiate(table_amount_prefab_prefab, table_amount_parent);
                amount.text = "₹ " + item.boot_amount.ToString("n0");

                RectTransform amount_rect = amount.GetComponent<RectTransform>();
                amount_rect.sizeDelta = new Vector2(980 / tables.Count, amount_rect.sizeDelta.y);
            }
            table_amount_parent.GetComponent<HorizontalLayoutGroup>().spacing = (980 / tables.Count) / (tables.Count - 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(table_amount_parent.GetComponent<RectTransform>());

            available_gametypes_variation = new List<Toggle>();
            available_gametypes_diamond = new List<Toggle>();
            for (int i = 0; i < toggles_variation_gametypes.Length; i++)
            {
                if (AppManager.Instance.GameSettings.game_variations.FindIndex(x => x.mode == toggles_variation_gametypes[i].gameObject.name && x.is_active) != -1)
                {
                    available_gametypes_variation.Add(toggles_variation_gametypes[i]);
                    available_gametypes_diamond.Add(toggles_diamond_gametypes[i]);

                    toggles_variation_gametypes[i].gameObject.SetActive(true);
                    toggles_diamond_gametypes[i].gameObject.SetActive(true);
                }
                else
                {
                    toggles_variation_gametypes[i].gameObject.SetActive(false);
                    toggles_diamond_gametypes[i].gameObject.SetActive(false);
                }
            }

            if (game_type_index == 1)
            {
                available_gametypes_variation[0].isOn = true;
                OnToggle_Changed_Gametype_Variation(true);
            }
            else if (game_type_index == 2)
            {
                available_gametypes_diamond[0].isOn = true;
                OnToggle_Changed_Gametype_Diamond(true);
            }

            slider_amount.maxValue = tables.Count - 1;
            slider_amount.value = 0;
            OnChanged_Slider(0);
        }


        private void OnGameTypeSelection_Changed(int index)
        {
            switch (index)
            {
                case 0:
                    obj_variation.SetActive(false);
                    obj_diamond.SetActive(false);
                    SetValues_Game_Type_Selection(AppManager.Instance.GameTables, index);
                    break;
                case 1:
                    obj_variation.SetActive(true);
                    obj_diamond.SetActive(false);
                    SetValues_Game_Type_Selection(AppManager.Instance.GameTables.FindAll(x => !x.is_delux), index);
                    break;
                case 2:
                    obj_variation.SetActive(false);
                    obj_diamond.SetActive(true);
                    SetValues_Game_Type_Selection(AppManager.Instance.GameTables.FindAll(x => x.is_delux), index);
                    break;
                default:
                    break;
            }
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

            if ((GAME_TYPE)(selected_toggle_index + 1) == GAME_TYPE.POTBLIND)
                txt_max_blind.text = "Always Blind";

            AudioManager.Instance.Play_Sound(SOUNDS.SLIDER_CHANGE);
        }
        private void OnToggle_Changed_Gametype_Variation(bool value)
        {
            if (!value)
                return;

            selected_toggle_index = Array.FindIndex(toggles_variation_gametypes, x => x.isOn);
            foreach (var item in available_gametypes_variation)
            {
                Color item_color = item.targetGraphic.color;
                if (item == toggles_variation_gametypes[selected_toggle_index])
                    item_color.a = 1;
                else
                    item_color.a = 0.5f;

                item.targetGraphic.color = item_color;
            }

            OnChanged_Slider(slider_amount.value);
        }
        private void OnToggle_Changed_Gametype_Diamond(bool value)
        {
            if (!value)
                return;

            selected_toggle_index = Array.FindIndex(toggles_diamond_gametypes, x => x.isOn);
            foreach (var item in available_gametypes_diamond)
            {
                Color item_color = item.targetGraphic.color;
                if (item == toggles_diamond_gametypes[selected_toggle_index])
                    item_color.a = 1;
                else
                    item_color.a = 0.5f;

                item.targetGraphic.color = item_color;
            }

            OnChanged_Slider(slider_amount.value);
        }



        private void OnClick_Cancel()
        {
            OnClick_Close();
        }
        private void OnClick_Create()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            SceneChangeManager.Instance.Show_Loading();
            TPRoomManager.Instance.CreateRoom(dropdown_game_type_selection.value == 0 ? GAME_TYPE.CLASSIC : (GAME_TYPE)(selected_toggle_index + 1),
                selected_table._id,
                true,
                () =>
                {
                    SceneChangeManager.Instance.ChangeScene(SCENES.Game, false);
                },
                (error) =>
                {
                    SceneChangeManager.Instance.Hide_Loading();
                    ScreenManager.Instance.Show_Warning("Private table creation failed!\n" + error, "Okay", null, false);
                }).Forget();
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}