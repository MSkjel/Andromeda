using InfinityScript;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromeda
{
    public static class Async
    {
        private class DetachedState
        {
            public readonly IEnumerator WaitAround;
            public DetachedState(float? waitDelays = null)
            {
                if (waitDelays.HasValue)
                    WaitAround = BaseScript.Wait(waitDelays.Value);
                else
                    WaitAround = BaseScript.WaitForFrame();
            }
        }

        public static object Detach(float? waitDelays = null)
            => new DetachedState(waitDelays);

        private static readonly object attach = new object();
        public static object Attach()
            => attach;

        public static void Start(IEnumerator func)
        {
        //    IEnumerator wrap(IEnumerator routine)
        //    {
        //        bool next = routine.MoveNext();
        //        IEnumerator waitAround = null;

        //        while(next)
        //        {
        //            switch(routine.Current)
        //            {
        //                case DetachedState ds:
        //                    waitAround = ds.WaitAround;
        //                    break;
        //                case object obj when (obj == attach):
        //                    waitAround = null;
        //                    break;
        //                default:
        //                    yield return routine.Current;
        //                    break;
        //            }

        //            if(waitAround == null)
        //                next = routine.MoveNext();
        //            else
        //            {
        //                var task = Task.Factory.StartNew(routine.MoveNext);

        //                while (!task.IsCompleted)
        //                    yield return waitAround;

        //                next = task.Result;
        //            }
        //        }
        //    }

            BaseScript.StartAsync(/*wrap(*/func/*)*/);
        }
    }
}
