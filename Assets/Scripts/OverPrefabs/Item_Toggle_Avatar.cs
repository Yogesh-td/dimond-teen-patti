using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Item_Toggle_Avatar : MonoBehaviour
    {
        [SerializeField] Image img_avatar;
        [SerializeField] Toggle toggle_avatar;

        Screen_EditProfile screen_editprofile;

        public void Initialize(int id, Sprite sprite, bool isSelected, ToggleGroup toggleParent, Screen_EditProfile _screen_editprofile)
        {
            gameObject.name = id.ToString();
            img_avatar.sprite = sprite;

            screen_editprofile = _screen_editprofile;
            toggle_avatar.isOn = isSelected;
            toggle_avatar.group = toggleParent;

            toggle_avatar.onValueChanged.AddListener(OnChanged_Avatar);
        }

        private void OnChanged_Avatar(bool arg0)
        {
            if (!arg0)
                return;

            screen_editprofile.Update_Avatar(int.Parse(gameObject.name));
        }
    }
}