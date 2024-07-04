using UnityEngine;

namespace Global.Helpers
{
    public class SingletonScriptable<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<T>("Singletons/" + typeof(T).Name);
                    (instance as SingletonScriptable<T>).OnInitialize();
                }
                return instance;
            }
        }

        protected virtual void OnInitialize()
        {
        }
    }
}