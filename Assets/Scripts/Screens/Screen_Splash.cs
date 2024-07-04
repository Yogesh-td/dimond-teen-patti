using Cysharp.Threading.Tasks;
using TeenPatti.App;
using TeenPatti.App.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Splash : Base_Screen
    {
        [Space]
        [SerializeField] Animator animator;

        [Header("Loading")]
        [SerializeField] Image img_loading_filler;
        [SerializeField] TextMeshProUGUI txt_loading_info;

        [Space]
        [SerializeField] float loading_fill_time;


        private void Start()
        {
            Initialize_App().Forget();
        }


        private async UniTaskVoid Initialize_App()
        {
            try
            {
                await Get_GameSettings(0, 50, "Validating App Infomation...");
                await Get_GameTables(50, 100, "Getting Game Data...");
                //await Get_GameIaps(66, 100, "getting in-app-purchase data...");

                ScreenManager.Instance.ShowScreen(SCREEN_TYPE.LOGIN);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                ScreenManager.Instance.Show_Warning("App Initialize error:\n" + e.Message, "Re-Try", Start, false);
            }
        }
        private async UniTask Get_GameSettings(int startLoading, int endLoading, string message)
        {
            Update_Filler(startLoading);
            Update_Loading_Text(message);

            bool taskCompleted = false;
            bool taskSuccess = false;
            string taskerror = "";
            AppManager.Instance.Get_GameSettings((success, error) =>
            {
                taskSuccess = success;
                taskerror = error;

                taskCompleted = true;
            });

            while (!taskCompleted)
                await UniTask.Delay(100);

            if (!taskSuccess)
                throw new System.Exception(taskerror);
            else
            {
                if (AppManager.Instance.GameSettings.app_version != Application.version)
                    throw new System.Exception("Failed to initialize app!\n\nInvalid app version! Please contact adminstrator!");
                else
                {
                    Update_Loading_Filler(endLoading);
                    await UniTask.Delay((int)(loading_fill_time * 1200));
                }
            }
        }
        private async UniTask Get_GameTables(int startLoading, int endLoading, string message)
        {
            Update_Filler(startLoading);
            Update_Loading_Text(message);

            bool taskCompleted = false;
            bool taskSuccess = false;
            string taskerror = "";
            AppManager.Instance.Get_GameTables((success, error) =>
            {
                taskSuccess = success;
                taskerror = error;

                taskCompleted = true;
            });

            while (!taskCompleted)
                await UniTask.Delay(100);

            if (!taskSuccess)
                throw new System.Exception(taskerror);
            else
            {
                Update_Loading_Filler(endLoading);
                await UniTask.Delay((int)(loading_fill_time * 1200));
            }
        }
        private async UniTask Get_GameIaps(int startLoading, int endLoading, string message)
        {
            Update_Filler(startLoading);
            Update_Loading_Text(message);

            bool taskCompleted = false;
            bool taskSuccess = false;
            string taskerror = "";
            AppManager.Instance.Get_GameIaps((success, error) =>
            {
                taskSuccess = success;
                taskerror = error;

                taskCompleted = true;
            });

            while (!taskCompleted)
                await UniTask.Delay(100);

            if (!taskSuccess)
                throw new System.Exception(taskerror);
            else
            {
                Update_Loading_Filler(endLoading);
                await UniTask.Delay((int)(loading_fill_time * 1200));
            }
        }


        private void Update_Loading_Filler(int value)
        {
            if (iTween.Count(this.gameObject) > 0)
                iTween.Stop(this.gameObject);

            iTween.ValueTo(this.gameObject, iTween.Hash
                (
                    "from", img_loading_filler.fillAmount * 100,
                    "to", value,
                    "time", loading_fill_time,
                    "easetype", iTween.EaseType.linear,
                    "onupdate", nameof(Update_Filler)
                ));
        }
        private void Update_Filler(int value)
        {
            img_loading_filler.fillAmount = Mathf.Clamp(value / 100f, 0f, 1f);
        }
        private void Update_Loading_Text(string value)
        {
            txt_loading_info.text = value;
        }



        public void Toggle_IsSmall(bool value)
        {
            animator.speed = 1f / CoreSettings.Instance.Screen_SwitchTime;
            animator.SetBool("issmall", value);

            img_loading_filler.transform.parent.gameObject.SetActive(!value);
        }
    }
}