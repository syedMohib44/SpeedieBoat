using UnityEngine;



namespace SpeedyBoat
{
    public class TrackCurveSegment : MonoBehaviour
    {
        public bool Traversed;



        public int Index
        {
            get; private set;
        }



        public void Setup(TrackCurve curve, int index, TrackBuilder.TrackSegment segment, TrackDetails trackDetails)
        {
            m_curve = curve;
            gameObject.name = (index + 1).ToString();

            Index = index;
            Traversed = false;

            Debug.Assert(segment.CurveMesh.vertexCount <= 8);

            GetComponent<MeshFilter>().sharedMesh = segment.CurveMesh;
            GetComponent<MeshCollider>().sharedMesh = segment.CurveCollisionMesh;
            GetComponent<MeshRenderer>().material = trackDetails.CurveMaterial;

            m_colors = segment.CurveMesh.colors;

            m_starEffect = transform.FindIncludingInactive("StarEffect");
            m_starEffect.gameObject.SetActive(false);

            var psMain = m_starEffect.GetComponent<ParticleSystem>().main;
            psMain.startColor = m_colors[0];

            SetColors(false);
        }



        private void OnTriggerEnter(Collider collision)
        {
            if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (collision.transform.GetComponentInParents<HumanPlayer>())
                {
                    //Debug.Log(Index + ": TrackCurveSegment Collision");
                    SetTraversed(true);
                }
            }
        }



        public void SetTraversed(bool informCurve)
        {
            if(Traversed)
            {
                return;
            }

            SetColors(true);

            Traversed = true;

            m_starEffect.transform.position = GetComponent<MeshRenderer>().bounds.center;
            m_starEffect.gameObject.SetActive(true);

            if (informCurve)
            {
                m_curve.OnSegmentTraversed(this);
            }
        }



        private void SetColors(bool highlighted)
        {
            var colorScale = highlighted ? 1 : .25f;

            var colors = (Color[])m_colors.Clone();
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] *= colorScale;
            }

            var mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.colors = colors;
        }



        private TrackCurve  m_curve;
        private Color[]     m_colors;
        private Transform   m_starEffect;
    }
}