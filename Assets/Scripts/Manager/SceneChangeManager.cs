using Cysharp.Threading.Tasks;
using Global.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeenPatti
{
    public enum SCENES
    {
        Login,
        Menu,
        Game
    }
    public class SceneChangeManager : Singleton<SceneChangeManager>
    {
        [Space]
        [SerializeField] GameObject loading;


        #region Scene Handler
        public void ChangeScene(SCENES _stype, bool _showLoading)
        {
            ChangeSceneAsync(_stype, _showLoading).Forget();
        }
        async UniTask ChangeSceneAsync(SCENES _stype, bool _showLoading)
        {
            if (_showLoading)
                Show_Loading();

            await SceneManager.LoadSceneAsync(_stype.ToString());

            if (_showLoading)
                Hide_Loading();
        }
        #endregion


        #region Loading Handlers
        public void Show_Loading()
        {
            loading.SetActive(true);
        }
        public void Hide_Loading()
        {
            loading.SetActive(false);
        }
        #endregion
    }
}