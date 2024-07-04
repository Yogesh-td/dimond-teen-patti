using System;
using UnityEngine;

namespace TeenPatti.Helpers
{
    public static class General_Extention
    {
        public static string To_Color(this string value, Color color) => string.Format("<color=#{0}>{1}</color>", 
            ColorUtility.ToHtmlStringRGBA(color), 
            value);


        public static string To_KiloFormat(this int num)
        {
            if (num >= 100000000)
            {
                return (num / 1000000D).ToString("0.##M");
            }
            if (num >= 1000000)
            {
                return (num / 1000000D).ToString("0.##M");
            }
            if (num >= 100000)
            {
                return (num / 1000D).ToString("0.##k");
            }
            if (num >= 10000)
            {
                return (num / 1000D).ToString("0.##k");
            }

            return num.ToString("#,0");
        }


        public static long To_Unix_Timestamp(this DateTime time)
        {
            return (long)(time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }
    }
}