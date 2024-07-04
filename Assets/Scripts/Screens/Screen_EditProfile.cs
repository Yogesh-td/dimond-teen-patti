using TeenPatti.App;
using TeenPatti.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Toasts.Example;

namespace TeenPatti.Screens
{
    public class Screen_EditProfile : Base_Screen
    {
        [Header("Current Profile")]
        [SerializeField] Image img_avatar;
        [SerializeField] TextMeshProUGUI txt_userid;
        [SerializeField] TMP_InputField inpt_username;

        [Header("Avatars")]
        [SerializeField] Item_Toggle_Avatar toggle_avatar_prefab_prefab;
        [SerializeField] Transform toggle_avatar_parent;

        [Header("Others")]
        [SerializeField] Button btn_update;
        [SerializeField] Button btn_cancel;

        int selected_avatar_index;

        private void Start()
        {
            btn_update.onClick.AddListener(OnClick_Update);
            btn_cancel.onClick.AddListener(OnClick_Cancel);

            txt_userid.text = string.Format("ID: #{0}", AppManager.Instance.PlayerData.social_id);
            selected_avatar_index = AppManager.Instance.PlayerData.avatar_index;

            ToggleGroup toggleParent = toggle_avatar_parent.GetComponent<ToggleGroup>();
            Sprite[] all_avatars = AppManager.Instance.AvatarSettings.Get_All_Avatars();

            for (int i = 0; i < all_avatars.Length; i++)
            {
                Item_Toggle_Avatar avatar = Instantiate(toggle_avatar_prefab_prefab, toggle_avatar_parent);
                avatar.Initialize(i, all_avatars[i], i == selected_avatar_index, toggleParent, this);
            }

            toggle_avatar_parent.localPosition = new Vector3(0, toggle_avatar_parent.localPosition.y);
        }


        public override void Show()
        {
            inpt_username.text = AppManager.Instance.PlayerData.username;
            img_avatar.sprite = AppManager.Instance.AvatarSettings.Get_Avatar(AppManager.Instance.PlayerData.avatar_index);

            base.Show();

            toggle_avatar_parent.localPosition = new Vector3(0, toggle_avatar_parent.localPosition.y);
        }



        private void OnClick_Update()
        {
            if (inpt_username.text != AppManager.Instance.PlayerData.username || selected_avatar_index != AppManager.Instance.PlayerData.avatar_index)
            {
                AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
                if (string.IsNullOrEmpty(inpt_username.text))
                {
                    UnityNativeToastsHelper.ShowShortText("Invalid username");
                    return;
                }

                AppManager.Instance.Update_PlayerData(inpt_username.text, selected_avatar_index, (success, msg) =>
                {
                    if (success)
                        OnClick_Cancel();

                    UnityNativeToastsHelper.ShowShortText(msg);
                });
            }
            else
                OnClick_Cancel();
        }
        private void OnClick_Cancel()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);

            toggle_avatar_parent.Find(selected_avatar_index.ToString()).GetComponent<Toggle>().isOn = false;
            toggle_avatar_parent.Find(AppManager.Instance.PlayerData.avatar_index.ToString()).GetComponent<Toggle>().isOn = true;

            ScreenManager.Instance.HideScreen(this.screen_type);
        }



        public void Update_Avatar(int index)
        {
            selected_avatar_index = index;
            img_avatar.sprite = AppManager.Instance.AvatarSettings.Get_Avatar(selected_avatar_index);
        }
    }
}