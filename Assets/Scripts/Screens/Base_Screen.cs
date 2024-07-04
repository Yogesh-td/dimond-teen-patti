using Global.Helpers;
using TeenPatti.App.Settings;
using UnityEngine;

namespace TeenPatti.Screens
{
    public class Base_Screen : MonoBehaviour
    {
        [Header("Screen Settings")]
        [SerializeField] internal SCREEN_TYPE screen_type;
        [SerializeField] protected GameObject screen_anim_obj;
        [SerializeField] Animation screen_anim;


        private void OnDestroy()
        {
            Hide();
        }
        public virtual void Show()
        {
            if (gameObject.activeInHierarchy)
                return;

            gameObject.SetActive(true);
            if (screen_anim != null && screen_anim.GetClip("Show") != null)
            {
                screen_anim.clip = screen_anim.GetClip("Show");
                screen_anim["Show"].speed = screen_anim["Show"].length / CoreSettings.Instance.Screen_SwitchTime;
                screen_anim.Play();
            }
            else if (screen_anim_obj != null)
                Perform_PopupAnimation();
        }
        public virtual void Hide()
        {
            if (!gameObject.activeInHierarchy)
                return;

            float switchTime = 0;
            if (screen_anim != null && screen_anim.GetClip("Hide") != null)
            {
                screen_anim.clip = screen_anim.GetClip("Hide");
                screen_anim["Hide"].speed = screen_anim["Hide"].length / CoreSettings.Instance.Screen_SwitchTime;
                screen_anim.Play();

                switchTime = CoreSettings.Instance.Screen_SwitchTime;
            }

            Timer.Schedule(this, switchTime, () => 
            {
                gameObject.SetActive(false);
            });
        }


        private void Perform_PopupAnimation()
        {
            if (screen_anim_obj == null)
                return;

            screen_anim_obj.transform.localScale = Vector3.zero;
            iTween.ScaleTo(this.screen_anim_obj, iTween.Hash
                (
                    "scale", Vector3.one,
                    "time", CoreSettings.Instance.Screen_SwitchTime,
                    "easetype", CoreSettings.Instance.Screen_SwitchAnimation,
                    "ignoretimescale", true
                ));
        }
    }
}