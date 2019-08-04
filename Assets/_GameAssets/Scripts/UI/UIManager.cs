using UnityEngine;
using System.Collections.Generic;



// Manages panels and whatever else is needed
namespace SpeedyBoat
{
    public enum PanelType
    {
        None = -1,
        Home,
        Game,
        GameOver,
        LevelComplete,
        Count
    }



    public class UIManager : MonoBehaviour
    {
        public Main   Main     { get; private set; }
        public Camera UICamera { get { return GetComponent<Canvas>().worldCamera; } }



        public void ShowHoldToFlip(bool show)
        {
            if (m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).ShowHoldToFlip(show);
            }
        }



        public void ShowLevelStart(bool show)
        {
            if (m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).ShowLevelStart(show);
            }
        }



        public void UpdateCoins(int count)
        {
            if(m_activePanel != null)
            {
                m_activePanel.UpdateCoins(count);
            }
        }



        public Vector3 CurrencyCoinPosition
        {
            get { return m_activePanel != null ? m_activePanel.CurrencyCoinPosition : Vector3.zero; }
        }



        public void ShowPickupCoin(Vector3 pos, Vector3 up, float scale, bool isSuper)
        {
            if (m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).ShowPickupCoin(pos, up, scale);
            }
        }



        public void Setup(Main main)
        {
            // Keep main handy as panels can often need to query dynamic game functions
            Main = main;

            StopAllCoroutines();

            // Create the panels from the heirachy, storing them in order of PanelType for indexing
            m_panels.Add(new HomeScreenPanel(this, transform.FindIncludingInactive("HomePanel")));
            m_panels.Add(new GameUIPanel(this, transform.FindIncludingInactive("GamePanel")));
            m_panels.Add(new GameOverPanel(this, transform.FindIncludingInactive("GameOverPanel")));
            m_panels.Add(new LevelCompletePanel(this, transform.FindIncludingInactive("LevelCompletePanel")));

            // Make sure they all start inactive as quite often they are left visible after modifying in Editor
            foreach (var panel in m_panels)
            {
                panel.Close();
            }
        }



        public void UpdatePlayerPositions(List<Player> orderedPlayers)
        {
            if (m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).UpdatePlayerPositions(orderedPlayers);
            }
        }



        public void ShowPerfect(bool show)
        {
            if(m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).ShowPerfect(show);
            }
        }



        // Closes current panel and opens new
        public void ShowPanel(PanelType panel, PanelInitialiser initialiser = null)
        {
            if (m_activePanel != null)
            {
                m_activePanel.Close();
                m_activePanel = null;
            }

            for (int i=0;i < m_panels.Count;++i)
            {
                var show = i == (int)panel;
                if(show)
                {
                    m_panels[i].Open(initialiser);
                    m_activePanel = m_panels[i];
                }
            }
        }



        // Typically called from the game loop
        public void UpdateScore(int score)
        {
            // Can check for active panel type or make functions virtual if multiple panels share functionality
            if (m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).UpdateScore(score);
            }
        }



        // Typically called from the game loop
        public void UpdateLevelProgress(int level, float progressNormal)
        {
            // Can check for active panel type or make functions virtual if multiple panels share functionality
            if (m_activePanel is GameUIPanel)
            {
                ((GameUIPanel)m_activePanel).UpdateLevelProgress(level, progressNormal);
            }
        }



        private void Update()
        {
            if (m_activePanel != null)
            {
                m_activePanel.Update();
            }
        }



        private UIPanel                 m_activePanel;
        private readonly List<UIPanel>  m_panels = new List<UIPanel>();
    }
}