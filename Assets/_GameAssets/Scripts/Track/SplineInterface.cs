using BansheeGz.BGSpline.Components;
using UnityEngine;



namespace SpeedyBoat
{
    public abstract class SplineInterface
    {
        public abstract float Length { get; }

        public abstract Vector3 CalcPositionByDistance(float dist);
        public abstract Vector3 CalcPositionAndTangentByDistance(float dist, out Vector3 forward);
    }



    public class BansheeSplineInterface : SplineInterface
    {
        public BansheeSplineInterface(BGCcMath math)
        {
            m_math = math;
        }



        public override float Length
        {
            get { return m_math.GetDistance(); }
        }



        public override Vector3 CalcPositionByDistance(float dist)
        {
            dist = Mathf.Min(Length, dist);
            return m_math.CalcPositionByDistance(dist);
        }
        


        public override Vector3 CalcPositionAndTangentByDistance(float dist, out Vector3 tangent)
        {
            dist = Mathf.Min(Length, dist);
            return m_math.CalcPositionAndTangentByDistance(dist, out tangent);
        }



        private readonly BGCcMath m_math;
    }



    public class SplineMeshSplineInterface : SplineInterface
    {
        public SplineMeshSplineInterface(Spline spline)
        {
            m_spline = spline;
        }



        public override float Length
        {
            get { return m_spline.Length; }
        }



        public override Vector3 CalcPositionByDistance(float dist)
        {
            dist = Mathf.Min(Length, dist);
            return m_spline.GetLocationAlongSplineAtDistance(dist) + m_spline.transform.position;
        }



        public override Vector3 CalcPositionAndTangentByDistance(float dist, out Vector3 tangent)
        {
            dist = Mathf.Min(Length, dist);
            return m_spline.GetLocationAndTangentAtDistance(dist, out tangent) + m_spline.transform.position;
            //return m_spline.GetLocationAlongSplineAtDistance(dist) + m_spline.transform.position;
        }



        private readonly Spline m_spline;
    }
}