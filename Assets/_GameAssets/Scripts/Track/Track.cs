using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using UnityEngine;



namespace SpeedyBoat
{
    public class Track : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool Rebuild;
#endif

        public float                        TrackLength;
        public TrackDetails                 TrackData;
        public TrackBuilder.TrackSegment[]  TrackSegments { get; private set; }



        public void AdvanceWaterOffset(int layer, float amount)
        {
            var water = layer <= 0 ? m_water : m_water2;
            var material = water.GetComponent<MeshRenderer>().material;
            material.mainTextureOffset = new Vector2(0, material.mainTextureOffset.y + amount);
        }



        public void Destroy()
        {
            if (m_trackDecals != null)
            {
                m_trackDecals.Reset();
            }

            if (TrackSegments != null)
            {
                foreach (var segment in TrackSegments)
                {
                    segment.FreeAllocatedMeshes();
                }
            }
        }



        public float SegmentLength
        {
            get; private set;
        }



        public Vector3 StartLinePos
        {
            get { return CalcPositionByDistance(StartLineDist); }
        }



        public float StartLineDist
        {
            get { return TrackData.StartLineDist; }
        }



        public Vector3 GetEndPos(out Vector3 forward)
        {
            return CalcPositionAndTangentByDistance(TrackLength, out forward);
        }



        public Vector3 CalcPositionByDistance(float dist)
        {
            dist = Mathf.Min(dist, TrackLength);
            return m_splineInterface.CalcPositionByDistance(dist);
        }



        public Vector3 CalcPositionAndTangentByDistance(float dist, out Vector3 forward)
        {
            dist = Mathf.Min(dist, TrackLength);
            return m_splineInterface.CalcPositionAndTangentByDistance(dist, out forward);
        }



        public Vector2 GetLateralLimits(float dist)
        {
            return TrackBuilder.GetLateralLimits(dist, this);
        }



        public Vector2 GetLateralLimits(float dist, out bool isExtended)
        {
            return TrackBuilder.GetLateralLimits(dist, this, out isExtended);
        }



        public Vector3 GetPos(float dist)
        {
            dist = Mathf.Min(dist, TrackLength);
            return CalcPositionByDistance(dist);
        }



        public Vector3 GetPosWithDir(float dist, out Vector3 lookDir)
        {
            dist = Mathf.Min(dist, TrackLength);
            return CalcPositionAndTangentByDistance(dist, out lookDir);
        }



        public Vector3 GetPosWithDirAndUp(float dist, out Vector3 lookDir, out Vector3 up)
        {
            dist = Mathf.Min(dist, TrackLength);

            var segmentLength = TrackLength / TrackData.SegmentsCount;

            var pos = CalcPositionAndTangentByDistance(dist, out lookDir);

            up = Quaternion.LookRotation(lookDir) * Quaternion.Euler(-90, 0, 0) * Vector3.forward;

            return pos;
        }



        public Mesh BuildDecalMesh(float trackDist, float lateralAbs, float width, float height, float length, int segmentCount, bool clamp, Mesh existingMesh = null)
        {
            return TrackBuilder.BuildDecalMesh(this, trackDist, lateralAbs, width, height, length, segmentCount, clamp, existingMesh);
        }



        public TrackDecal CreateDecal(TrackDecalType type, float trackDist, float lateralAbs, float width, float height, float length, bool clamp = false)
        {
            var go = m_trackDecals.AllocateObject();
            if (go)
            {
                var segmentCount = Mathf.CeilToInt(length / .1f);
                return go.GetComponent<TrackDecals>().Setup(m_level, this, type, trackDist, lateralAbs, width, height, length, segmentCount);
            }

            return null;
        }



        public void FreeDecal(GameObject go)
        {
            m_trackDecals.FreeObject(go);
        }



        public void Setup(Level level)
        {
            m_level = level;

            var otherSplines = GetComponent<Spline>();
            var bansheeSplines = GetComponent<BGCcMath>();

            if (bansheeSplines)
            {
                m_splineInterface = new BansheeSplineInterface(bansheeSplines);
            }
            else
            {
                m_splineInterface = new SplineMeshSplineInterface(otherSplines);
            }

            TrackLength = m_splineInterface.Length;
            SegmentLength = TrackLength / TrackData.SegmentsCount;

            if (m_trackDecals == null)
            {

#if UNITY_EDITOR
                // In Editor mode we want to be able to preview track changes
                var splinesCurve = GetComponent<BGCurve>();
                splinesCurve.Changed -= OnTrackChanged;
                splinesCurve.Changed += OnTrackChanged;

                // Yet another check to accomodate the Prefab viewer, preventing it from creating multiple TrackDecals folders, oh my..
                var existing = transform.FindIncludingInactive("TrackDecals");
                if (existing)
                {
                    DestroyImmediate(existing.gameObject);
                }
#endif

                var poolParent = new GameObject("TrackDecals").transform;
                poolParent.parent = transform;
                m_trackDecals = new ObjectPool("TrackDecals", GameSettings.Instance().TrackDecalsPrefab, poolParent, GameSettings.Instance().MaxTrackDecals);
            }
            else
            {
                m_trackDecals.Reset();
            }

            Render();

            m_water = transform.FindIncludingInactive("Water");
            m_water2 = transform.FindIncludingInactive("Water2");

            CreateDecal(TrackDecalType.Chequer, TrackData.StartLineDist, 0, TrackData.WaterWidth, .001f, TrackData.WaterWidth * .5f);
            CreateDecal(TrackDecalType.Chequer, TrackData.EndLineDist, 0, TrackData.WaterWidth, .001f, TrackData.WaterWidth * .5f);
        }



        public void Render()
        {
#if UNITY_EDITOR
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            Destroy();

            TrackSegments = TrackBuilder.BuildTrack(m_level, this);

#if UNITY_EDITOR
            Debug.Log("BuildTrack: " + sw.Elapsed.Milliseconds + "ms");
#endif
        }



#if UNITY_EDITOR
        public void EditorRender(object sender = null, BGCurveChangedArgs args = null)
        {
            if (!Application.isPlaying)
            {
                // For prefab viewing we need to setup the whole thing as the prefab is shown in its own scene
                Setup(m_level);
            }
        }



        private void OnTrackChanged(object sender = null, BGCurveChangedArgs args = null)
        {
            EditorRender();
        }
#endif



        private Level           m_level;
        private Transform       m_water, m_water2;
        private ObjectPool      m_trackDecals;
        private SplineInterface m_splineInterface;
    }
}