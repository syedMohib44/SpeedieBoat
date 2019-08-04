using System.Collections.Generic;
using System.Text;
using UnityEngine;



namespace SpeedyBoat
{
    public static class TrackBuilder
    {
        public const int MaxMeshes = 4096;



        // posNormal is how far through the segment the position lies
        public static TrackSegment GetTrackSegment(float dist, Track track, out float posNormal)
        {
            dist %= track.TrackLength;

            var index = Mathf.FloorToInt(dist / track.SegmentLength);
            posNormal = (dist - index * track.SegmentLength) / track.SegmentLength;

            return track.TrackSegments[index];
        }



        // Returns the negative left limit and positive right limit for a position within the Segment at Dist
        public static Vector2 GetLateralLimits(float dist, Track track)
        {
            float posNormal;

            var segment = GetTrackSegment(dist, track, out posNormal);

            return Vector2.Lerp(segment.LateralLimitsStart, segment.LateralLimitsEnd, posNormal);
        }



        public static Vector2 GetLateralLimits(float dist, Track track, out bool isExtended)
        {
            float posNormal;

            var segment = GetTrackSegment(dist, track, out posNormal);

            isExtended = segment.IsExtended;

            return Vector2.Lerp(segment.LateralLimitsStart, segment.LateralLimitsEnd, posNormal);
        }



        // Builds a decal mesh along the track at a given distance and lateral position, an option to the clamp the mesh to the tracks edge is provided
        public static Mesh BuildDecalMesh(Track track, float startDist, float lateralAbs, 
                                            float width, float height, float length, int segmentCount, bool clamp, Mesh existingMesh = null)
        {
            var segmentLength = length / segmentCount;
            var segments = new DecalSegment[segmentCount];

            var dist = startDist;
            for (int i = 0; i < segmentCount; ++i)
            {
                segments[i] = new DecalSegment(i, track, dist, lateralAbs, height, width, segmentLength);
                segments[i].Build(i, segmentCount, clamp);

                dist = (dist + segmentLength) % track.TrackLength;
            }

            //var meshName = new StringBuilder(segments.Length*2);
            var combinedMeshes = new CombineInstance[segments.Length];
            for (int i = 0; i < segments.Length; ++i)
            {
                //meshName.Append(segments[i].Mesh.name);
                //meshName.Append("-");

                combinedMeshes[i] = new CombineInstance
                {
                    mesh = segments[i].Mesh,
                    transform = Matrix4x4.identity// track.transform.localToWorldMatrix
                };
            }

            var mesh = existingMesh != null ? existingMesh : new Mesh();// { name = meshName.ToString() };
            mesh.Clear();

            mesh.CombineMeshes(combinedMeshes);
            mesh.RecalculateBounds();

            foreach(var segment in segments)
            {
                ms_meshPool.FreeMesh(segment.Mesh);
            }

            return mesh;
        }



