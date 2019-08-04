using UnityEngine;



namespace SpeedyBoat
{
    public class PostProp : Prop
    {
        public void ShowConfetti(bool show)
        {
            var ps = m_confetti.GetComponentInChildren<ParticleSystem>();
            var psEmission = ps.emission;
            psEmission.enabled = show;

            if (show)
            {
                m_confetti.gameObject.SetActive(false);
                m_confetti.gameObject.SetActive(true);
            }
        }



        public override void Setup(PropType type, Props container, PropInitialiser initialiser)
        {
            base.Setup(type, container, initialiser);

            var postInitialiser = (PostPropInitialiser)m_initialiser;

            Vector3 lookDir, up;
            var pos = Track.GetPosWithDirAndUp(postInitialiser.Dist, out lookDir, out up);

            var lateralLimits = postInitialiser.Track.GetLateralLimits(postInitialiser.Dist);
            var lateral = postInitialiser.Left ? lateralLimits.x : lateralLimits.y;

            var left = Vector3.Cross(lookDir, up);
            pos += left * lateral * 1.25f;

            var anchor = transform.FindIncludingInactive("Anchor");
            anchor.localScale = new Vector3(1, (pos.y - postInitialiser.GroundHeight) + 1f, 1);

            var ring = transform.FindIncludingInactive("Ring");
            var ringPos = ring.transform.localPosition;
            ring.transform.localPosition = new Vector3(ringPos.x, anchor.GetComponentInChildren<Renderer>().bounds.max.y - .5f, ringPos.z);

            pos.y = postInitialiser.GroundHeight;

            transform.position = pos;
            transform.localRotation = Quaternion.LookRotation(-lookDir);

            m_confetti = transform.FindIncludingInactive("Confetti");
            m_confetti.gameObject.SetActive(false);
        }



        private Transform m_confetti;
    }
}