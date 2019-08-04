using UnityEngine;



namespace SpeedyBoat
{
    public class EnterPlayerAction : PlayerAction
    {
        public override void Update()
        {
            m_dropAccel += DropSpeed * Time.deltaTime;
            m_dropSpeed += m_dropAccel;
            m_player.Height = Mathf.Max(0, m_player.Height - m_dropSpeed * Time.deltaTime);

            if (m_player.Height <= 0)
            {
                m_player.ChangeAction(PlayerActionType.Drive);
            }
        }



        public EnterPlayerAction(Player player, PlayerActionInitialiser initialiser)
             : base(player, initialiser)
        {
            m_player.Velocity = 0;
            m_player.Height = DropHeight;
        }



        private float DropSpeed
        {
            get { return ((EnterPlayerActionInitialiser)m_initialiser).DropSpeed; }
        }



        private float DropHeight
        {
            get { return ((EnterPlayerActionInitialiser)m_initialiser).DropHeight; }
        }



        private float m_dropAccel, m_dropSpeed;
    }
}