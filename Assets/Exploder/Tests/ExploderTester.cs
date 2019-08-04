using System;
using System.Collections;
using System.Collections.Generic;
using Exploder;
using UnityEngine;

namespace ExploderTesting
{
    public class ExploderTester : MonoBehaviour
    {
        public GameObject[] testObjects;

        private List<TestCase> cases = new List<TestCase>(255);
        public static ExploderTester Instance;

        private void Start()
        {
            Instance = this;

            int[] array = {0, 1, 2, 5, 10, 50, 100, 200};

            //
            // test destroyed object
            //
            cases.Add(new DestroyedObject_Test());

            //
            // test explode object
            //
            foreach (var i in array)
            {
                cases.Add(new FragmentCountTest(i));
            }

            //
            // test multiple objects
            //
            foreach (var i in array)
            {
                cases.Add(new FragmentCountTestMultiple(i, UnityEngine.Random.Range(1, i)));
            }

            //
            // test crack
            //
            foreach (var i in array)
            {
                cases.Add(new FragmentCrackCount(i));
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 50), "Start Test"))
            {
                StartCoroutine(StartTesting());
            }
        }

        private IEnumerator StartTesting()
        {
            foreach (var testCase in cases)
            {
                yield return testCase.Run();

                yield return new WaitForSeconds(0.3f);
                FragmentPool.Instance.DeactivateFragments();
            }
        }
    }
}
