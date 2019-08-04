using UnityEngine;



namespace SpeedyBoat
{
    public class FlipJumpPlayerAction : JumpPlayerAction
    {
        public override float JumpProgress
        {
            get { return 1f - (m_jumpTime / m_jumpTimeStart); }
        }



        public override void Update()
        {
            m_player.UpdateMovement(m_jumpBounceCount > 0, false);

            var canFlip = FlipTime > 0 && m_jumpBounceCount <= 0;
            var isFlipping = m_flipTime > 0;

            if (canFlip && !isFlipping && (Input.GetMouseButton(0) || m_flipCount <= 0))
            {
                m_flipTime = FlipTime;
            }

            m_jumpTime = Mathf.Max(0, m_jumpTime - Time.deltaTime);

            var progress = JumpProgress;
            var progress010 = Mathf.Sin(Mathf.PI * progress);

            var pos = m_player.transform.position;

            // Bringing the height down over the first jumps arc to match the tracks y position
            var yOffset = (m_launchHeight - m_player.TrackPos.y) * (m_jumpBounceCount <= 0 ? (1f - progress) : 0);
            pos.y = m_player.TrackPos.y + yOffset + m_jumpHeight * progress010;
            m_player.transform.position = pos;

            var xrot = m_player.transform.localEulerAngles.x;

            var xRotation = -LaunchAngle;
            if (m_flipTime > 0)
            {
                var flipProgress = Mathf.Sin(Mathf.PI * .5f * (1f - (m_flipTime / FlipTime)));
                xRotation = -LaunchAngle - flipProgress * 360;

                m_flipTime = Mathf.Max(0, m_flipTime - Time.deltaTime);

                if(m_flipTime <= 0)
                {
                    m_player.OnFlipped(++m_flipCount);
                }
            }
            else
            {
                xRotation = 0;
            }

            // Player levels out over the jump becoming level at the apex
            var playerRotation = m_player.transform.localRotation;
            var targetRotation = Quaternion.Euler(0, playerRotation.eulerAngles.y, playerRotation.eulerAngles.z);
            playerRotation = Quaternion.Lerp(playerRotation, targetRotation, Mathf.Clamp01(m_jumpBounceCount + progress * 2));
            m_player.transform.localRotation = playerRotation;

            // Internal boat model rotation applied for the Flip
            m_player.BoatRotation = Quaternion.Euler(xRotation, 0, 0);

            if (m_jumpTime <= 0)
            {
                /*
                if (Mathf.Abs(Mathf.DeltaAngle(0, xRotation)) > 15)
                {
                    m_player.ChangeAction(PlayerActionType.Sink, new SinkPlayerActionInitialiser(2));
                }
                else
                */
                {
                    if (Mathf.Abs(Mathf.DeltaAngle(0, xRotation)) <= 15 && (++m_jumpBounceCount <= JumpBounces))
                    {
                        m_jumpTime = m_jumpTimeStart = JumpTime * JumpScale * .075f;
                        m_jumpHeight = JumpHeight * JumpScale * .075f;
                    }
                    else
                    {
                        m_player.BoatRotation = Quaternion.identity;
                        m_player.ChangeAction(PlayerActionType.Drive);
                    }
                }
            }
        }



        public FlipJumpPlayerAction(Player player, PlayerActionInitialiser initialiser)
            : base(player, initialiser)
        {
            var x = m_player.transform.localEulerAngles.x;

            m_launchHeight = m_player.transform.position.y;
            m_jumpHeight = JumpHeight * JumpScale;
            m_jumpTime = m_jumpTimeStart = JumpTime * JumpScale;

            m_player.ShowTrail(false);
        }



        private float FlipTime
        {
            get { return ((FlipJumpPlayerActionInitialiser)m_initialiser).FlipTime; }
        }



        private float JumpHeight
        {
            get { return ((FlipJumpPlayerActionInitialiser)m_initialiser).JumpHeight; }
        }



        private float JumpTime
        {
            get { return ((FlipJumpPlayerActionInitialiser)m_initialiser).JumpTime; }
        }



        private float JumpScale
        {
            get { return ((FlipJumpPlayerActionInitialiser)m_initialiser).JumpScale; }
        }



        private float JumpBounces
        {
            get { return ((FlipJumpPlayerActionInitialiser)m_initialiser).JumpBounces; }
        }



        private float LaunchAngle
        {
            get { return ((FlipJumpPlayerActionInitialiser)m_initialiser).LaunchAngle; }
        }



        private int     m_jumpBounceCount, m_flipCount;
        private float   m_launchHeight, m_flipTime, m_jumpHeight, m_jumpTime, m_jumpTimeStart;
    }
}