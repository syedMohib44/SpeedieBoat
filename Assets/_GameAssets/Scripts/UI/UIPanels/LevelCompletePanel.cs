using TMPro;
using UnityEngine;



// This is typically the panel which is visible during gameplay
namespace SpeedyBoat
{
    public class LevelCompletePanel : UIPanel
    {
        public override void Open(PanelInitialiser initialiser)
        {
            base.Open(initialiser);

            // Do opening Stuff..

            var data = (LevelCompletePanelInitialiser)initialiser;

            m_coinsText.text = "<sprite=\"coin_icon\" index=0> " + data.Coins;

            m_bonusText.text = data.Bonus.ToString();
            m_positionTextLeft.text = data.Position.ToString();

            var postfix = "th";

            switch(data.Position)
            {
                case 1:
                    postfix = "st";
                    break;

                case 2:
                    postfix = "nd";
                    break;

                case 3:
                    postfix = "rd";
                    break;
            }

            m_positionTextRight.text = postfix;
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



        // t is the panels transform in the heirachy/prefab, variable is names as Unitys transform
        public LevelCompletePanel(UIManager uiManager, Transform t)
            : base(uiManager, t)
        {
            // Cache any objects that need to be updated
            m_coinsText = transform.FindIncludingInactive("Currency/Text").GetComponent<TMP_Text>();
            m_bonusText = transform.FindIncludingInactive("Bonus/Value").GetComponent<TMP_Text>();
            m_positionTextLeft = transform.FindIncludingInactive("Left").GetComponent<TMP_Text>();
            m_positionTextRight = transform.FindIncludingInactive("Right").GetComponent<TMP_Text>();
        }



        private TMP_Text m_coinsText, m_bonusText, m_positionTextLeft, m_positionTextRight;
    }
}