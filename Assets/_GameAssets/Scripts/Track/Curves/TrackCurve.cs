using System.Collections.Generic;
using UnityEngine;



namespace SpeedyBoat
{
    public class TrackCurve : MonoBehaviour
    {
        public int TraversedCount;
        public int EntrySegmentErrorCount = 3;
        public readonly List<TrackCurveSegment> Segments = new List<TrackCurveSegment>();



        public void Setup(Level level, Transform curves, int curveIndex)
        {
            m_level = level;

            gameObject.name = "Curve " + (curveIndex + 1);
            transform.SetParent(curves, false);
        }



        public void OnSegmentTraversed(TrackCurveSegment segment)
        {
            if (segment.Index <= EntrySegmentErrorCount)
            {
                for (int i = 0; i < segment.Index; ++i)
                {
                    var curveSegment = transform.GetChild(i).GetComponent<TrackCurveSegment>();
                    if (curveSegment != segment && !curveSegment.Traversed)
                    {
                        curveSegment.SetTraversed(false);
                        ++TraversedCount;

                        //Debug.Log("OnSegmentTraversed::EntrySegmentErrorCount setting " + curveSegment.Index);
                    }
                }
            }

            //Debug.Log("OnSegmentTraversed setting " + segment.Index);

            if (++TraversedCount == transform.childCount)
            {
                m_level.OnPerfectCurve();
            }

            m_level.OnSegmentTraversed();
        }



        public void AddSegment(TrackBuilder.TrackSegment segment, TrackDetails trackDetails)
        {
            var curveSegment = Instantiate(GameSettings.Instance().TrackCurveSegmentPrefab).GetComponent<TrackCurveSegment>();
            curveSegment.transform.SetParent(transform, false);

            curveSegment.Setup(this, transform.childCount-1, segment, trackDetails);
        }



        private Level m_level;
    }
}