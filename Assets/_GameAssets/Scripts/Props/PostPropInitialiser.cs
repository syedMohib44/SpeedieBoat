


namespace SpeedyBoat
{
    public class PostPropInitialiser : PropInitialiser
    {
        public readonly bool    Left;
        public readonly float   Dist, GroundHeight;



        public PostPropInitialiser(Track track, float groundHeight,  float dist, bool left)
            : base(track)
        {
            Dist = dist;
            GroundHeight = groundHeight;

            Left = left;
        }
    }
}