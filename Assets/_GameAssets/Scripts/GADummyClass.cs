using UnityEngine;

/*  
    When creating a build for Kash, he will add INCLUDE_GA to the defines in build settings replacing this class with the real SDK

    Initialize() Is typically called from Awake on main script.
    StartSession() Is called afterwards or in Start() for example

    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "game"); Called when a new game starts
    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "game", m_game.Score); Called on Game Over

    EndSession() Is called typically OnApplicationQuit
 */



#if !INCLUDE_GA
namespace GameAnalyticsSDK
{
    public enum GAProgressionStatus
    {
        Start,
        Complete,
    }



    public static class GameAnalytics
    {
        public static void Initialize()
        {
            Debug.Log("GameAnalytics::Initialise");
        }



        public static void StartSession()
        {
            Debug.Log("GameAnalytics::StartSession");
        }



        public static void EndSession()
        {
            Debug.Log("GameAnalytics::EndSession");
        }



        public static void NewProgressionEvent(GAProgressionStatus status, string name, int score = -1)
        {
            var text = "GameAnalytics::NewProgressionEvent Status:" + status + " Name: " + name;
            if (score >= 0)
            {
                text += " Score:" + score;
            }

            Debug.Log(text);
        }
    }
}
#endif
