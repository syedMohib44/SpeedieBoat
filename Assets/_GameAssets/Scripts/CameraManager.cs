using UnityEngine;



namespace SpeedyBoat
{
    public class CameraManager : MonoBehaviour
    {
        public Player   TrackedPlayer;
        public Vector3  LookPosition;
        public float    RotateSpeed = 1;

        public Vector2  FOVRange            = new Vector2(60, 40);
        public float    TrackingDist        = 8;
        public float    TrackingMoveSpeed   = 4;
        public float    TrackingRotateSpeed = 360;


        public void StopLooking()
        {
            RotateSpeed = 1;
            LookPosition = Vector3.zero;
        }

         

        public void LookAt(Vector3 pos, float rotateSpeed)
        {
            LookPosition = pos;
            RotateSpeed = rotateSpeed;
        }

        public void TrackPlayer(Player player, bool noTransition = false)
        {
            TrackedPlayer = player;
            if(TrackedPlayer)
            {
                UpdateTracking(noTransition);
            }
        }



        void LateUpdate()
        {
            //if(TrackedPlayer)
            {
                UpdateTracking(false);
            }
        }



        private void UpdateTracking(bool noTransition)
        {
            var targetPosition = transform.position;

            if (TrackedPlayer)
            {
                Camera.main.fieldOfView = Mathf.Lerp(FOVRange.x, FOVRange.y, Mathf.Sin(Mathf.PI * TrackedPlayer.JumpProgress));

                var up = TrackedPlayer.transform.up;
                var forward = TrackedPlayer.transform.forward;
                forward.y = 0;
                forward.Normalize();

                var left = -Vector3.Cross(forward, up);

                targetPosition = TrackedPlayer.transform.position + (left + -forward + Vector3.up).normalized * TrackingDist;
            }

            if (noTransition)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, TrackingMoveSpeed * Time.deltaTime);
            }

            if (TrackedPlayer)
            {
                transform.LookAt(TrackedPlayer.transform.position);
            }
            else if(LookPosition != Vector3.zero)
            {
                var oldRotation = transform.localRotation;

                transform.LookAt(LookPosition);

                var targetRotation = transform.localRotation;

                transform.localRotation = Quaternion.RotateTowards(oldRotation, targetRotation, 360 * RotateSpeed * Time.deltaTime);
            }
        }
    }
}