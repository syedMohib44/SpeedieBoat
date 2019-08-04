using UnityEngine;



namespace SpeedyBoat
{
    public class Enemies : MonoBehaviour
    {
        public void Setup(EnemyType type, EnemyInitialiser initialiser)
        {
            gameObject.SetActive(true);

            var pickupName = type.ToString();
            foreach(Transform t in transform)
            {
                if(t.name == pickupName)
                {
                    t.GetComponent<Enemy>().Setup(type, this, initialiser);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
            }
        }
    }
}