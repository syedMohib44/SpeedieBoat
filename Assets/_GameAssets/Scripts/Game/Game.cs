using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SpeedyBoat
{
    public class Game : MonoBehaviour
    {
        public Main         Main        { get; private set; }
        public GameMode     Mode        { get; private set; }
        public int          Score       { get; private set; }
        public Player       Player      { get; private set; }
        public Level        Level       { get; private set; }
        public Level        NextLevel   { get; private set; }
        public bool         isFirst     { get; private set; }

        //Material            groundMat;
        private int levelNo = 1;
        private float Red, Green, Blue;
        MeshRenderer        mat;

        public void OnHomeButton()
        {
            Main.OnGameOver(true);
        }

        public void OnPlayerDeath(Player player)
        {
            if (player is HumanPlayer)
            {
                Main.OnGameOver(false);
            }
        }



        public void OnAttractedCoinsCollected(int count)
        {
            Main.AddCoins(count);
        }



        public void OnPickupCollision(Pickup pickup)
        {
            switch(pickup.Type)
            {
                case PickupType.CoinPickup:
                    Main.UIManager.ShowPickupCoin(pickup.transform.position, pickup.transform.up, 1, false);
                    PlaySound(SoundFXType.Coin);
                    break;

                default:
                    Debug.LogError("OnPickupCollision did not handle type " + pickup.Type);
                    break;
            }

            Level.OnPickupCollected(pickup);
        }



        public void OnPerfectCurve()
        {
            Player.UpgradeBoat(GameSettings.Instance().BoatModels[1]);
            Main.UIManager.ShowPerfect(true);
        }



        public void PlaySound(SoundFXType sound)
        {
            Camera.main.GetComponent<AudioSource>().PlayOneShot(GameSettings.Instance().SoundFX[(int)sound]);
        }



        public bool IsShowingBonusEffect
        {
            get { return m_bonusEffects.ActiveObjects.Count > 0; }
        }



        public void ShowHoldFlip()
        {
            StartCoroutine("ShowHoldFlipForTime", 1);
        }



        private IEnumerator ShowHoldFlipForTime(float time)
        {
            Main.UIManager.ShowHoldToFlip(true);

            Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(time);

            Main.PlayerData.ShownHoldFlip = true;

            Time.timeScale = 1;

            Main.UIManager.ShowHoldToFlip(false);
        }



        public void ApplyFlipBonus(Vector3 pos, int flipCount)
        {
            var score = GameSettings.Instance().FlipBonusBaseScore * flipCount;

            var go = m_bonusEffects.AllocateObject();
            if (go)
            {
                go.GetComponent<FlipBonus>().Setup(Player, score, flipCount, m_bonusEffects);
            }

            Main.AddCoins(score);

            PlaySound(SoundFXType.Flip);
        }



        public void ShowSmokePuff(Vector3 pos, float scale = 1, bool showStars = false)
        {
            var go = m_smokePuffs.AllocateObject();
            if(go)
            {
                go.transform.parent = Player.transform;
                go.transform.localPosition = Vector3.zero;
                go.GetComponent<ParticleEffect>().Setup(pos, scale, showStars, (puffGO) => m_smokePuffs.FreeObject(puffGO));
            }
        }



        public void Initialise()
        {
            m_levelFolder = transform.FindIncludingInactive("CurrentLevel");
            while (m_levelFolder.childCount > 0)
            {
                var child = m_levelFolder.GetChild(0);
                child.parent = null;
                Destroy(child.gameObject);
            }

            m_nextLevelFolder = transform.FindIncludingInactive("NextLevel");
            while (m_nextLevelFolder.childCount > 0)
            {
                var child = m_nextLevelFolder.GetChild(0);
                child.parent = null;
                Destroy(child.gameObject);
            }
        }



        public void Setup(Main main, int level, bool demoMode, int startScore = 0)
        {

            GameAnalytics.Initialize();

            Main = main;
            Score = startScore;

            GetComponent<AudioSource>().Play();

            m_cameraManager = Camera.main.GetComponent<CameraManager>();

            SetupObjectPools();
            CreateLevelScene(level);

            if (Player == null)
            {
                Player = Instantiate(GameSettings.Instance().Player.Prefab).GetComponent<Player>();
                Player.transform.parent = transform;
                Player.gameObject.name = "Player";
            }
            Player.Setup(this, GameSettings.Instance().BoatModels[0], GameSettings.Instance().PlayerColors[0], 0, 0);

            CreateAIPlayers();

            m_orderedPlayers = new List<Player> { Player };
            foreach (var aiPlayer in m_aiPlayers.ActiveObjects)
            {
                m_orderedPlayers.Add(aiPlayer.GetComponent<Player>());
            }

            Main.UIManager.ShowPerfect(false);
            Main.UIManager.UpdateCoins(Main.PlayerData.Coins);

            ChangeMode(demoMode ? GameMode.Demo : GameMode.LevelStart);

            m_cameraManager.TrackPlayer(Player, true);

            Debug.Log(gameObject.transform.Find("CurrentLevel").transform.Find("Ground"));

            if (levelNo > 4)
            {
                gameObject.transform.Find("CurrentLevel").transform.Find("Level" + levelNo).transform.Find("Ground").transform.Find("popUps").gameObject.SetActive(false);
            }
            else
                gameObject.transform.Find("CurrentLevel").transform.Find("Level" + levelNo).transform.Find("Ground").transform.Find("popUps").gameObject.SetActive(true);

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, Application.version, levelNo);

        }


        // Not all games need this but Gav has included it in the mocks, called from a button event on the TestLevelComplete button in the GamePanel
        public void OnLevelComplete()
        {
            ChangeMode(GameMode.LevelComplete);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, Application.version, levelNo);
        }



        public void PlayNextLevel()
        {
            // Update the level progress and setup for the next level
            Setup(Main, ++Main.PlayerData.LevelProgress, false, Score);
            levelNo++;

            Interstitial.Instance().ShowIntestitial(levelNo);
            setNextLevelMap();
        }

        //Updated by Syed Mohib...
        public void setNextLevelMap()
        {
            //Changing map...
            Red = Random.Range(0.0f, 1f);
            Green = Random.Range(0.0f, 1f);
            Blue = Random.Range(0.0f, 1f);
            mat = gameObject.transform.FindIncludingInactive("CurrentLevel").transform.Find("Level1").transform.Find("Ground").GetComponent<MeshRenderer>();

            //name of Material Gameobject...
            mat.gameObject.name = "Ground";



            mat.material.color = new Color(Red, Green, Blue, 1);
        }
        //Till here...

        // Late so its post all movement
        private void LateUpdate()
        {
            m_orderedPlayers.Sort((a, b) => { return a.TravelDist > b.TravelDist ? -1 : a.TravelDist < b.TravelDist ? 1 : 0; });
            Main.UIManager.UpdatePlayerPositions(m_orderedPlayers);

            for (int i = 0; i < m_orderedPlayers.Count; ++i)
            {
                var player = m_orderedPlayers[i];
                player.Position = i;
                player.ShowCrown(i == 0);
            }
        }



        private void Update()
        {
#if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = (int)Time.timeScale ^ 1;
            }
