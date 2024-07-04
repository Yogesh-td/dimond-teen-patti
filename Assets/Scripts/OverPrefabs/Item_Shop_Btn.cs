using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.IAP
{
    public class Item_Shop_Btn : MonoBehaviour
    {
        [SerializeField] Image img_graphic;
        [SerializeField] TextMeshProUGUI txt_chips;
        [SerializeField] Text txt_price;
        [SerializeField] GameObject obj_is_popular;

        IAP_PRODUCT product;

        public void Initialize(IAP_PRODUCT product)
        {
            this.product = product;

            img_graphic.sprite = product.graphic;
            txt_chips.text = product.coins.To_KiloFormat();
            txt_price.text = string.Format("₹ {0}", product.price);
            obj_is_popular.SetActive(product.is_popular);


            GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
                AppManager.Instance.IAPSettings.ProductPurchase_Init(this.product);
            });
        }
    }
}