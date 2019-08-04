using UnityEngine;



namespace SpeedyBoat
{
    public class Boat : MonoBehaviour
    {
        public Color Color1, Color2;

        public float MaxSpeed       = 6; //8
        public float Acceleration   = 2; //3
        public float Decceleration  = 4;



        public Transform Model      { get; private set; }
        public Transform FrontLift  { get; private set; }
        public Transform BackLift   { get; private set; }



        public void ShowCrown(bool show)
        {
            m_crown.gameObject.SetActive(show);
        }
            


        public void ShowTrail(bool show)
        {
            m_speedTrail.GetComponent<TrailRenderer>().enabled = show;
        }



        public void Explode()
        {
            Exploder.Utils.ExploderSingleton.Instance.ExplodeObject(Model.gameObject);
        }



        public bool Enlarged
        {
            get { return m_enlarged; }

            set
            {
                m_enlarged = value;
                transform.localScale = Vector3.one * (m_enlarged ? m_scale * 1.25f : m_scale);

                var curve = m_speedTrail.GetComponent<TrailRenderer>().widthCurve;
                var keys = new Keyframe[]
                {
                    curve.keys[0],
                };

                keys[0].value = m_trailWidth * (value ? 2 : 1);

                m_speedTrail.GetComponent<TrailRenderer>().widthCurve = new AnimationCurve(keys);
            }
        }



        public float Length
        {
            get { return Model.GetComponent<Collider>().bounds.size.z; }
        }



        public void Setup(Color color1, Color color2)
        {
            gameObject.SetActive(true);

            var renderer = GetComponentInChildren<MeshRenderer>();
            renderer.materials[2].color = color1;
            renderer.materials[3].color = color2;

            Color1 = color1;
            Color2 = color2;

            Enlarged = false;
        }



        private void Update()
        {
            if(m_crown.gameObject.activeSelf)
            {
                m_crown.localRotation *= Quaternion.Euler(0, 180 * Time.deltaTime, 0);
            }
        }



        private void Awake()
        {
            Model = transform.FindIncludingInactive("Model");

            FrontLift = transform.FindIncludingInactive("FrontLift");
            BackLift = transform.FindIncludingInactive("BackLift");

            m_scale = transform.localScale.x;
            m_speedTrail = transform.FindIncludingInactive("SpeedTrail");

            m_crown = transform.FindIncludingInactive("Crown");
            m_trailWidth = m_speedTrail != null ? m_speedTrail.GetComponent<TrailRenderer>().widthCurve.keys[0].value : 0;
        }



        private bool            m_enlarged;
        private float           m_scale, m_trailWidth;
        private Transform       m_speedTrail, m_crown;
    }
}