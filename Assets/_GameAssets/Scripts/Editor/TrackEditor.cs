#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;



namespace SpeedyBoat
{
    [CustomEditor(typeof(Track))]
    public class TrackEditor : Editor
    {
        public void OnSceneGUI()
        {
            var track = (Track)target;

            if (!Application.isPlaying && track.Rebuild)
            {
                track.EditorRender();
            }
        }
    }
}
#endif