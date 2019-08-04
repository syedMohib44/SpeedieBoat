using UnityEngine;



namespace SpeedyBoat
{
    public class ChequerTrackDecal : TrackDecal
    {
        public override void Setup(TrackDecals container, Track track, float trackDist, float lateral, float width, float height, float length, int segmentCount)
        {
            base.Setup(container, track, trackDist, lateral, width, height, length, segmentCount);
        }



        protected override void Update()
        {
        }
    }
}