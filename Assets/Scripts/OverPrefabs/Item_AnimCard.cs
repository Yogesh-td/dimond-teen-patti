using System;
using TeenPatti.App.Settings;
using UnityEngine;

namespace TeenPatti.Gameplay
{
    public class Item_AnimCard : MonoBehaviour
    {
        [SerializeField] iTween.EaseType tween_anim;
        Action complete_callback;

        public void Initialize(Vector3 toDestroyLocation, Action callback = null)
        {
            complete_callback = callback;
            iTween.RotateTo(this.gameObject, iTween.Hash
            (
                "rotation", Vector3.one,
                "time", CoreSettings.Instance.Card_ThrowTime,
                "easetype", tween_anim,
                "islocal", true
            ));
            iTween.MoveTo(this.gameObject, iTween.Hash
                (
                    "position", toDestroyLocation,
                    "time", CoreSettings.Instance.Card_ThrowTime,
                    "easetype", tween_anim,
                    "oncomplete", nameof(Anim_Completed)
                ));
        }

        private void Anim_Completed()
        {
            complete_callback?.Invoke();
            Destroy(this.gameObject);
        }
    }
}