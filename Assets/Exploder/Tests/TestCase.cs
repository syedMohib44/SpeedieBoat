using System.Collections;
using System.Diagnostics;
using Exploder;
using Exploder.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ExploderTesting
{
    abstract class TestCase
    {
        protected TestCase()
        {
            watch = new Stopwatch();
        }

        public virtual IEnumerator Run()
        {
            watch.Start();
            yield return RunTest();
            watch.Stop();
        }

        protected ExploderObject Exploder { get { return ExploderSingleton.Instance; } }

        protected ExploderTester Tester {  get { return ExploderTester.Instance; } }

        protected abstract IEnumerator RunTest();

        protected void OnTestSuccess()
        {
            Debug.LogFormat("Test success {0}", ToString());
        }

        protected void OnTestFailed(string reason)
        {
            Debug.LogErrorFormat("Test failed {0}, reason: {1}", ToString(), reason);
//            Debug.Assert(false);
        }

        private Stopwatch watch;
    }
}
