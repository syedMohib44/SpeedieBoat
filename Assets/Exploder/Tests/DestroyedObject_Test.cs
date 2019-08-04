using System.Collections;
using Exploder;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ExploderTesting
{
    class DestroyedObject_Test : TestCase
    {
        public DestroyedObject_Test()
        {
        }

        protected override IEnumerator RunTest()
        {
            var finished = false;

            var testObj = Tester.testObjects[0];
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = testObj.transform.position;

            ExploderUtils.SetActiveRecursively(obj, true);
            GameObject.DestroyImmediate(obj);

            Exploder.ExplodeObject(obj, (ms, state) =>
            {
                if (state == ExploderObject.ExplosionState.ExplosionFinished)
                {
                    OnTestSuccess();
                    finished = true;
                }
            });

            yield return new WaitUntil((() => finished));
        }
    }
}
