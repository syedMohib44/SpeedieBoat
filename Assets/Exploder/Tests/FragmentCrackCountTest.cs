using System.Collections;
using Exploder;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ExploderTesting
{
    class FragmentCrackCount : TestCase
    {
        private int target;

        public FragmentCrackCount(int count)
        {
            target = count;
        }

        public override string ToString()
        {
            return base.ToString() + " " + target;
        }

        protected override IEnumerator RunTest()
        {
            Exploder.TargetFragments = target;

            var finished = false;

            var obj = Tester.testObjects[0];
            ExploderUtils.SetActiveRecursively(obj, true);

            Exploder.CrackObject(obj, (ms, state) =>
            {
                if (state == ExploderObject.ExplosionState.ObjectCracked)
                {
                    Exploder.ExplodeCracked(obj, (timeMs, explosionState) =>
                    {
                        if (explosionState == ExploderObject.ExplosionState.ExplosionFinished)
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
                    });
                }
            });

            yield return new WaitUntil((() => finished));
        }
    }
}
