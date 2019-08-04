using UnityEngine;

namespace Exploder.Demo
{
    public class DemoClickPartialExplode : MonoBehaviour
    {
        public GameObject[] toCrack;

        private ExploderObject exploder;

        private void Start()
        {
            Application.targetFrameRate = 60;

            //
            // access exploder from singleton
            //
            exploder = Utils.ExploderSingleton.Instance;

            foreach (var obj in toCrack)
            {
                exploder.CrackObject(obj);
            }
        }

        private void Update()
        {
            // we hit the mouse button
            if (Input.GetMouseButtonDown(0))
            {
                Ray mouseRay;
                mouseRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;

                // we hit the object
                if (Physics.Raycast(mouseRay, out hitInfo))
                {
                    var obj = hitInfo.collider.gameObject;

                    exploder.ExplodePartial(obj, mouseRay.direction, hitInfo.point, 1.0f, (ms, state) =>
                    {
                        Debug.LogFormat("Explosion callback: {0}", state);
                    });
                }
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Reset"))
            {
            }

            if (GUI.Button(new Rect(10, 50, 100, 30), "NextScene"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            }
        }
    }
}
