using UnityEngine;

namespace Global.Helpers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance { get => instance; }

        [Header("Singleton Property")]
        [SerializeField] bool isDontDestroy;


        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                if (isDontDestroy)
                    DontDestroyOnLoad(this.gameObject);
            }
            else
                Destroy(this.gameObject);
        }
    }
}