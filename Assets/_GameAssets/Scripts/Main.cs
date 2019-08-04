using Facebook.Unity;
using GameAnalyticsSDK;
using System.Collections.Generic;
using UnityEngine;



// Main script which naviagtes between the UI & Game areas
namespace SpeedyBoat
{
    public class Main : MonoBehaviour
    {
        // As the PlayerData evolves it needs to be wiped so it can be parsed correctly
#if UNITY_EDITOR
        public bool NoDeath;
        public bool WipePlayerData;

        public int LevelStart;
#endif


        public Game       Game       { get; private set; }
        public UIManager  UIManager  { get; private set; }
        public PlayerData PlayerData { get; private set; }



        public void AddCoins(int amount)
        {
            PlayerData.Coins += amount;
            if(PlayerData.Coins > PlayerData.BestScore)
            {
                PlayerData.BestScore = PlayerData.Coins;
            }

            UIManager.UpdateCoins(PlayerData.Coins);
        }



        // A UI Button on the HomeScreen panel in the heirachy would typically be set up with this event
        public void PlayGame()
        {
            StartGame(false);
        }



        // Typically called from Game once the game is complete
        public void OnGameOver(bool quit)
        {
            // Update the best score
            if(Game.Score > PlayerData.BestScore)
            {
                PlayerData.BestScore = Game.Score;
            }

            if (!quit)
            {
                UIManager.ShowPanel(PanelType.GameOver, new GameOverPanelInitialiser(Game.Score, PlayerData.BestScore));
            }
            else
            {
                ShowHomScreen();
            }
            ;
            Interstitial.Instance().ShowIntestitial();

            // Note: "game" is the actual string required here not the name of the game
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "game", Game.Score);
        }



        // A UI Button on the GameOver panel in the heirachy would typically be set up with this event
        public void ShowHomScreen()
        {
            StartGame(true);
        }



        private void StartGame(bool demoMode)
        {
            if(demoMode)
            {
                // Show the home panel whilst the game plays a demo-mode in the background
                UIManager.ShowPanel(PanelType.Home, new HomePanelInitialiser(PlayerData.BestScore));
            }
            else
            {
                UIManager.ShowPanel(PanelType.Game);

                // Note: "game" is the actual string required here not the name of the game
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "game");
            }

            Game.Setup(this, PlayerData.LevelProgress, demoMode);
        }



        private void Awake()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                //Handle FB.Init
                FB.Init(() => { FB.ActivateApp(); });
            }

            AppsFlyer.setAppsFlyerKey(GameSettings.Instance().AppsflyerDevKey);
            AppsFlyer.setAppID(GameSettings.Instance().ItunesAppID);
            AppsFlyer.trackAppLaunch();

            var testObjects = GameObject.Find("TestObjects");
            if(testObjects)
            {
                testObjects.SetActive(false);
            }

            //GetComponentInChildren<Light>().shadowCustomResolution = 8192;

            GameAnalytics.Initialize();

            // Destroy out of date data
#if UNITY_EDITOR
            if(WipePlayerData)
            {
                PlayerPrefs.DeleteAll();
            }
#endif

            // Load the player data from player prefs
            PlayerData = new PlayerData(this);

#if UNITY_EDITOR
            if(LevelStart > 0)
            {
                PlayerData.LevelProgress = LevelStart;
            }
#endif

            AudioListener.volume = PlayerData.SoundOn ? 1 : 0;

            // Usually set up in the heirachy as the UI folder
            UIManager = GetComponentInChildren<UIManager>(true);
            UIManager.Setup(this);

            // Usually set up in the heirachy as the games' folder, I parent all dynamic game objects here
            Game = GetComponentInChildren<Game>(true);
            Game.Initialise();

            // Start the game in demo mode
            StartGame(true);
        }
        


        private void Start()
        {
            GameAnalytics.StartSession();
        }



        private void OnApplicationPause(bool pause)
        {
            PlayerData.Serialize();
        }



        private void OnApplicationQuit()
        {
            GameAnalytics.EndSession();
            PlayerData.Serialize();

            var p = new Dictionary<string, object>();
            p[AppEventParameterName.Level] = PlayerData.LevelProgress.ToString();
            FB.LogAppEvent(AppEventName.AchievedLevel, null, p);
        }
    }
}



