using UnityEngine;



namespace SpeedyBoat
{
    public class SinkPlayerAction : PlayerAction
    {
        public override void Update()
        {
            if (m_sinkNormal < 1)
            {
                m_player.UpdateMovement(false, true);

                m_sinkNormal = Mathf.Min(1, m_sinkNormal + Time.deltaTime);

                var pos = m_player.transform.position;
                pos.y -= m_sinkNormal * .5f;
                m_player.transform.position = pos;

                m_player.BoatRotation = Quaternion.RotateTowards(m_player.BoatRotation, Quaternion.identity, 360 * Time.deltaTime * 2);

                if (m_sinkNormal >= 1)
                {
                    m_player.OnDeath();
                }
            }
        }
    



        public SinkPlayerAction(Player player, PlayerActionInitialiser initialiser)
             : base(player, initialiser)
        {
            m_player.Game.PlaySound(SoundFXType.Sink);
            m_player.ShowBubbles(true);
        }



        private float m_sinkNormal;
    }
}