        public static TrackSegment[] BuildTrack(Level level, Track track)
        {
            var trackDetails = track.TrackData;

            /*
            var trackPos = track.transform.localPosition;
            trackPos.y = trackDetails.Height;
            track.transform.localPosition = trackPos;
            */

#if UNITY_EDITOR
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var trackSegments = new TrackSegment[trackDetails.SegmentsCount];
            var waterMeshData = new WaterMeshData(trackDetails.SegmentsCount);

            for (int i = 0; i < trackSegments.Length; ++i)
            {
                trackSegments[i] = new TrackSegment(i, track);
            }

#if UNITY_EDITOR
            Debug.Log("Create Segments took: " + sw.Elapsed.Milliseconds + "ms");
            sw.Reset();
            sw.Start();
#endif

            TaperCurves(trackSegments, trackDetails);

#if UNITY_EDITOR
            Debug.Log("Taper Curves took: " + sw.Elapsed.Milliseconds + "ms");
            sw.Reset();
            sw.Start();
#endif

            for (int i = 0; i < trackSegments.Length; ++i)
            {
                trackSegments[i].Build(i, trackSegments, waterMeshData, false);
            }

#if UNITY_EDITOR
            Debug.Log("Build segments took: " + sw.Elapsed.Milliseconds + "ms");
            sw.Reset();
            sw.Start();
#endif

            // Structure mesh - created by combining each Segments unique Structure mesh
            {
                var structure = track.transform.FindIncludingInactive("Structure");
                if (structure == null)
                {
                    structure = new GameObject("Structure").transform;
                    structure.SetParent(track.transform, false);
                }

                var mf = structure.GetComponent<MeshFilter>();
                if (mf == null)
                {
                    mf = structure.gameObject.AddComponent<MeshFilter>();
                }

                var mr = structure.GetComponent<MeshRenderer>();
                if (mr == null)
                {
                    mr = structure.gameObject.AddComponent<MeshRenderer>();
                }
                mr.material = trackDetails.StructureMaterial;

                var structureMesh = mf.sharedMesh;
                if (structureMesh == null)
                {
                    structureMesh = new Mesh { name = "Structure" };
                }
                else
                {
                    structureMesh.Clear();
                }

                var combinedMeshes = new List<CombineInstance>();

                for (int i = 0; i < trackSegments.Length; ++i)
                {
                    combinedMeshes.Add(new CombineInstance
                    {
                        mesh = trackSegments[i].StructureMesh,
                        transform = Matrix4x4.identity,
                    });

                    if(trackSegments[i].EndCapStructureMesh != null)
                    {
                        combinedMeshes.Add(new CombineInstance
                        {
                            mesh = trackSegments[i].EndCapStructureMesh,
                            transform = Matrix4x4.identity
                        });
                    }
                }

                structureMesh.CombineMeshes(combinedMeshes.ToArray());

                structureMesh.RecalculateNormals();
                structureMesh.RecalculateBounds();
                structure.GetComponent<MeshFilter>().sharedMesh = structureMesh;

                var structureCollider = structure.GetComponent<MeshCollider>();
                if (structureCollider == null)
                {
                    structureCollider = structure.gameObject.AddComponent<MeshCollider>();
                    structureCollider.sharedMaterial = GameSettings.Instance().StructureMaterial;
                }

                /*
                // Collider mesh with taller sides
                combinedMeshes.Clear();
                var structureMeshCollider = new Mesh();

                for (int i = 0; i < trackSegments.Length; ++i)
                {
                    combinedMeshes.Add(new CombineInstance
                    {
                        mesh = trackSegments[i].StructureCollisionMesh,
                        transform = Matrix4x4.identity
                    });
                }
                structureMeshCollider.CombineMeshes(combinedMeshes.ToArray());

                if (structureCollider == null)
                {
                    structureCollider = structure.gameObject.AddComponent<MeshCollider>();
                    structureCollider.sharedMaterial = GameSettings.Instance().StructureMaterial;
                }
                */

                structureCollider.sharedMesh = structureMesh;

#if UNITY_EDITOR
                Debug.Log("Creating structure meshes took: " + sw.Elapsed.Milliseconds + "ms");
                sw.Reset();
                sw.Start();
#endif
            }

            // Water mesh - a single mesh consisting of all Segments' water
            {
                CreateWaterLayer(0, track, trackDetails, trackSegments, waterMeshData);
                CreateWaterLayer(1, track, trackDetails, trackSegments, waterMeshData);

                /*
                var water = track.transform.FindIncludingInactive("Water");
                if (water == null)
                {
                    water = new GameObject("Water").transform;
                    water.SetParent(track.transform, false);
                }
                water.gameObject.layer = LayerMask.NameToLayer("Water");

                var mf = water.GetComponent<MeshFilter>();
                if (mf == null)
                {
                    mf = water.gameObject.AddComponent<MeshFilter>();
                }

                var mr = water.GetComponent<MeshRenderer>();
                if (mr == null)
                {
                    mr = water.gameObject.AddComponent<MeshRenderer>();
                }
                mr.material = trackDetails.WaterMaterial;
                mr.material.mainTextureScale = new Vector2(1, trackSegments.Length/8);

                var waterMesh = mf.sharedMesh;
                if (waterMesh == null)
                {
                    waterMesh = new Mesh { name = "Water" };
                }
                else
                {
                    waterMesh.Clear();
                }

                waterMesh.vertices = waterMeshData.Verts;
                waterMesh.triangles = waterMeshData.Triangles;
                waterMesh.normals = waterMeshData.Normals;
                waterMesh.uv = waterMeshData.UVs;

                waterMesh.RecalculateNormals();
                waterMesh.RecalculateBounds();

                water.GetComponent<MeshFilter>().sharedMesh = waterMesh;

                // Water collider is pushed down so that the boats sit below the water level
                var waterColliderMesh = new Mesh();
                var colliderVerts = waterMesh.vertices;

                for(int i=0;i < colliderVerts.Length;++i)
                {
                    colliderVerts[i].y -= .05f;
                }

                waterColliderMesh.vertices = colliderVerts;
                waterColliderMesh.triangles = waterMesh.triangles;

                var waterCollider = water.GetComponent<MeshCollider>();
                if(waterCollider == null)
                {
                    waterCollider = water.gameObject.AddComponent<MeshCollider>();
                    waterCollider.sharedMaterial = GameSettings.Instance().WaterMaterial;
                }
                waterCollider.sharedMesh = waterColliderMesh;
                */

#if UNITY_EDITOR
                Debug.Log("Creating Water meshes took: " + sw.Elapsed.Milliseconds + "ms");
                sw.Reset();
                sw.Start();
#endif
            }


            // Curve segments which sit on top of the water
            {
                var curves = track.transform.FindIncludingInactive("Curves");
                if (curves == null)
                {
                    curves = new GameObject("Curves").transform;
                    curves.SetParent(track.transform, false);
                }

                // Release old curve segments
                while(curves.childCount > 0)
                {
                    var curve = curves.GetChild(0);
                    curve.parent = null;

                    while (curve.childCount > 0)
                    {
                        var curveSegment = curve.GetChild(0);
                        var mf = curveSegment.GetComponent<MeshFilter>();
                        if (mf)
                        {
                            if (mf.sharedMesh)
                            {
                                ms_meshPool.FreeMesh(mf.sharedMesh);
                                mf.sharedMesh = null;
                            }
                        }

                        curveSegment.parent = null;
#if UNITY_EDITOR
                        if (Application.isPlaying)
                        {
                            Object.Destroy(curveSegment.gameObject);
                        }
                        else
                        {
                            Object.DestroyImmediate(curveSegment.gameObject);
                        }
#else
                        Object.Destroy(curveSegment.gameObject);
#endif
                    }

#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        Object.Destroy(curve.gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(curve.gameObject);
                    }
#else
                    Object.Destroy(curve.gameObject);
#endif
                }

                for(int i=0;i < trackDetails.Curves.Count;++i)
                {
                    var curveDetails = trackDetails.Curves[i];
                    var curve = Object.Instantiate(GameSettings.Instance().TrackCurvePrefab).GetComponent<TrackCurve>();
                    curve.Setup(level, curves, i);

                    var start = curveDetails.Start;
                    var end = curveDetails.Start + curveDetails.Length;

                    for (int k = start; k < end; ++k)
                    {
                        var segment = trackSegments[k];
                        curve.GetComponent<TrackCurve>().AddSegment(segment, trackDetails);
                    }
                }

#if UNITY_EDITOR
                Debug.Log("Creating Curve meshes took: " + sw.Elapsed.Milliseconds + "ms");
                sw.Reset();
                sw.Start();
#endif
            }

