using UnityEngine;



namespace SpeedyBoat
{
    public class Pickups : MonoBehaviour
    {
        public void Setup(PickupType type, PickupInitialiser initialiser)
        {
            gameObject.SetActive(true);

            var pickupName = type.ToString();
            foreach(Transform t in transform)
            {
                if(t.name == pickupName)
                {
                    t.GetComponent<Pickup>().Setup(type, this, initialiser);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
            }
        }
    }
}