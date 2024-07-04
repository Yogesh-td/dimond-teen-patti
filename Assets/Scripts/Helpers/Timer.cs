using System;
using System.Collections;
using UnityEngine;

namespace Global.Helpers
{
    public static class Timer
    {
        public static void Schedule(MonoBehaviour behaviour, float delay, Action callback)
        {
            behaviour.StartCoroutine(DoTask(delay, callback));
        }
        static IEnumerator DoTask(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
    }
}