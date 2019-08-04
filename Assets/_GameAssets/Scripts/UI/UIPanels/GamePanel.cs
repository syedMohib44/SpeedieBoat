using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



// This is typically the panel which is visible during gameplay
namespace SpeedyBoat
{
    public class GameUIPanel : UIPanel
    {
        public override void Open(PanelInitialiser initialiser)
        {
            base.Open(initialiser);

            // Do opening Stuff..
        }



        public override void Close()
        {
            base.Close();

            // Do cleanup Stuff..
        }



        public override Vector3 CurrencyCoinPosition
        {
            get { return m_coinsText.transform.position; }
        }



        public override void UpdateCoins(int count)
        {
            m_coinsText.text = "<sprite=\"coin_icon\" index=0> " + count;
        }



        public void ShowHoldToFlip(bool show)
        {
            m_holdToFlipPanel.gameObject.SetActive(show);
        }



        public void ShowLevelStart(bool show)
        {
            m_levelStartPanel.gameObject.SetActive(show);
        }



        public void ShowPickupCoin(Vector3 pos, Vector3 up, float scale)
        {
            var screenPos = Camera.main.WorldToScreenPoint(pos);
            screenPos.z = m_uiManager.GetComponent<Canvas>().transform.position.z;

            var uiPos = m_uiManager.UICamera.ScreenToWorldPoint(screenPos);

            m_pickupCoinsEffect.transform.position = uiPos;
            m_pickupCoinsEffect.transform.up = up;

            m_pickupCoinsEffect.Setup(m_uiManager.Main.Game, scale);
            m_pickupCoinsEffect.GetComponent<ParticleSystem>().Emit(1);
        }



        public void ShowPerfect(bool show)
        {
            m_perfectPanel.gameObject.SetActive(show);
        }



        // Typically called from the game loop via the UIManager
        public void UpdateScore(int score)
        {
            var scoreText = score.ToString();
            m_scoreText.text = scoreText;
        }



        // Typically called from the game loop via the UIManager
        public void UpdateLevelProgress(int level, float progressNormal)
        {
            m_progressThisLevel.text = (level + 1).ToString();
            m_progressNextLevel.text = (level + 2).ToString();

            m_progressBar.fillAmount = progressNormal;
        }



        public void UpdatePlayerPositions(List<Player> orderedPlayers)
        {
            int i = 0;
            for (;i < orderedPlayers.Count;++i)
            {
                var player = orderedPlayers[i];
                var position = m_playerPositions.GetChild(i);

                var show = i > 0 && player.ShowPosition;
                position.gameObject.SetActive(show);

                if (show)
                {
                    position.transform.position =
                        m_uiManager.UICamera.ScreenToWorldPoint(Camera.main.WorldToScreenPoint(player.PositionTextPos));

                    var tmpText = position.GetComponent<TMP_Text>();
                    tmpText.text = (i + 1).ToString();
                    tmpText.color = player.PrimaryColor * 2;
                }
            }

            for(;i < m_playerPositions.childCount;++i)
            {
                var position = m_playerPositions.GetChild(i);
                position.gameObject.SetActive(false);
            }
        }



        public override void Update()
        {
            base.Update();

            // Do any polling updates
        }



        // t is the panels transform in the heirachy/prefab, variable is named as Unitys transform so it looks and is used like a game object
        public GameUIPanel(UIManager uiManager, Transform t)
            : base(uiManager, t)
        {
            // Cache any objects that need to be updated
            m_scoreText = transform.FindIncludingInactive("Score").GetComponent<TMP_Text>();

            var levelProgress = transform.FindIncludingInactive("LevelProgress");
            m_progressBar = levelProgress.FindIncludingInactive("BarFill").GetComponent<Image>();
            m_progressThisLevel = levelProgress.transform.FindIncludingInactive("Left/Text").GetComponent<TMP_Text>();
            m_progressNextLevel = levelProgress.transform.FindIncludingInactive("Right/Text").GetComponent<TMP_Text>();

            m_holdToFlipPanel = transform.FindIncludingInactive("HoldFlipPanel");
            m_holdToFlipPanel.gameObject.SetActive(false);

            m_perfectPanel = transform.FindIncludingInactive("PerfectPanel");
            m_perfectPanel.gameObject.SetActive(false);

            m_playerPositions = transform.FindIncludingInactive("PlayerPositions");
            m_pickupCoinsEffect = m_uiManager.transform.FindIncludingInactive("PickupCoinsEffect").GetComponent<PickupAttractedCoinsEffect>();

            m_coinsText = transform.FindIncludingInactive("Currency/Text").GetComponent<TMP_Text>();
            m_levelStartPanel = transform.FindIncludingInactive("LevelStartPanel");
        }



        private Image       m_progressBar;
        private Transform   m_perfectPanel, m_playerPositions, m_levelStartPanel, m_holdToFlipPanel;
        private TMP_Text    m_scoreText, m_progressThisLevel, m_progressNextLevel, m_coinsText;

        private PickupAttractedCoinsEffect m_pickupCoinsEffect;
    }
}