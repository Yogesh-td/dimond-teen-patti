using System;
using TeenPatti.App.Settings;
using TMPro;
using UnityEngine;

namespace TeenPatti.Gameplay
{
    public class Item_CoinAmount : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txt_amount;
        [SerializeField] iTween.EaseType tween_anim;

        Action complete_callback;

        public void Initialize(string msg_txt, Vector3 toDestroyLocation, Action callback = null)
        {
            txt_amount.text = msg_txt;
            complete_callback = callback;

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