            foreach (var segment in trackSegments)
            {
                ms_meshPool.FreeMesh(segment.StructureMesh);
            }

            return trackSegments;
        }



        private static void CreateWaterLayer(int layer, Track track, TrackDetails trackDetails, TrackSegment[] trackSegments, WaterMeshData waterMeshData)
        {
            var waterLayerName = layer <= 0 ? "Water" : "Water2";

            var water = track.transform.FindIncludingInactive(waterLayerName);
            if (water == null)
            {
                water = new GameObject(waterLayerName).transform;
                water.SetParent(track.transform, false);
            }
            water.gameObject.layer = LayerMask.NameToLayer("Water");

            var mf = water.GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = water.gameObject.AddComponent<MeshFilter>();
            }

            var mr = water.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                mr = water.gameObject.AddComponent<MeshRenderer>();
            }
            mr.material = layer <= 0 ? trackDetails.WaterMaterial : trackDetails.WaterMaterial2;
            mr.material.mainTextureScale = new Vector2(1, trackSegments.Length / 8);

            var waterMesh = mf.sharedMesh;
            if (waterMesh == null)
            {
                waterMesh = new Mesh { name = waterLayerName };
            }
            else
            {
                waterMesh.Clear();
            }

            waterMesh.vertices = waterMeshData.Verts;
            waterMesh.triangles = waterMeshData.Triangles;
            waterMesh.normals = waterMeshData.Normals;
            waterMesh.uv = waterMeshData.UVs;

            waterMesh.RecalculateNormals();
            waterMesh.RecalculateBounds();

            water.GetComponent<MeshFilter>().sharedMesh = waterMesh;

            // Water collider is pushed down so that the boats sit below the water level
            var waterColliderMesh = new Mesh();
            var colliderVerts = waterMesh.vertices;

            for (int i = 0; i < colliderVerts.Length; ++i)
            {
                colliderVerts[i].y -= .05f;
            }

            waterColliderMesh.vertices = colliderVerts;
            waterColliderMesh.triangles = waterMesh.triangles;

            var waterCollider = water.GetComponent<MeshCollider>();
            if (waterCollider == null)
            {
                waterCollider = water.gameObject.AddComponent<MeshCollider>();
                waterCollider.sharedMaterial = GameSettings.Instance().WaterMaterial;
            }
            waterCollider.sharedMesh = waterColliderMesh;
        }



        private static void TaperCurves(Segment[] segments, TrackDetails trackDetails)
        {
            foreach (var curve in trackDetails.Curves)
            {
                var powScale = 2;

                for (int i = 0; i < curve.Length; ++i)
                {
                    var segment = segments[curve.Start + i];

                    // Range starts late and ends early to get a smoother continuation of the track into and out of the widened curve 
                    var lerpRange = new Vector2(0, curve.Length * 2);

                    var curveStartNormal = Mathf.InverseLerp(lerpRange.x, lerpRange.y, i * 2);
                    segment.CurveStartNormal = curveStartNormal;

                    var curveEndNormal = Mathf.InverseLerp(lerpRange.x, lerpRange.y, i * 2 + 1);
                    segment.CurveEndNormal = curveEndNormal;

                    if (i <= 0)
                    {
                        segment.CurveScaleStart = Mathf.Pow(Mathf.Sin(Mathf.PI * curveStartNormal), powScale);
                    }
                    else
                    {
                        // Start is the previous segments end so they line up
                        var prevIndex = i - 1;
                        segment.CurveScaleStart = segments[curve.Start + prevIndex].CurveScaleEnd;
                    }

                    segment.CurveScaleEnd = Mathf.Pow(Mathf.Sin(Mathf.PI * curveEndNormal), powScale);
                }
            }
        }



        public abstract class Segment
        {
            public int ExtendedSide;

            public float CurveStartNormal, CurveEndNormal;
            public float CurveScaleStart = 1;
            public float CurveScaleEnd = 1;

            public readonly int Index;
            public readonly Vector3 Pos, NextPos;
            public readonly float CurveDelta, NextCurveDelta;
            public readonly Quaternion Forward, NextForward, NextNextForward;



            public bool IsExtended
            {
                get { return ExtendedSide != 0; }
            }



            // Segment length is NOT the tracks segment length for Decals so must be passed
            protected Segment(int index, float dist, float segmentLength, Track track)
            {
                Index = index;
                m_track = track;

                Vector3 tangent, nextTangent, nextNextTangent;
                Pos = track.CalcPositionAndTangentByDistance(dist, out tangent);
                Pos -= track.transform.position;
                Forward = Quaternion.LookRotation(tangent);

                NextPos = track.CalcPositionAndTangentByDistance(dist + segmentLength, out nextTangent);
                NextPos -= track.transform.position;
                NextForward = Quaternion.LookRotation(nextTangent);

                track.CalcPositionAndTangentByDistance(dist + segmentLength*2, out nextNextTangent);
                NextNextForward = Quaternion.LookRotation(nextNextTangent);
            }



            protected TrackDetails TrackDetails
            {
                get { return m_track.TrackData; }
            }
                


            protected readonly Track m_track;
        }



        public class DecalSegment : Segment
        {
            public Mesh Mesh;



            public void Build(int segmentIndex, int segmentCount, bool clamp)
            {
                var height = TrackDetails.BaseHeight + TrackDetails.WaterLevel + m_height;

                var basePos = Pos + Vector3.up * height;
                var nextBasePos = NextPos + Vector3.up * height;

                var forwardNormal = Forward * Vector3.forward;
                var nextForwardNormal = NextForward * Vector3.forward;

                var leftNormal = Forward * Quaternion.Euler(0, -90, 0) * Vector3.forward;
                var rightNormal = Forward * Quaternion.Euler(0, 90, 0) * Vector3.forward;

                var upNormal = Vector3.Cross(forwardNormal, rightNormal);
                var nextUpNormal = Vector3.Cross(nextForwardNormal, rightNormal);

                var nextLeftNormal = NextForward * Quaternion.Euler(0, -90, 0) * Vector3.forward;
                var nextRightNormal = NextForward * Quaternion.Euler(0, 90, 0) * Vector3.forward;

                var lateralPos = m_lateral;// m_trackDetails.WaterWidth * .5f * m_lateral;

                var lateralStart = rightNormal * lateralPos;
                var nextLateralStart = nextRightNormal * lateralPos;

                var limits = GetLateralLimits(m_dist, m_track);

                // Clamping the sides so it doesnt go off the track meaning it is squished as it nears the tracks edge
                var leftPosClamped = clamp ? Mathf.Max(limits.x, lateralPos - m_width * .5f) : lateralPos - m_width * .5f; 
                var rightPosClamped = clamp ? Mathf.Min(limits.y, lateralPos + m_width * .5f) : lateralPos + m_width * .5f;

                // Modify the UVs horizontal start and end so the image dissapears when out of the track area
                var leftLength = Mathf.Abs(leftPosClamped - lateralPos);
                var leftUVStart = Mathf.Lerp(.5f, 0, leftLength / (m_width * .5f));

                var rightLength = Mathf.Abs(rightPosClamped - lateralPos);
                var rightUVEnd = Mathf.Lerp(.5f, 1, rightLength / (m_width * .5f));

                Mesh.vertices = new Vector3[]
                {
                    // BL
                    basePos + lateralStart + leftNormal * leftLength,
                    // TL
                    nextBasePos + nextLateralStart + nextLeftNormal * leftLength,
                    // TR
                    nextBasePos + nextLateralStart + nextRightNormal * rightLength,
                    // BR
                    basePos + lateralStart + rightNormal * rightLength
                };

                Mesh.normals = new Vector3[]
                {
                    upNormal,
                    nextUpNormal,
                    nextUpNormal,
                    upNormal,
                };

                Mesh.triangles = new int[]
                {
                    0, 1, 2,
                    0, 2, 3
                };

                var uvStep = 1f / segmentCount;

                Mesh.uv = new Vector2[]
                {
                    new Vector2(leftUVStart, uvStep * segmentIndex),
                    new Vector2(leftUVStart, uvStep * (segmentIndex + 1)),
                    new Vector2(rightUVEnd, uvStep * (segmentIndex + 1)),
                    new Vector2(rightUVEnd, uvStep * segmentIndex)
                };
            }



            // Height refers to height above the water level
            public DecalSegment(int index, Track track, float dist, float lateral, float height, float width, float segmentLength)
                : base(index, dist, segmentLength, track)
            {
                m_width = width;
                m_height = height;

                m_dist = dist;
                m_lateral = lateral;

                Mesh = ms_meshPool.AllocateMesh();
            }



            private readonly float m_width, m_height, m_dist, m_lateral;
        }



        // A track degment holds the data required to Build a mesh
        public class TrackSegment : Segment
        {
            public Vector2  LateralLimitsStart, LateralLimitsEnd;
            public Mesh     StructureMesh, StructureCollisionMesh, CurveMesh, CurveCollisionMesh, EndCapStructureMesh;



            public void FreeAllocatedMeshes()
            {
                if(CurveMesh != null)
                {
                    ms_meshPool.FreeMesh(CurveMesh);
                    CurveMesh = null;
                }

                if (CurveCollisionMesh != null)
                {
                    ms_meshPool.FreeMesh(CurveCollisionMesh);
                    CurveCollisionMesh = null;
                }

                if (StructureMesh != null)
                {
                    ms_meshPool.FreeMesh(StructureMesh);
                    StructureMesh = null;
                }

                if (StructureCollisionMesh != null)
                {
                    ms_meshPool.FreeMesh(StructureCollisionMesh);
                    StructureCollisionMesh = null;
                }
            }



            // The structure mech has unique verts for hard edge normals, ergo each segment has its own mesh
            // The water mesh is a shared mesh so that verts can be welded for both UV scrolling and smooth normals
            public void Build(int segmentIndex, TrackSegment[] trackSegments, WaterMeshData waterMeshData, bool clamp)
            {
                var nextSegment = trackSegments[Mathf.Min(trackSegments.Length-1, segmentIndex + 1)];

                var leftNormal = Forward * Quaternion.Euler(0, -90, 0) * Vector3.forward;
                var rightNormal = Forward * Quaternion.Euler(0, 90, 0) * Vector3.forward;

                var nextLeftNormal = NextForward * Quaternion.Euler(0, -90, 0) * Vector3.forward;
                var nextRightNormal = NextForward * Quaternion.Euler(0, 90, 0) * Vector3.forward;

                var baseHeight = TrackDetails.BaseHeight;

                var baseTop = Pos + Vector3.up * baseHeight;
                var nextBaseTop = NextPos + Vector3.up * baseHeight;

                var widening = ExtendedSide == 0 ? 0 : TrackDetails.CurveWidthScale * CurveScaleStart;
                var nextWidening = nextSegment.ExtendedSide == 0 ? 0 : TrackDetails.CurveWidthScale * nextSegment.CurveScaleStart;

                // Structure
                {
                    /*
                    if(segmentIndex == 0)
                    {
                        BuildEndCap(true);
                    }
                    */

                    var baseWidth = TrackDetails.BaseWidth;

                    var wallWidth = TrackDetails.WallWidth;
                    var wallHeight = TrackDetails.WallHeight;

                    var halfBaseWidth = baseWidth * .5f;

                    var wallTop = baseTop + Vector3.up * wallHeight;
                    var baseBottom = Pos;

                    var nextWallTop = nextBaseTop + Vector3.up * wallHeight;
                    var nextBaseBottom = NextPos;

                    if(Index == 0)
                    {
                        Debug.Log("First vert at " + (baseTop + leftNormal * halfBaseWidth));
                    }

                    StructureMesh.vertices = new Vector3[]
                    {
                        // BASE TOP: Quad (verts 0-3)

                        // BL
                        baseTop + leftNormal * halfBaseWidth,
                        // TL
                        nextBaseTop + nextLeftNormal * halfBaseWidth,
                        // TR
                        nextBaseTop + nextRightNormal * (halfBaseWidth + nextWidening),
                        // BR
                        baseTop + rightNormal * (halfBaseWidth + widening),

                        // BASE BOTTOM: Quad (verts 4-7)

                        // BL
                        baseBottom + leftNormal * halfBaseWidth,
                        // TL
                        nextBaseBottom + nextLeftNormal * halfBaseWidth,
                        // TR
                        nextBaseBottom + nextRightNormal * (halfBaseWidth + nextWidening),
                        // BR
                        baseBottom + rightNormal * (halfBaseWidth + widening),

                        // LEFT WALL (Cube without bottom)
                        
                        // LEFT WALL TOP: Quad (verts 8-11)

                        // BL
                        wallTop + leftNormal * halfBaseWidth,
                        // TL
                        nextWallTop + nextLeftNormal * halfBaseWidth,
                        // TR
                        nextWallTop + nextLeftNormal * halfBaseWidth + nextRightNormal * wallWidth,
                        // BR
                        wallTop + leftNormal * halfBaseWidth + rightNormal * wallWidth,

                        // LEFT WALL OUTSIDE: Quad (verts 12-15)

                        // BL
                        nextBaseBottom + nextLeftNormal * halfBaseWidth,
                        // TL
                        nextWallTop + nextLeftNormal * halfBaseWidth,
                        // TR
                        wallTop + leftNormal * halfBaseWidth,
                        // BR
                        baseBottom + leftNormal * halfBaseWidth,

                        // LEFT WALL INSIDE: Quad (verts 16-19)

                        // BL
                        baseBottom + leftNormal * halfBaseWidth + rightNormal * wallWidth,
                        // TL
                        wallTop + leftNormal * halfBaseWidth + rightNormal * wallWidth,
                        // TR
                        nextWallTop + nextLeftNormal * halfBaseWidth + nextRightNormal * wallWidth,
                        // BR
                        nextBaseBottom + nextLeftNormal * halfBaseWidth + nextRightNormal * wallWidth,


                        // RIGHT WALL (Cube without bottom)
                        
                        // RIGHT WALL TOP: Quad (verts 20-23)

                        // BL
                        wallTop + rightNormal * (halfBaseWidth + widening) + leftNormal * wallWidth,
                        // TL
                        nextWallTop + nextRightNormal * (halfBaseWidth + nextWidening) + nextLeftNormal * wallWidth,
                        // TR
                        nextWallTop + nextRightNormal * (halfBaseWidth + nextWidening),
                        // BR
                        wallTop + rightNormal * (halfBaseWidth + widening),

                        // RIGHT WALL OUTSIDE: Quad (verts 24-27)

                        // BL
                        baseBottom + rightNormal * (halfBaseWidth + widening),
                        // TL
                        wallTop + rightNormal * (halfBaseWidth + widening),
                        // TR
                        nextWallTop + nextRightNormal * (halfBaseWidth + nextWidening),
                        // BR
                        nextBaseBottom + nextRightNormal * (halfBaseWidth + nextWidening),

                        // RIGHT WALL INSIDE: Quad (verts 28-31)

                        // BL
                        nextBaseBottom + nextRightNormal * (halfBaseWidth + nextWidening) + nextLeftNormal * wallWidth,
                        // TL
                        nextWallTop + nextRightNormal * (halfBaseWidth + nextWidening) + nextLeftNormal * wallWidth,
                        // TR
                        wallTop + rightNormal * (halfBaseWidth + widening) + leftNormal * wallWidth,
                        // BR
                        baseBottom + rightNormal * (halfBaseWidth + widening) + leftNormal * wallWidth,
                    };

                    StructureMesh.triangles = new int[]
                    {
                        // BASE TOP: Quad (verts 0-3)
                        0, 1, 2,
                        0, 2, 3,

                        // BASE BOTTOM: Quad (verts 4-7)
                        4, 7, 6,
                        4, 6, 5,

                        // LEFT WALL TOP: Quad (verts 8-11)
                        8, 9, 10,
                        8, 10, 11,

                        // LEFT WALL OUTSIDE: Quad (verts 12-15)
                        12, 13, 14,
                        12, 14, 15,

                        // LEFT WALL INSIDE: Quad (verts 16-19)
                        16, 17, 18, // 16, 11, 10, // Smooth edge/Hard edge
                        16, 18, 19, // 16, 10, 19, // Smooth edge/Hard edge

                        // RIGHT WALL TOP: Quad (verts 20-23)
                        20, 21, 22,
                        20, 22, 23,

                        // RIGHT WALL OUTSIDE: Quad (verts 24-27)
                        24, 25, 26,
                        24, 26, 27,

                        // RIGHT WALL INSIDE: Quad (verts 28-31)
                        28, 29, 30, // 28, 21, 20, // Smooth edge/Hard edge 
                        28, 30, 31, // 28, 20, 31, // Smooth edge/Hard edge 
                    };

                    // Stretch the sides of the mesh for the collider to prevent coming off the track
                    var colliderVerts = StructureMesh.vertices;
                    for(int i=8;i < colliderVerts.Length;i+=32)
                    {
                        for(int k=0;k < 4;++k)
                        {
                            // LEFT WALL TOP: Quad (verts 8-11)
                            ++colliderVerts[i + k].y;

                            // RIGHT WALL TOP: Quad (verts 20-23)
                            ++colliderVerts[i + 12 + k].y;
                        }
                    }

                    StructureCollisionMesh.vertices = colliderVerts;
                    StructureCollisionMesh.triangles = StructureMesh.triangles;
                }


                // Water
                {
                    var waterWidth = TrackDetails.WaterWidth;
                    var waterLevel = TrackDetails.WaterLevel;

                    var basePos = baseTop + Vector3.up * waterLevel;
                    var nextBasePos = nextBaseTop + Vector3.up * waterLevel;

                    var forwardNormal = Forward * Vector3.forward;
                    var nextForwardNormal = NextForward * Vector3.forward;

                    var upNormal = Vector3.Cross(forwardNormal, rightNormal);
                    var nextUpNormal = Vector3.Cross(nextForwardNormal, rightNormal);

                    var leftLength = waterWidth * .5f;
                    var rightLength = waterWidth * .5f;

                    LateralLimitsStart = new Vector2(-leftLength, rightLength + widening);
                    LateralLimitsEnd = new Vector2(-leftLength, rightLength + nextWidening);

                    var verts = new Vector3[]
                    {
                        // BL
                        basePos + rightNormal * LateralLimitsStart.x,
                        // TL
                        nextBasePos + nextRightNormal * LateralLimitsEnd.x,
                        // TR
                        nextBasePos + nextRightNormal * LateralLimitsEnd.y,
                        // BR
                        basePos + rightNormal * LateralLimitsStart.y
                    };

                    var normals = new Vector3[]
                    {
                        upNormal,
                        nextUpNormal,
                        nextUpNormal,
                        upNormal,
                    };

                    var triangles = new int[]
                    {
                        0, 1, 2,
                        0, 2, 3
                    };

                    var uvY = (float)segmentIndex / trackSegments.Length;
                    var uvNextY = (float)(segmentIndex + 1) / trackSegments.Length;

                    var uvs = new Vector2[]
                    {
                        new Vector2(0, uvY),
                        new Vector2(0, uvNextY),
                        new Vector2(1, uvNextY),
                        new Vector2(1, uvY),
                    };

                    waterMeshData.AddSegmentData(segmentIndex, verts, normals, uvs);

                    // Extended Curve segment
                    if (IsExtended)
                    {
                        CurveMesh = ms_meshPool.AllocateMesh();
                        CurveCollisionMesh = ms_meshPool.AllocateMesh();

                        // Lift it above the water
                        basePos += upNormal * .02f;
                        nextBasePos += nextUpNormal * .02f;

                        CurveMesh.vertices = new Vector3[]
                        {
                            // BL
                            basePos + rightNormal * rightLength,
                            // TL
                            nextBasePos + nextRightNormal * rightLength,
                            // TR
                            nextBasePos + nextRightNormal * (rightLength + nextWidening),
                            // BR
                            basePos + rightNormal * (rightLength + widening),

                            // For the collider to be a trigger the mesh needs to be convex, a convex mesh collider cannot be Co planar (flat plane)
                            // so need to turn it into a 3D shape so as to be able to use triggers since I dont want physics collisions as these slow the boat

                            // BL
                            basePos + rightNormal * rightLength + Vector3.down * .01f,
                            // TL
                            nextBasePos + nextRightNormal * rightLength + Vector3.down * .01f,
                            // TR
                            nextBasePos + nextRightNormal * (rightLength + nextWidening) + Vector3.down * .01f,
                            // BR
                            basePos + rightNormal * (rightLength + widening) + Vector3.down * .01f,
                        };

                        CurveMesh.normals = new Vector3[]
                        {
                            upNormal,
                            nextUpNormal,
                            nextUpNormal,
                            upNormal,

                            -upNormal,
                            -nextUpNormal,
                            -nextUpNormal,
                            -upNormal
                        };

                        CurveMesh.triangles = new int[]
                        {
                            // Top face
                            0, 1, 2,
                            0, 2, 3,

                            // Bottom
                            7, 5, 4,
                            7, 6, 5,

                            // Left
                            5, 1, 0,
                            5, 0, 4,

                            // Right
                            7, 3, 2,
                            7, 2, 6,

                            // Front
                            4, 0, 3,
                            4, 3, 7,

                            // Back
                            2, 1, 5,
                            2, 5, 6
                        };

                        var startColor = new HSBColor { h = CurveStartNormal, s = 1, b = 1 }.ToColor();
                        startColor.a = 1;

                        var endColor = new HSBColor { h = CurveEndNormal, s = 1, b = 1 }.ToColor();
                        endColor.a = 1;

                        CurveMesh.colors = new Color[]
                        {
                            startColor,
                            endColor,
                            endColor,
                            startColor,

                            startColor,
                            endColor,
                            endColor,
                            startColor
                        };

                        // Collision Mesh: A taller mesh is required to catch the boat in Y
                        var strectchedVerts = CurveMesh.vertices;
                        for(int i=0;i < strectchedVerts.Length; ++i)
                        {
                            // Top 4 stretched up, bottom 4 stretched down
                            strectchedVerts[i] += i < 4 ? Vector3.up : Vector3.down;
                        }

                        CurveCollisionMesh.vertices = strectchedVerts;
                        CurveCollisionMesh.triangles = CurveMesh.triangles;
                    }
                }
            }



            public TrackSegment(int index, Track track)
                : base(index, track.SegmentLength * index, track.SegmentLength, track)
            {
                ExtendedSide = TrackDetails.GetExtendedSide(index);

                StructureMesh = ms_meshPool.AllocateMesh();
                StructureCollisionMesh = ms_meshPool.AllocateMesh();
            }



            private void BuildEndCap(bool front)
            {
                EndCapStructureMesh = ms_meshPool.AllocateMesh();

                var leftNormal = Forward * Quaternion.Euler(0, -90, 0) * Vector3.forward;
                var rightNormal = Forward * Quaternion.Euler(0, 90, 0) * Vector3.forward;

                var baseHeight = TrackDetails.BaseHeight;
                var baseTop = Pos + Vector3.up * baseHeight;

                var baseWidth = TrackDetails.BaseWidth;
                var halfBaseWidth = baseWidth * .5f;

                var wallWidth = TrackDetails.WallWidth;
                var wallHeight = TrackDetails.WallHeight;

                var wallBottom = baseTop;
                var wallTop = baseTop + Vector3.up * wallHeight;
                var baseBottom = Pos;


                const int PointCount = 16;
                var startAngle = Mathf.Atan2(leftNormal.z, leftNormal.x);

                var verts = new Vector3[(PointCount + 1) * (4 + 4)];

                var angleStep = Mathf.PI / PointCount;

                for (int i=0;i <= PointCount; ++i)
                {
                    var angle = startAngle + i * angleStep;

                    var cos = Mathf.Cos(angle);
                    var sin = Mathf.Sin(angle);

                    var outerOffset = new Vector3(halfBaseWidth * cos, 0, halfBaseWidth * sin);
                    var innerOffset = new Vector3((halfBaseWidth - wallWidth) * cos, 0, (halfBaseWidth - wallWidth) * sin);

                    // Base
                    verts[i + 0] = baseBottom;
                    verts[i + 1] = baseTop;
                    verts[i + 2] = baseTop + outerOffset;
                    verts[i + 3] = baseBottom + outerOffset;

                    // Left Wall
                    verts[i + 4] = wallBottom + innerOffset;
                    verts[i + 5] = wallTop + innerOffset;
                    verts[i + 6] = wallTop + outerOffset;
                    verts[i + 7] = wallBottom + outerOffset;

                    // Right Wall
                    verts[i + 4] = wallBottom - innerOffset;
                    verts[i + 5] = wallTop - innerOffset;
                    verts[i + 6] = wallTop - outerOffset;
                    verts[i + 7] = wallBottom - outerOffset;
                }

                // 8 for the base (top, bottom, left, right sides) & 6 for each wall (top, left, right sides)
                var tris = new int[PointCount * 3 * (8 + 6 + 6)];

                for (int i = 0, tIndex = 0; i < PointCount;++i)
                {
                    var vStart = i * 12;

                    // Base 

                    // Top
                    tris[tIndex++] = vStart + 1;
                    tris[tIndex++] = vStart + 13;
                    tris[tIndex++] = vStart + 14;

                    tris[tIndex++] = vStart + 1;
                    tris[tIndex++] = vStart + 14;
                    tris[tIndex++] = vStart + 2;

                    // Left
                    tris[tIndex++] = vStart + 0;
                    tris[tIndex++] = vStart + 12;
                    tris[tIndex++] = vStart + 13;

                    tris[tIndex++] = vStart + 0;
                    tris[tIndex++] = vStart + 13;
                    tris[tIndex++] = vStart + 1;

                    // Right
                    tris[tIndex++] = vStart + 2;
                    tris[tIndex++] = vStart + 14;
                    tris[tIndex++] = vStart + 15;

                    tris[tIndex++] = vStart + 2;
                    tris[tIndex++] = vStart + 15;
                    tris[tIndex++] = vStart + 3;
                }

                EndCapStructureMesh.vertices = verts;
                EndCapStructureMesh.triangles = tris;

                EndCapStructureMesh.RecalculateNormals();
                EndCapStructureMesh.RecalculateBounds();
            }
        }



        private class MeshPool
        {
            public Mesh AllocateMesh()
            {
                var mesh = m_availableMeshes.Count > 0 ? m_availableMeshes[0] : null;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    // In editor the meshes are lost, reallocate if missing
                    if (mesh == null && m_availableMeshes.Count > 0)
                    {
                        m_availableMeshes.Clear();
                        m_allocatedMeshes.Clear();

                        for (int i = 0; i < MaxMeshes; ++i)
                        {
                            m_availableMeshes.Add(new Mesh { name = (MaxMeshes - i).ToString() });
                        }

                        mesh = m_availableMeshes[0];
                    }
                }
#endif

                if (mesh)
                {
                    m_availableMeshes.RemoveAt(0);
                    m_allocatedMeshes.Add(mesh);
                    mesh.Clear();
                }

                return mesh;
            }



            public void FreeMesh(Mesh mesh)
            {
                if (m_allocatedMeshes.Contains(mesh))
                {
                    m_allocatedMeshes.Remove(mesh);
                    m_availableMeshes.Insert(0, mesh);
                }
            }



            public MeshPool()
            {
                for(int i=0;i < MaxMeshes; ++i)
                {
                    m_availableMeshes.Add(new Mesh { name = (MaxMeshes - i).ToString() });
                }
            }



            private readonly List<Mesh>    m_availableMeshes = new List<Mesh>();
            private readonly HashSet<Mesh> m_allocatedMeshes = new HashSet<Mesh>();
        }



        public class WaterMeshData
        {
            public readonly Vector3[]   Verts;
            public readonly Vector3[]   Normals;
            public readonly Vector2[]   UVs;
            public readonly int[]       Triangles;



            // Vert order: BL (near left), TL (far left), TR (far right), BR (near right)
            public void AddSegmentData(int segmentIndex, Vector3[] verts, Vector3[] normals, Vector2[] uvs)
            {
                // First segment adds all 4 verts, subsequent segments add in their far 2.
                if(segmentIndex > 0)
                {
                    var sourceIndex = 1; // Adding only TL & TR
                    for (int i = m_vCount; i < m_vCount + 2; ++i, ++sourceIndex)
                    {
                        Verts[i] = verts[sourceIndex];
                        Normals[i] = normals[sourceIndex];
                        UVs[i] = uvs[sourceIndex];
                    }
                    m_vCount += 2;
                }
                else
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        // Need to remap order so we have BL BR TL TR so TL TR can be continuously added without changing triangle indexing
                        // Not changing incoming vert order as may revert, if not this can be changed and the Remap removed
                        var v = VertsRemap[i];

                        Verts[i] = verts[v];
                        Normals[i] = normals[v];
                        UVs[i] = uvs[v];
                    }
                    m_vCount += 4;
                }

                Triangles[m_triIndex++] = m_vStart + 0;
                Triangles[m_triIndex++] = m_vStart + 2;
                Triangles[m_triIndex++] = m_vStart + 1;

                Triangles[m_triIndex++] = m_vStart + 2;
                Triangles[m_triIndex++] = m_vStart + 3;
                Triangles[m_triIndex++] = m_vStart + 1;

                m_vStart += 2;
            }



            public WaterMeshData(int segmentCount)
            {
                var vCount  = 2 + segmentCount * 2;
                Verts       = new Vector3[vCount];
                Normals     = new Vector3[vCount];
                UVs         = new Vector2[vCount];
                Triangles   = new int[segmentCount * 3 * 2];
            }



            private int m_vStart, m_vCount, m_triIndex;

            private static readonly int[] VertsRemap = 
            {
                0, 3, 1, 2
            };
        }



        private static readonly MeshPool ms_meshPool = new MeshPool();
    }
}
