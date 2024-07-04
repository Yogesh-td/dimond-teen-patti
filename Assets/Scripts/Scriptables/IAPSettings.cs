using System.Collections.Generic;
using TeenPatti.App;
using TeenPatti.Encryption;
using TeenPatti.Screens;
using UnityEngine;

namespace TeenPatti.IAP
{
    [CreateAssetMenu(fileName = "IAPSettings", menuName = "Scriptables/IAPSettings", order = 1)]
    public class IAPSettings : ScriptableObject
    {
        [SerializeField] IAP_PRODUCT[] all_products;
        private List<IAP_PRODUCT> available_products;


        public List<IAP_PRODUCT> Get_Available_Products()
        {
            return available_products;
        }
        public void Set_Available_Products(List<IIAPPRODUCT> iaps)
        {
            available_products = new List<IAP_PRODUCT>();
            foreach (var item in iaps)
            {
                IAP_PRODUCT product = System.Array.Find(all_products, x => x.product_id == item.product_id);
                if (product != null)
                {
                    product.coins = item.coins;
                    product.price = item.price;
                    available_products.Add(product);
                }
            }
        }
        public void ProductPurchase_Init(IAP_PRODUCT product)
        {
            AppManager.Instance.Validate_Purchase(product.product_id, AESCryptography.Generate_Random_Key(10),
                (success, error) =>
                {
                    if (success)
                        ScreenManager.Instance.Show_Warning(
                            "Purchase Successful!\n\n" + "Your purchased has been verified and the reward has been granted to your account!",
                            "Thanks!",
                            () => { ScreenManager.Instance.HideScreen(SCREEN_TYPE.SHOP); },
                            false);
                    else
                        ScreenManager.Instance.Show_Warning("Purchase failed!\n\n" + "Unable to verify your purchase. Please contact admin", "Okay");
                });
        }
    }

    [System.Serializable]
    public class IAP_PRODUCT
    {
        public string product_id;
        public int coins;
        public int price;

        [Space]
        public Sprite graphic;
        public bool is_popular;
    }
}