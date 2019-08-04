using UnityEngine;



namespace SpeedyBoat
{
    public class FaceCam : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}