#endif
            // Game update, eventually calling Main.OnGameOver when complete.
            // For testing there is a button on the GamePanel in the heirachy which will call Main.OnGameOver when pressed

            switch (Mode)
            {
                case GameMode.GameOver:
                    return;

                case GameMode.LevelStart:
                    m_levelStartTime -= Time.deltaTime;
                    if(m_levelStartTime <= 0)
                    {
                        ChangeMode(GameMode.GamePlay);
                    }
                    break;
            }

            Main.UIManager.UpdateScore(Score);

            // Test update for the level progress display
            var level = Main.PlayerData.LevelProgress;
            var progress = Mathf.Clamp(Player.TravelDist, 0, Level.Track.TrackLength) / Level.Track.TrackLength;
            Main.UIManager.UpdateLevelProgress(level, progress);
        }



        private void CreateLevelScene(int index)
        {
            var levels = GameSettings.Instance().Levels;
            var levelDetails = levels[index % levels.Length];

            // Always destroy the previous level
            if (Level != null)
            {
                Level.Destroy();

                Level.transform.parent = null;
                Destroy(Level.gameObject);
            }

            // Only destroy the next level if we are starting a new game or the level we are moving to is not the one set up
            var next = m_nextLevelFolder.childCount > 0 ? m_nextLevelFolder.GetChild(0).GetComponent<Level>() : null;
            //if (next && (index == 0 || (next.Index != index)))

            // Always destroying right now due to strange curve meshes bug
            if(next)
            {
                next.Destroy();

                next.transform.parent = null;
                Destroy(next.gameObject);

                next = null;
            }

            // If the next is already set up move it to the current levels folder
            if(next)
            {
                next.transform.parent = m_levelFolder;
                Level = next;

                next = null;
            }
            // Otherwise create the current level in situ
            else
            {
                Level = Instantiate(levelDetails.ScenePrefab).GetComponent<Level>();
                Level.transform.parent = m_levelFolder;
                
                Level.name = levelDetails.ScenePrefab.name;
                Level.Setup(index, this);
            }

            Level.SetActive(true);

            // Set the next level up so its visible during the current level as we need to jump to it at the end
            if (next == null)
            {
                var nextIndex = index + 1;
                var nextLevelDetails = levels[nextIndex % levels.Length];

                NextLevel = Instantiate(nextLevelDetails.ScenePrefab).GetComponent<Level>();
                NextLevel.transform.parent = m_nextLevelFolder;
                NextLevel.name = nextLevelDetails.ScenePrefab.name;

                NextLevel.Setup(nextIndex, this, Level);
            }
    
        }



        private void CreateAIPlayers()
        {
            var colors = new List<Color>(GameSettings.Instance().PlayerColors);
            colors.RemoveAt(0); // Human uses 0

            for (int i = 0; i < Level.AIPlayerCount; ++i)
            {
                var go = m_aiPlayers.AllocateObject();
                if (go)
                {
                    var column = i & 1;
                    var row = -(i + 1);

                    var lateralStart = -.5f + column;
                    var distStart = Level.TrackData.StartLineDist + row;

                    var colorIndex = Random.Range(0, colors.Count);
                    var color = colors[colorIndex];
                    colors.RemoveAt(colorIndex);

                    go.GetComponent<AIPlayer>().Setup(this, GameSettings.Instance().BoatModels[0], color, distStart, lateralStart);
                }
            }
        }



        private void SetupObjectPools()
        {
            if (m_aiPlayers == null)
            {
                var poolParent = new GameObject("AIPlayers").transform;
                poolParent.parent = transform;
                m_aiPlayers = new ObjectPool("AIPlayers", GameSettings.Instance().AIPlayerPrefab, poolParent, GameSettings.Instance().MaxAIPlayers);
            }
            else
            {
                m_aiPlayers.Reset();
            }

            if (m_smokePuffs == null)
            {
                var poolParent = new GameObject("SmokePuffs").transform;
                poolParent.parent = transform;
                m_smokePuffs = new ObjectPool("SmokePuffs", GameSettings.Instance().SmokePuffPrefab, poolParent, GameSettings.Instance().MaxSmokePuffs);
            }
            else
            {
                m_smokePuffs.Reset();
            }

            if (m_bonusEffects == null)
            {
                var poolParent = new GameObject("BonusEffects").transform;
                poolParent.parent = transform;
                m_bonusEffects = new ObjectPool("BonusEffects", GameSettings.Instance().BonusEffectPrefab, poolParent, GameSettings.Instance().MaxBonusEffects);
            }
            else
            {
                m_bonusEffects.Reset();
            }
        }



        private void ChangeMode(GameMode mode)
        {
            switch(mode)
            {
                case GameMode.LevelStart:
                    m_cameraManager.StopLooking();
                    Level.OnLevelStart();

                    Main.UIManager.ShowPanel(PanelType.Game);
                    Main.UIManager.ShowPerfect(false);
                    Main.UIManager.ShowLevelStart(true);

                    m_levelStartTime = 3;
                    break;

                case GameMode.GamePlay:
                    Main.UIManager.ShowPanel(PanelType.Game);
                    Main.UIManager.ShowPerfect(false);
                    Main.UIManager.ShowLevelStart(false);
                    break;

                case GameMode.LevelComplete:

                    m_cameraManager.LookAt(Level.LevelEndFocalPoint, .5f);
                    Level.OnLevelComplete();

                    m_cameraManager.TrackPlayer(null);

                    var bonuses = GameSettings.Instance().PositionBonuses;
                    var bonus = Player.Position < bonuses.Length ? bonuses[Player.Position] : 0;
                    Main.AddCoins(bonus);

                    Main.UIManager.ShowPanel(PanelType.LevelComplete, new LevelCompletePanelInitialiser(Main.PlayerData.Coins, bonus, Player.Position));
                    break;
            }

            Mode = mode;
        }



        public enum GameMode
        {
            None = -1,
            Demo,
            LevelStart,
            GamePlay,
            LevelComplete,
            GameOver,
            Count
        }



        private AudioSource     m_gameSFX;
        private CameraManager   m_cameraManager;
        private List<Player>    m_orderedPlayers;

        private float           m_levelStartTime;
        private Transform       m_levelFolder, m_nextLevelFolder;
        private ObjectPool      m_aiPlayers, m_smokePuffs, m_bonusEffects;
    }
}
