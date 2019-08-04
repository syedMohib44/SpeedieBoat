using System.Collections;
using System.Collections.Generic;
using Exploder;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ExploderTesting
{
    class FragmentCountTestMultiple : TestCase
    {
        private int count;
        private int target;

        public FragmentCountTestMultiple(int target, int count)
        {
            this.target = target;
            this.count = count;
        }

        public override string ToString()
        {
            return base.ToString() + " " + target + " " + count;
        }

        protected override IEnumerator RunTest()
        {
            Exploder.TargetFragments = target;

            var obj = Tester.testObjects[0];
            var objs = new GameObject[count];

            for (int i = 0; i < count; i++)
            {
                objs[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                objs[i].transform.position = obj.transform.position +
                                             UnityEngine.Random.onUnitSphere*UnityEngine.Random.Range(1.0f, 10.0f);
            }

            var finished = false;

            Exploder.ExplodeObjects((ms, state) =>
            {
                if (state == ExploderObject.ExplosionState.ExplosionFinished)
                {
                    var active = FragmentPool.Instance.GetActiveFragments();

                    if (active.Count == target)
                    {
                        OnTestSuccess();
                    }
                    else
                    {
                        OnTestFailed(string.Format("Invalid number of result fragmens: {0}, expected: {1}", active.Count, target));
                    }

                    finished = true;
                }
            }, objs);

            yield return new WaitUntil((() => finished));
        }
    }
}
