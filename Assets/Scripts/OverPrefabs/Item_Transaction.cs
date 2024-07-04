using System;
using TeenPatti.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Item_Transaction : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txt_amount;
        [SerializeField] TextMeshProUGUI txt_status;
        [SerializeField] TextMeshProUGUI txt_msg;
        [SerializeField] TextMeshProUGUI txt_date;

        [Space]
        [SerializeField] Image img_transaction_type;

        [Space]
        [SerializeField] Sprite[] sprs_transaction_types;

        public void Initialize(PLAYER_TRANSACTION transaction_data)
        {
            txt_amount.text = string.Format("₹ {0}", (transaction_data.transaction_type.ToLower() == "cr" ? transaction_data.amount : -transaction_data.amount).ToString("n0"));
            txt_status.text = string.Format("Status: <color={0}>{1}", transaction_data.status.ToLower() == "pending" ? "orange" : "green", transaction_data.status);
            txt_msg.text = transaction_data.message;

            DateTime countryTime = TimeZoneInfo.ConvertTime(transaction_data.created_at, TimeZoneInfo.Local);
            txt_date.text = string.Format("Date: {0:00}-{1:00}-{2:00}, Time: {3:00}:{4:00}",
                countryTime.Day,
                countryTime.Month,
                countryTime.Year,
                countryTime.Hour,
                countryTime.Minute);

            img_transaction_type.sprite = transaction_data.transaction_type.ToLower() == "cr" ? sprs_transaction_types[0] : sprs_transaction_types[1];
        }
    }
}