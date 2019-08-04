using UnityEngine;



namespace SpeedyBoat
{
    public class WakeTrackDecal : TrackDecal
    {
        public override void Setup(TrackDecals container, Track track, float trackDist, float lateral, float width, float height, float length, int segmentCount)
        {
            base.Setup(container, track, trackDist, lateral, width, height, length, segmentCount);

            m_lifeTime = m_lifeTimeStart = 1;

            m_startWidth = width;
            m_endWidth = width * 10;

            m_material = GetComponent<MeshRenderer>().material;
        }



        protected override void Update()
        {
            base.Update();

            m_lifeTime = Mathf.Max(0, m_lifeTime - Time.deltaTime);

            var progress = Mathf.Sin(Mathf.PI * .5f * (1f - (m_lifeTime / m_lifeTimeStart)));
            Width = Mathf.Lerp(m_startWidth, m_endWidth, Mathf.Pow(progress, 2));

            var col = m_material.GetColor("_TintColor");
            col.a = (1f - progress) * .5f;
            m_material.SetColor("_TintColor", col);

            if(m_lifeTime <= 0)
            {
                m_container.FreeDecal();
            }
        }



        private Material    m_material;
        private float       m_lifeTime, m_lifeTimeStart, m_startWidth, m_endWidth;
    }
}