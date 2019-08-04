using UnityEngine;



namespace SpeedyBoat
{
    public class Props : MonoBehaviour
    {
        public void Free()
        {
            m_pool.FreeObject(gameObject);
        }
            
        public Prop Setup(PropType type, PropInitialiser initialiser, ObjectPool pool)
        {
            gameObject.SetActive(true);

            m_pool = pool;

            var propName = type.ToString();
            if(propName == "StaticJumpRamp")
            {
                propName = "JumpRamp";
            }

            Prop prop = null;

            foreach (Transform t in transform)
            {
                if (t.name == propName)
                {
                    prop = t.GetComponent<Prop>();
                    prop.Setup(type, this, initialiser);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
            }

            return prop;
        }



        private ObjectPool m_pool;
    }
}