#if !INCLUDE_FB
using System;
using System.Collections.Generic;
using UnityEngine;



namespace Facebook.Unity
{
    public static class AppEventName
    {
        public const string AchievedLevel = "fb_mobile_level_achieved";
    }



    public static class AppEventParameterName
    {
        public const string Level = "fb_level";
    }



    public static class FB
    {
        public static bool IsInitialized;




        public static void ActivateApp()
        {
        }



        public static void Init(Action action)
        {
        }



        public static void LogAppEvent(string name, float? unused, Dictionary<string, object> parameters)
        {
            Debug.Log("<FB> Would log Event " + name);

            foreach (var data in parameters)
            {
                Debug.Log("<FB Event Data> " + data.Key + " - " + (string)data.Value);
            }
        }
    }
}
#endif