using UnityEngine;



namespace SpeedyBoat
{
    public class Pickup : MonoBehaviour
    {
        public PickupType Type        { get; private set; }
        public Pickups    Container   { get; private set; }



        public virtual void Setup(PickupType type, Pickups container, PickupInitialiser initialiser)
        {
            Type = type;

            Container = container;
            m_initialiser = initialiser;

            gameObject.SetActive(true);
        }



        protected Track Track
        {
            get { return m_initialiser.Track; }
        }



        protected PickupInitialiser m_initialiser;
    }
}