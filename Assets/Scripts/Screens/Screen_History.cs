using System.Linq;
using TeenPatti.App;
using TeenPatti.Audios;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_History : Base_Screen
    {
        [Space]
        [SerializeField] Item_Transaction transaction_prefab_prefab;
        [SerializeField] Transform transaction_parent;

        [Space]
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_close.onClick.AddListener(OnClick_Close);
        }

        public override void Show()
        {
            SceneChangeManager.Instance.Show_Loading();

            ApiManager.Instance.Api_GetHistory(
            (transactions) =>
            {
                SceneChangeManager.Instance.Hide_Loading();
                foreach (var transaction in transactions.OrderByDescending(x => x.created_at))
                {
                    Item_Transaction item = Instantiate(transaction_prefab_prefab, transaction_parent);
                    item.Initialize(transaction);
                }
                base.Show();
                transaction_parent.localPosition = new Vector3(transaction_parent.localPosition.x, 0);
            },
            error =>
            {
                SceneChangeManager.Instance.Hide_Loading();
                ScreenManager.Instance.Show_Warning(error, "Got it!");
                OnClick_Close();
            });
        }
        public override void Hide()
        {
            for (int i = 0; i < transaction_parent.childCount; i++)
                Destroy(transaction_parent.GetChild(i).gameObject);

            base.Hide();
        }

        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}