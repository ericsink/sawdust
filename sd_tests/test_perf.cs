#if DEBUG

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using EricSinkMultiCoreLib;

using NUnit.Framework;

#if false

namespace sd
{
	[TestFixture]
	public class test_perf
	{
        private void do_plans()
        {
            Builtin_Plans.CreateBookShelf().Execute();
            Builtin_Plans.CreateFamilyRoomShelf().Execute();
            Builtin_Plans.CreateTable().Execute();
            Builtin_Plans.CreateMiteredBoard().Execute();
        }

        [Test]
        public void do_plans_threaded()
        {
            List<Plan> plans = new List<Plan>();
            plans.Add(Builtin_Plans.CreateBookShelf());
            plans.Add(Builtin_Plans.CreateFamilyRoomShelf());
            plans.Add(Builtin_Plans.CreateTable());
            plans.Add(Builtin_Plans.CreateMiteredBoard());

            multicore.Map_Void(plans,
                delegate(Plan p)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        p.Execute();
                    }
                }
            );
        }

        [Test]
        public void do_standard_32plan_test()
        {
            // On my Vaio, this was 85 seconds.
            // Then I added multicore stuff and it went to 75.
            // Then I fixed CompoundSolid.Clone and it went to 45
            // Then I added a bb check to bool3d.Subtract and it went to 38
            // Remove GlueJoints stuff and it goes to 24

            for (int i = 0; i < 32; i++)
            {
                do_plans();
            }
        }

        private delegate void stopwatchdelegate();

        private static long timeit(stopwatchdelegate func)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            func();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        [Test]
        public void test_plans_timed()
        {
            long time_without = 0;
            long time_with = 0;

            for (int i = 0; i < 4; i++)
            {
                multicore.Mode = MultiCoreMode.NeverUseThreads;
                time_without += timeit(do_plans);
                multicore.Mode = MultiCoreMode.AlwaysUseThreads;
                time_with += timeit(do_plans);
            }

            Console.WriteLine("Without threads: {0}", time_without);
            Console.WriteLine("With    threads: {0}", time_with);
            Console.WriteLine("ProcessorCount:  {0}", System.Environment.ProcessorCount);
            Console.WriteLine("Improvement:     {0}%", (time_without - time_with) * 100 / time_without);
        }
    }
}
#endif

#endif
