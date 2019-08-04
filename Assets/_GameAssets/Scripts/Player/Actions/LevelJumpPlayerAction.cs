using UnityEngine;



namespace SpeedyBoat
{
    public class LevelJumpPlayerAction : JumpPlayerAction
    {
        public override float JumpProgress
        {
            get { return 1f - (m_jumpTime / JumpTime); }
        }



        public override void Update()
        {
            m_jumpTime = Mathf.Max(0, m_jumpTime - Time.deltaTime);

            var progress = JumpProgress;
            var progress010 = Mathf.Sin(Mathf.PI * progress);

            var pos = Vector3.Lerp(m_startPos, LandPos, progress);

            pos.y = progress < .5f ? Mathf.Lerp(m_startPos.y, m_startPos.y + JumpHeight, progress010)
                                   : Mathf.Lerp(m_startPos.y + JumpHeight, LandPos.y, 1f - progress010);

            m_player.transform.position = pos;

            var xRotation = Mathf.Lerp(-LaunchAngle, 0, progress);
            m_player.BoatRotation = Quaternion.Euler(xRotation, 0, 0);

            if (m_jumpTime <= 0)
            {
                m_player.OnLevelJumpComplete();
                m_player.gameObject.SetActive(false);
            }
        }



        public LevelJumpPlayerAction(Player player, PlayerActionInitialiser initialiser)
            : base(player, initialiser)
        {
            m_startPos = m_player.transform.position;
            m_jumpTime = JumpTime;
        }



        private float JumpHeight
        {
            get { return ((LevelJumpPlayerActionInitialiser)m_initialiser).JumpHeight; }
        }



        private float JumpTime
        {
            get { return ((LevelJumpPlayerActionInitialiser)m_initialiser).JumpTime; }
        }



        private float JumpBounces
        {
            get { return ((LevelJumpPlayerActionInitialiser)m_initialiser).JumpBounces; }
        }



        private float LaunchAngle
        {
            get { return ((LevelJumpPlayerActionInitialiser)m_initialiser).LaunchAngle; }
        }



        private Vector3 LandPos
        {
            get { return ((LevelJumpPlayerActionInitialiser)m_initialiser).LandPos; }
        }



        private int     m_jumpBounceCount;
        private float   m_jumpTime;
        private Vector3 m_startPos;
    }
}