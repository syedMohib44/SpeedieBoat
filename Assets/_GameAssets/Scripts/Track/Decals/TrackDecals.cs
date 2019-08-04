using UnityEngine;



namespace SpeedyBoat
{
    public class TrackDecals : MonoBehaviour
    {
        public Level Level { get; private set; }



        public void FreeDecal()
        {
            Level.Track.FreeDecal(gameObject);
        }



        public TrackDecal Setup(Level level, Track track, TrackDecalType type, float startDist, float lateralAbs, float width, float height, float length, int segmentCount)
        {
            gameObject.SetActive(true);

            Level = level;
            transform.position = track.transform.position;

            var decal = transform.FindIncludingInactive(type.ToString()).GetComponent<TrackDecal>();

            for (int i=0;i < transform.childCount;++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            decal.Setup(this, track, startDist, lateralAbs, width, height, length, segmentCount);

            return decal;
        }
    }
}