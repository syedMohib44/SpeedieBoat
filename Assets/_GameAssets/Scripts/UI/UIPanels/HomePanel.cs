using TMPro;
using UnityEngine;



// This is typically the panel which is visible as you enter the app
namespace SpeedyBoat
{
    public class HomeScreenPanel : UIPanel
    {
        public override void Open(PanelInitialiser initialiser)
        {
            base.Open(initialiser);

            // Do opening Stuff..
            var setupData = (HomePanelInitialiser)initialiser;
            m_bestScore.text = setupData.BestScore;
        }



        public override void Close()
        {
            base.Close();

            // Do cleanup Stuff..
        }



        public override void Update()
        {
            base.Update();

            // Do any polling updates
        }



        public HomeScreenPanel(UIManager uiManager, Transform t)
            : base(uiManager, t)
        {
            // Cache any objects that need to be updated
            m_bestScore = transform.FindIncludingInactive("BestScore").GetComponent<TMP_Text>();
        }



        private readonly TMP_Text m_bestScore;
    }
}