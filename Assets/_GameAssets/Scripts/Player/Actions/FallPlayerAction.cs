using UnityEngine;



namespace SpeedyBoat
{
    public class FallPlayerAction : PlayerAction
    {
        public override void Update()
        {
            var pos = m_player.transform.position;
            var oldY = pos.y;

            m_fallVel.x = Mathf.MoveTowards(m_fallVel.x, 0, Time.deltaTime * .1f);
            m_fallVel.y -= Gravity * Time.deltaTime;
            m_fallVel.z = Mathf.MoveTowards(m_fallVel.z, 0, Time.deltaTime * .1f);

            pos += m_fallVel * Time.deltaTime;
            pos.y = Mathf.Max(GroundHeight, pos.y);

            m_player.transform.position = pos;

            if (oldY > GroundHeight && pos.y <= GroundHeight)
            {
                m_fallVel = Vector3.zero;
                m_player.Game.PlaySound(SoundFXType.Smash);

                m_player.OnDeath(true);
            }
        }



        public FallPlayerAction(Player player, PlayerActionInitialiser initialiser)
             : base(player, initialiser)
        {
            m_fallVel = m_player.WorldVelocity * 10 + LateralNormal * LateralPushVel + Vector3.up * HopVel;
        }



        private float GroundHeight
        {
            get { return ((FallPlayerActionInitialiser)m_initialiser).GroundHeight; }
        }



        private Vector3 LateralNormal
        {
            get { return ((FallPlayerActionInitialiser)m_initialiser).LateralNormal; }
        }



        private const float HopVel          = 4;
        private const float LateralPushVel  = .1f;
        private const float Gravity         = 10;

        private Vector3     m_fallVel;
    }
}