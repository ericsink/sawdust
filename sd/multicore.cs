
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


using System;
using System.Collections.Generic;
using System.Threading;

namespace EricSinkMultiCoreLib
{
    /// <summary>
    /// This enum contains the three possible values for
    /// the Mode property, which is used to configure how
    /// threads will or will not be used by the Map functions.
    /// </summary>
    public enum MultiCoreMode
    {
        AlwaysUseThreads,
        NeverUseThreads,
        UseThreadsIfMultipleProcessorsPresent
    }

    /// <summary>
    /// This class contains things which help an application use
    /// multiple CPUs/processors/cores.
    /// 
    /// More specifically, this class contains several implementations
    /// of "Map".  The name Map comes from the concepts of Map and
    /// Reduce in functional programming.  I'm no expert in that
    /// area, so I'm probably misusing the term in some way.  
    /// I apologize in advance for this egregious offense.
    /// 
    /// Anyway, the basic idea is this:
    /// 
    /// Instead of running a loop over every item in a list or
    /// array, pass your list and your code fragment into map.
    /// By allowing map to handle things, you gain the benefits
    /// of using more than one CPU or processor core.  Instead
    /// of calling your code fragment sequentially once for each 
    /// item in the list, map will use multiple threads to make
    /// those calls happen in parallel.
    /// 
    /// Obviously, if you need the items in your list to be
    /// processed in order, don't use map.  The order in which
    /// your items will be handled is not defined.
    /// 
    /// Furthermore, the arguments you pass into map need to
    /// be thread-safe.  Ideally, your threads and list of
    /// arguments aren't going to be sharing any data, certainly
    /// not trying to modify any shared data.  In practice, this
    /// isn't always the easiest way to do things, so you'll need
    /// to use locks.
    /// 
    /// Map is not a panacea.  The hardest part of using map
    /// is getting your code into a state where it can be
    /// safely and usefully executed in parallel.
    /// 
    /// Note that there is some overhead in using map.  The
    /// performance improvement of map is best when you have
    /// a few items, each of which is a big job.  If you have
    /// lots of very small tasks to perform, the overhead of
    /// creating threads will be greater than the time saved
    /// by executing them in parallel.
    /// 
    /// I have tested this code only with Visual Studio 2005
    /// under Windows XP.  I'll assume that no other compiler
    /// or environment will work, but I don't actually know.
    /// 
    /// For more information, and for the sources of inspiration
    /// I used in writing this code:
    /// 
    /// http://www.ookii.org/showpost.aspx?post=8
    /// 
    /// http://www.codeguru.com/columns/experts/article.php/c4767/
    /// 
    /// http://www.devx.com/amd/Article/32301
    /// 
    /// http://www.joelonsoftware.com/items/2006/08/01.html
    /// 
    /// http://labs.google.com/papers/mapreduce.html
    /// 
    /// http://msdn.microsoft.com/msdnmag/issues/06/09/CLRInsideOut/default.aspx
    /// 
    /// http://en.wikipedia.org/wiki/MapReduce
    /// 
    /// </summary>
    public class multicore
    {
        private static bool _useThreads = (System.Environment.ProcessorCount > 1);

        /// <summary>
        /// Mode is a static class variable which determines
        /// how threads are to be used.  There are three
        /// possible values:
        /// 
        /// AlwaysUseThreads -- The Map functions use threads whether
        /// multiple CPUs are present or not.
        /// 
        /// NeverUseThreads -- The Map functions do not use threads.
        /// 
        /// UseThreadsIfMultipleProcessorsPresent -- The default.
        /// Threads are used if multiple cores are available.
        /// </summary>
        public static MultiCoreMode Mode
        {
            set
            {
                if (value == MultiCoreMode.AlwaysUseThreads)
                {
                    _useThreads = true;
                }
                else if (
                    (value == MultiCoreMode.UseThreadsIfMultipleProcessorsPresent)
                    && (System.Environment.ProcessorCount > 1)
                    )
                {
                    _useThreads = true;
                }
                else
                {
                    _useThreads = false;
                }
            }
        }

        public delegate T2 MapDelegate_ResultsIndexed<T, T2>(T input);
        public delegate T2 MapDelegate_ResultsAppended<T, T2>(T input);
        public delegate bool MapDelegate_AnyTrue<T>(T input);
        public delegate void MapDelegate_Void<T>(T input);

