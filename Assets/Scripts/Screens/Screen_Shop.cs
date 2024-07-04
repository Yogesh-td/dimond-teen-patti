using System;
using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;

namespace TeenPatti.Screens
{
    public class Screen_Shop : Base_Screen
    {
        [Space]
        [SerializeField] Toggle[] toogle_sections;
        [SerializeField] GameObject[] obj_sections;
        [SerializeField] Button btn_close;

        [Header("Chips")]
        [SerializeField] Item_Shop_Btn shop_btn_prefab_prefab;
        [SerializeField] Transform shop_btn_parent;

        [Header("Promocode")]
        [SerializeField] TMP_InputField inpt_promocode;
        [SerializeField] Button btn_submitcode;

        private void Start()
        {
            btn_submitcode.onClick.AddListener(OnClick_SubmitCode);
            btn_close.onClick.AddListener(OnClick_Close);

            foreach (var item in toogle_sections)
                item.onValueChanged.AddListener(OnToggleChanged_Sections);

            foreach (var item in AppManager.Instance.IAPSettings.Get_Available_Products())
            {
                Item_Shop_Btn product = Instantiate(shop_btn_prefab_prefab, shop_btn_parent);
                product.Initialize(item);
            }
        }

        public override void Show()
        {
            inpt_promocode.text = string.Empty;
            toogle_sections[0].isOn = true;

            base.Show();

            OnToggleChanged_Sections(true);
        }


        private void OnToggleChanged_Sections(bool value)
        {
            if (!value)
                return;

            int index = Array.FindIndex(toogle_sections, x => x.isOn);
            for (int i = 0; i < obj_sections.Length; i++)
                obj_sections[i].SetActive(i == index);

            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
        }
        private void OnClick_SubmitCode()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            if (string.IsNullOrEmpty(inpt_promocode.text))
            {
                UnityNativeToastsHelper.ShowShortText("Please enter a valid promocode!");
                return;
            }
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}