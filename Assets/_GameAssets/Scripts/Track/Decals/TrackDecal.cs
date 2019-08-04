using UnityEngine;



namespace SpeedyBoat
{
    public class TrackDecal : MonoBehaviour
    {
        public bool ClampToTrack;



        public float TrackDist
        {
            get { return m_trackDist; }
            set
            {
                if(m_trackDist != value)
                {
                    m_trackDist = value;
                    m_dirty = true;
                }
            }
        }



        // Lateral range = -1 to 1
        public float TrackLateral
        {
            get { return m_lateralAbs; }
            set
            {
                if (m_lateralAbs != value)
                {
                    m_lateralAbs = value;
                    m_dirty = true;
                }
            }
        }



        public float Width
        {
            get { return m_width; }
            set
            {
                if (m_width != value)
                {
                    m_width = value;
                    m_dirty = true;
                }
            }
        }



        public virtual void Setup(TrackDecals container, Track track, float trackDist, float lateralAbs, float width, float height, float length, int segmentCount)
        {
            gameObject.SetActive(true);

            m_track = track;
            m_container = container;

            m_trackDist     = trackDist;
            m_lateralAbs    = lateralAbs;
            m_width         = width;
            m_height        = height;
            m_length        = length;

            m_segmentCount  = segmentCount;

            m_dirty = true;
        }



        protected virtual void Update()
        {
            if(m_dirty)
            {
                BuildMesh();
                m_dirty = false;
            }
        }



        private void BuildMesh()
        {
            var mf = GetComponent<MeshFilter>();
            var existingMesh = mf.sharedMesh;
            mf.sharedMesh = m_track.BuildDecalMesh(m_trackDist, m_lateralAbs, m_width, m_height, m_length, m_segmentCount, ClampToTrack, existingMesh);
        }



        protected Track         m_track;
        protected TrackDecals   m_container;
        protected int           m_segmentCount;
        protected float         m_trackDist, m_width, m_length, m_lateralAbs, m_height;

        private bool            m_dirty;
    }
}