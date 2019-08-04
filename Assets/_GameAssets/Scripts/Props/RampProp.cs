using UnityEngine;



namespace SpeedyBoat
{
    public class RampProp : Prop
    {
        public float    JumpTime    = 3;
        public float    JumpHeight  = 3;

        public float    BaseLaunchAngle = 10;
        public Vector2  SpeedBoostRange = new Vector2(1, 2);

        public Vector2  HeightScaleRange = new Vector2(.1f, 2f);

        public float    RiseTime = 1;
        public float    HoldTime = 2;
        public float    FallTime = 1;



        public virtual bool ShouldElevate
        {
            get { return true; }
        }



        public float LaunchAngle
        {
            get { return BaseLaunchAngle * Mathf.InverseLerp(HeightScaleRange.x, HeightScaleRange.y, RampScale); }
        }



        public float BoostScale
        {
            get { return Mathf.Lerp(SpeedBoostRange.x, SpeedBoostRange.y, Mathf.InverseLerp(HeightScaleRange.x, HeightScaleRange.y, RampScale)); }
        }



        public override void Setup(PropType type, Props container, PropInitialiser initialiser)
        {
            base.Setup(type, container, initialiser);

            m_model = transform.FindIncludingInactive("Model");

            var rampInitialiser = (RampPropInitialiser)m_initialiser;

            Vector3 lookDir, up;
            var pos = Track.GetPosWithDirAndUp(rampInitialiser.DistStart, out lookDir, out up);

            transform.position = pos;
            transform.localRotation = Quaternion.LookRotation(lookDir);

            ChangeState(ShouldElevate ? State.Rising : State.HoldingUp);
        }



        protected virtual void Update()
        {
            if(!ShouldElevate)
            {
                return;
            }

            m_stateTime = Mathf.Max(0, m_stateTime - Time.deltaTime);

            switch (m_state)
            {
                case State.Rising:
                    {
                        RampScale = Mathf.Lerp(HeightScaleRange.x, HeightScaleRange.y, 1f - m_stateTime / RiseTime);
                        break;
                    }

                case State.HoldingUp:
                case State.HoldingDown:
                    break;

                case State.Falling:
                    {
                        RampScale = Mathf.Lerp(HeightScaleRange.x, HeightScaleRange.y,  m_stateTime / RiseTime);
                        break;
                    }
            }

            if (m_stateTime <= 0)
            {
                ChangeState(m_state + 1);
            }
        }



        protected void ChangeState(State state)
        {
            state = (State)((int)state % (int)State.Count);

            switch (state)
            {
                case State.Rising:
                    {
                        m_stateTime = RiseTime;
                        RampScale = HeightScaleRange.x;
                        break;
                    }

                case State.HoldingUp:
                    {
                        m_stateTime = HoldTime;
                        RampScale = HeightScaleRange.y;
                        break;
                    }

                case State.HoldingDown:
                    {
                        m_stateTime = HoldTime;
                        RampScale = HeightScaleRange.x;
                        break;
                    }

                case State.Falling:
                    {
                        m_stateTime = FallTime;
                        RampScale = HeightScaleRange.y;
                        break;
                    }
            }

            m_state = state;
        }



        private float RampScale
        {
            get { return m_model.localScale.y; }
            set
            {
                var scale = m_model.localScale;
                scale.y = value;
                m_model.localScale = scale;
            }
        }



        protected enum State
        {
            None = -1,
            Rising,
            HoldingUp,
            Falling,
            HoldingDown,
            Count
        }



        protected State     m_state;


        private Transform   m_model;
        private float       m_stateTime;
    }
}