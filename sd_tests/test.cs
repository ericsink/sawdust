
/*
 * Copyright 2006 Eric Sink
 * 
 * You may use this code under the terms of any of the
 * following licenses, your choice:
 * 
 * 1)  The GNU General Public License, Version 2
 *      http://www.opensource.org/licenses/gpl-license.php
 * 2)  The Apache License, Version 2.0
 *      http://www.opensource.org/licenses/apache2.0.php
 * 3)  The MIT License
 *      http://www.opensource.org/licenses/mit-license.php
 *
 * I am publishing this code on my blog as sample
 * code.  I am not intending to maintain this as a 
 * regular ongoing open source project or
 * anything like that.  I'm not looking for contributors
 * or hosting on sourceforge or anything like that.
 * 
 * Nonetheless, I hate it when I see an article with
 * sample code and it's not clear if I am allowed to
 * use the code or not.  The ambiguity is annoyoing.
 * So I am making this code available under your 
 * choice of open source licenses as described below.  
 * Informally and in a nutshell, you can use this code 
 * for any purpose as long as I am not liable for anything 
 * that goes wrong.  For more tedious and formal explanation,
 * pick one of the licenses above and use it.
 * 
 */


#if DEBUG

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace EricSinkMultiCoreLib
{
    [TestFixture]
    public class test_map
    {
        private void helper_EmptyLists()
        {
            multicore.MapDelegate_ResultsIndexed<int, int> del1 = delegate (int x) { return x * 2; };
            multicore.MapDelegate_ResultsAppended<int, int> del2 = delegate (int x) { return x * 2; };
            multicore.MapDelegate_Void<int> del3 = delegate (int x) { int y = x * 2; };
            multicore.MapDelegate_AnyTrue<int> del4 = delegate (int x) { return IsPrime(x); };

            List<int> inputs = new List<int>();

            Assert.AreEqual(0, multicore.Map_ResultsIndexed<int, int>(inputs, del1).Length);

            List<int> results2 = new List<int>();
            multicore.Map_ResultsAppended(inputs, del2, results2);
            Assert.AreEqual(0, results2.Count);

            multicore.Map_Void(inputs, del3);

            Assert.IsFalse(multicore.Map_AnyTrue(inputs, del4));

            inputs.Add(32);

            Assert.AreEqual(1, multicore.Map_ResultsIndexed<int, int>(inputs, del1).Length);
            multicore.Map_ResultsAppended(inputs, del2, results2);
            Assert.AreEqual(1, results2.Count);

            multicore.Map_Void(inputs, del3);

            Assert.IsFalse(multicore.Map_AnyTrue(inputs, del4));
        }

        private void helper_ResultsIndexed(int count)
        {
            List<int> inputs = new List<int>();
            for (int i = 0; i < count; i++)
            {
                inputs.Add(i);
            }

            int[] results = multicore.Map_ResultsIndexed<int, int>(inputs, delegate (int x) { return x * 2; });
            Assert.AreEqual(inputs.Count, results.Length);

            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(inputs[i] * 2, results[i]);
            }
        }

        private void helper_ResultsAppended(int count)
        {
            List<int> inputs = new List<int>();
            for (int i = 0; i < count; i++)
            {
                inputs.Add(i);
            }

            List<int> results = new List<int>();
            multicore.Map_ResultsAppended(inputs, delegate (int x) { return x * 2; }, results);
            Assert.AreEqual(inputs.Count, results.Count);

            int sum1 = 0;
            int sum2 = 0;
            for (int i = 0; i < count; i++)
            {
                sum1 += inputs[i];
                sum2 += results[i];
            }
            Assert.AreEqual(sum1 * 2, sum2);
        }

        public void helper_void(int count)
        {
            int counter = 0;
            int[] inputs = new int[count];
            multicore.Map_Void(inputs, delegate (int x) { Interlocked.Increment(ref counter); });
            Assert.AreEqual(count, counter);
        }

        private int WeirdFunc(int x)
        {
            int count = 0;
            while (true)
            {
                ++count;
                if (x == 1)
                {
                    break;
                }
                else if ((x % 2) == 0)
                {
                    x = x / 2;
                }
                else
                {
                    x = x * 3 + 1;
                }
            }
            return count;
        }

        private long Factorial(long x)
        {
            if (x > 1)
            {
                return x * Factorial(x - 1);
            }
            else
            {
                return 1;
            }
        }

        [Test]
        public void test_Factorial()
        {
            Assert.AreEqual(120, Factorial(5));
            Assert.AreEqual(362880, Factorial(9));
            Assert.AreEqual(355687428096000, Factorial(17));
            Assert.AreEqual(2432902008176640000, Factorial(20));
        }

        private bool IsPrime(int x)
        {
            if (x < 2)
            {
                return false;
            }
            if (x == 2)
            {
                return true;
            }
            int count = 0;
            for (int i = 2; i <= x; i++)
            {
                int m = x % i;
                if (m == 0)
                {
                    count++;
                    if (count > 1)
                    {
                        return false;
                    }
                }
            }
            return (count == 1);
        }

        private bool AnyPrime(params int[] vals)
        {
            return multicore.Map_AnyTrue(vals, delegate (int x) { return IsPrime(x); });
        }

        [Test]
        public void test_isPrime()
        {
            Assert.IsFalse(IsPrime(0));
            Assert.IsFalse(IsPrime(1));

            Assert.IsTrue(IsPrime(2));
            Assert.IsTrue(IsPrime(3));
            Assert.IsTrue(IsPrime(5));
            Assert.IsTrue(IsPrime(7));
            Assert.IsTrue(IsPrime(11));
            Assert.IsTrue(IsPrime(13));
            Assert.IsTrue(IsPrime(17));
            Assert.IsTrue(IsPrime(19));
            Assert.IsTrue(IsPrime(53));

            Assert.IsFalse(IsPrime(4));
            Assert.IsFalse(IsPrime(6));
            Assert.IsFalse(IsPrime(9));
            Assert.IsFalse(IsPrime(12));
            Assert.IsFalse(IsPrime(36));
            Assert.IsFalse(IsPrime(81));
        }

        private void helper_AnyPrime()
        {
            Assert.IsTrue(AnyPrime(2, 3, 4, 5, 6, 7, 8, 9));
            Assert.IsTrue(AnyPrime(73, 79, 83, 89, 97, 101, 103));
            Assert.IsTrue(AnyPrime(15485863, 4));
            Assert.IsTrue(AnyPrime(3));
            Assert.IsFalse(AnyPrime(4));
            Assert.IsFalse(AnyPrime(4, 8, 16, 32, 64, 128, 256));
            Assert.IsTrue(AnyPrime(4, 8, 16, 32, 64, 128, 256, 73));
            Assert.IsTrue(AnyPrime(4, 8, 16, 32, 64, 128, 256, 15485863));
            Assert.IsTrue(AnyPrime(15485863, 9283744, 3498752, 239876, 9876554, 1111112, 33332, 554, 88754));
            Assert.IsFalse(AnyPrime(44, 9283744, 3498752, 239876, 9876554, 1111112, 33332, 554, 88754));
        }

        private void helper_runtests()
        {
            helper_AnyPrime();

            helper_void(50);
            helper_void(1);

            helper_ResultsAppended(1000);
            helper_ResultsAppended(100);
            helper_ResultsAppended(1);

            helper_ResultsIndexed(1000);
            helper_ResultsIndexed(100);
            helper_ResultsIndexed(1);

            helper_EmptyLists();
        }

        [Test]
        public void test_AlwaysUseThreads()
        {
            multicore.Mode = MultiCoreMode.AlwaysUseThreads;

            helper_runtests();
        }

        [Test]
        public void test_NeverUseThreads()
        {
            multicore.Mode = MultiCoreMode.NeverUseThreads;

            helper_runtests();
        }

        private void helper_weirdfunc()
        {
            List<int> inputs = new List<int>();
            for (int i = 1234; i <= 1245; i++)
            {
                inputs.Add(i);
            }

            int[] results = multicore.Map_ResultsIndexed<int, int>(inputs,
                delegate (int x)
                {
                    int j = 0;
                    for (int n = 0; n < 100000; n++)
                    {
                        j += WeirdFunc(x);
                    }
                    return j;
                }
            );
            Assert.AreEqual(inputs.Count, results.Length);
        }

        private void helper_factorials()
        {
            for (int q = 0; q < 10; q++)
            {
                List<long> inputs = new List<long>();
                for (long i = 3; i <= 20; i++)
                {
                    inputs.Add(i);
                }

                long[] results = multicore.Map_ResultsIndexed<long, long>(inputs,
                    delegate (long x)
                    {
                        for (int n = 0; n < 10000; n++)
                        {
                            long ff = Factorial(x);
                        }
                        return Factorial(x);
                    }
                    );
                Assert.AreEqual(inputs.Count, results.Length);
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

        private void do_timed(stopwatchdelegate func, int num_iterations)
        {
            // initialize the times to 0

            long time_NeverUseThreads = 0;
            long time_AlwaysUseThreads = 0;
            long time_MaybeUseThreads = 0;

            // run the tests
            for (int i = 0; i < num_iterations; i++)
            {
                multicore.Mode = MultiCoreMode.AlwaysUseThreads;
                time_AlwaysUseThreads += timeit(func);

                multicore.Mode = MultiCoreMode.NeverUseThreads;
                time_NeverUseThreads += timeit(func);

                multicore.Mode = MultiCoreMode.UseThreadsIfMultipleProcessorsPresent;
                time_MaybeUseThreads += timeit(func);
            }

            Console.WriteLine("ProcessorCount:   {0}", System.Environment.ProcessorCount);

            Console.WriteLine("NeverUseThreads:  {0}", time_NeverUseThreads);
            Console.WriteLine("AlwaysUseThreads: {0}", time_AlwaysUseThreads);
            Console.WriteLine("MaybeUseThreads:  {0}", time_MaybeUseThreads);

            Console.WriteLine("Improvement:      {0}%", (time_NeverUseThreads - time_AlwaysUseThreads) * 100 / time_NeverUseThreads);
        }

        [Test]
        public void test_factorials_timed()
        {
            do_timed(helper_factorials, 3);
        }

        [Test]
        public void test_weirdfunc_timed()
        {
            do_timed(helper_weirdfunc, 3);
        }
    }
}

#endif