        /// <summary>
        /// Every version of Map accepts a list of inputs and a delegate 
        /// function which will be called once for each item in that list.
        /// 
        /// This version assumes that the delegate will return a value,
        /// and all of those return values will be returned in an array.
        /// The resulting array will have all the results in the same order
        /// as the inputs.
        /// </summary>
        /// <typeparam name="T">The type of the arguments to the function</typeparam>
        /// <typeparam name="T2">The type of the result from the function</typeparam>
        /// <param name="list">The list of function inputs</param>
        /// <param name="function">The function to be applied to each item in the list</param>
        /// <returns>An array of results</returns>
        public static T2[] Map_ResultsIndexed<T, T2>(IList<T> list, MapDelegate_ResultsIndexed<T, T2> function)
        {
            T2[] result = new T2[list.Count];
            if (list.Count == 0)
            {
                // nothing to do.  result will get returned empty.
            }
            else if (list.Count == 1)
            {
                result[0] = function(list[0]);
            }
            else
            {
                if (_useThreads)
                {
                    using (ManualResetEvent done = new ManualResetEvent(false))
                    {
                        int countdown = list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {
                            ThreadPool.QueueUserWorkItem(
                                delegate (object obj)
                                {
                                    int ndx = (int)obj;
                                    result[ndx] = function(list[ndx]);

                                    if (Interlocked.Decrement(ref countdown) == 0)
                                    {
                                        done.Set();
                                    }
                                }, i
                            );
                        }
                        done.WaitOne();
                    }
                }
                else
                {
                    for (int ndx = 0; ndx < list.Count; ndx++)
                    {
                        result[ndx] = function(list[ndx]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Every version of Map accepts a list of inputs and a delegate 
        /// function which will be called once for each item in that list.
        /// 
        /// This version assumes that the delegate will return a value,
        /// and all of those return values will be appended to a list which
        /// is passed as the third parameter.  The results list will be in
        /// no particular order.  The results list does not need to be
        /// initially empty.
        /// </summary>
        /// <typeparam name="T">The type of the arguments to the function</typeparam>
        /// <typeparam name="T2">The type of the result from the function</typeparam>
        /// <param name="list">The list of function inputs</param>
        /// <param name="function">The function to be applied to each item in the list</param>
        /// <param name="result">A list onto which all the results will be appended.</param>
        public static void Map_ResultsAppended<T, T2>(IList<T> list, MapDelegate_ResultsAppended<T, T2> function, List<T2> result)
        {
            if (list.Count == 0)
            {
                // nothing to do here
            }
            else if (list.Count == 1)
            {
                result.Add(function(list[0]));
            }
            else
            {
                if (_useThreads)
                {
                    using (ManualResetEvent done = new ManualResetEvent(false))
                    {
                        int countdown = list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {
                            ThreadPool.QueueUserWorkItem(
                                delegate (object obj)
                                {
                                    T q = (T)obj;
                                    T2 fr = function(q);
                                    lock (result)
                                    {
                                        result.Add(fr);
                                    }

                                    if (Interlocked.Decrement(ref countdown) == 0)
                                    {
                                        done.Set();
                                    }
                                }, list[i]
                            );
                        }
                        done.WaitOne();
                    }
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        result.Add(function(list[i]));
                    }
                }
            }
        }

        /// <summary>
        /// Every version of Map accepts a list of inputs and a delegate 
        /// function which will be called once for each item in that list.
        /// 
        /// This version assumes the delegate returns bool and will
        /// return true if ANY of the function calls returns true.
        /// 
        /// Unlike most versions of Map, this version may not wait until
        /// all the function calls are completed.  If any function returns
        /// true, Map_AnyTrue will return true as quickly as possible.
        /// </summary>
        /// <typeparam name="T">The type of the arguments to the function</typeparam>
        /// <param name="list">The list of function inputs</param>
        /// <param name="function">The function to be applied to each item in the list</param>
        /// <returns>true iff ANY of the function calls returns true</returns>
        public static bool Map_AnyTrue<T>(IList<T> list, MapDelegate_AnyTrue<T> function)
        {
            if (list.Count == 0)
            {
                return false;
            }
            else if (list.Count == 1)
            {
                return function(list[0]);
            }
            else
            {
                if (_useThreads)
                {
                    using (ManualResetEvent done = new ManualResetEvent(false))
                    {
                        int countdown = list.Count;
                        bool result = false;
                        for (int i = 0; i < list.Count; i++)
                        {
                            ThreadPool.QueueUserWorkItem(
                                delegate (object obj)
                                {
                                    if (result)
                                    {
                                        return;
                                    }

                                    T q = (T)obj;
                                    if (function(q))
                                    {
                                        if (result)
                                        {
                                            return;
                                        }

                                        result = true;

                                        try
                                        {
                                            done.Set();
                                        }
                                        catch (ObjectDisposedException)
                                        {
                                        }
                                        return;
                                    }

                                    if (result)
                                    {
                                        return;
                                    }

                                    if (Interlocked.Decrement(ref countdown) == 0)
                                    {
                                        try
                                        {
                                            done.Set();
                                        }
                                        catch (ObjectDisposedException)
                                        {
                                        }
                                    }
                                },
                                list[i]
                            );
                        }
                        done.WaitOne();
                        return result;
                    }
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (function(list[i]))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Every version of Map accepts a list of inputs and a delegate 
        /// function which will be called once for each item in that list.
        /// 
        /// This version returns nothing (void).  The delegate function must
        /// return void.
        /// </summary>
        /// <typeparam name="T">The type of the arguments to the function</typeparam>
        /// <param name="list">The list of function inputs</param>
        /// <param name="function">The function to be applied to each item in the list</param>
        public static void Map_Void<T>(IList<T> list, MapDelegate_Void<T> function)
        {
            if (list.Count == 0)
            {
                return;
            }
            else if (list.Count == 1)
            {
                function(list[0]);
            }
            else
            {
                if (_useThreads)
                {
                    using (ManualResetEvent done = new ManualResetEvent(false))
                    {
                        int countdown = list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {
                            ThreadPool.QueueUserWorkItem(
                                delegate (object obj)
                                {
                                    T q = (T)obj;
                                    function(q);

                                    if (Interlocked.Decrement(ref countdown) == 0)
                                    {
                                        done.Set();
                                    }
                                },
                                list[i]
                            );
                        }
                        done.WaitOne();
                    }
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        function(list[i]);
                    }
                }
            }
        }
    }
}

/*

Thanks:
 * 
 * to Michael Davis, for noticing that I wasn't handling zero-length
 * input lists properly.
 * 
 * to Marco Borasio for noticing that in Map_AnyTrue, the ManualResetEvent
 * might already be disposed when one of the threads tries to call its
 * done() method.

*/
