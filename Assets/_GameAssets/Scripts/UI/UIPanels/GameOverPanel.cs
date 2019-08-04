using TMPro;
using UnityEngine;



// This is typically the panel which is visible at the games end which has a button able to send it back to the home screen
namespace SpeedyBoat
{
    public class GameOverPanel : UIPanel
    {
        public override void Open(PanelInitialiser initialiser)
        {
            base.Open(initialiser);

            // Do opening Stuff..
            var setupData = (GameOverPanelInitialiser)initialiser;
            m_score.text = setupData.Score;
            m_bestScore.text = setupData.BestScore;
        }



        public GameOverPanel(UIManager uiManager, Transform t)
            : base(uiManager, t)
        {
            // Cache any objects that need to be updated
            m_score = transform.FindIncludingInactive("Score").GetComponent<TMP_Text>();
            m_bestScore = transform.FindIncludingInactive("BestScore").GetComponent<TMP_Text>();
        }



        private TMP_Text m_score, m_bestScore;
    }
}