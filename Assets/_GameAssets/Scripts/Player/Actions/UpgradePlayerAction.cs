using UnityEngine;



namespace SpeedyBoat
{
    public class UpgradePlayerAction : PlayerAction
    {
        public override void Update()
        {
            m_player.UpdateMovement(false);
            m_player.UpdateLateralPos();

            m_dropAccel += DropSpeed * Time.deltaTime;
            m_dropSpeed += m_dropAccel;
            m_player.Height = Mathf.Max(0, m_player.Height - m_dropSpeed * Time.deltaTime);

            if (m_player.Height <= 0)
            {
                m_player.ChangeAction(PlayerActionType.Drive);
                m_player.Boost(BoostType.Upgrade);
            }
        }



        public UpgradePlayerAction(Player player, PlayerActionInitialiser initialiser)
             : base(player, initialiser)
        {
            m_player.ShowSmokePuff(2, true);
            m_player.Height = DropHeight;
        }



        private float DropSpeed
        {
            get { return ((UpgradePlayerActionInitialiser)m_initialiser).DropSpeed; }
        }



        private float DropHeight
        {
            get { return ((UpgradePlayerActionInitialiser)m_initialiser).DropSpeed; }
        }



        private float m_dropAccel, m_dropSpeed;
    }
}