
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System.Linq;
using System.Collections;
using System.Text;
using UnityEngine;

public class AllocationsTest
{
    public struct MemoryWatch
    {
        const int MaxNoGCMemory = 1024 * 1024 * 10;

        long startSize;
        long endSize;

        public void Start()
        {
            GC.Collect();
            startSize = Profiler.GetMonoUsedSizeLong();

        }

        public void Stop()
        {
            endSize = Profiler.GetMonoUsedSizeLong();
            GC.Collect();
        }

        public long Allocations => endSize - startSize;

        public void AssertNoAlloc(string contextMessage)
        {
            if (0 != Allocations)
            {
                Debug.Log($"Allocated {Allocations / (1024 * 1024.0):F2} MB: {contextMessage}");
            } else
            {
                Debug.Log($"No allocations: {contextMessage}");
            }

            Assert.AreEqual(0, Allocations, $"Allocated {Allocations / (1024 * 1024.0):F2} MB: {contextMessage}");

        }
    }

    const int Seconds = 120;
    const int FramesPerSecond = 60;

    [Test]
    public void IntToStringAllocations()
    {
        MemoryWatch watch = new MemoryWatch();
        var random = new System.Random();
        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                string s = random.Next().ToString();
                l += s.Length;
            }

        }

        watch.Stop();
        watch.AssertNoAlloc($"One Int to string conversion per frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    private List<int> GetRandomList()
    {
        var random = new System.Random();

        var list = new List<int>();
        for (int i = 0; i < 100; ++i)
        {
            list.Add(random.Next(0, 200));
        }

        return list;
    }

    [Test]
    public void ClosureAllocation()
    {
        MemoryWatch watch = new MemoryWatch();
        var randomList = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                Func<int> closure = () => randomList[i % randomList.Count];
                l += closure();
            }

        }

        watch.Stop();
        watch.AssertNoAlloc($"Closure with single capture creation for at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void LambdaAllocation()
    {
        MemoryWatch watch = new MemoryWatch();
        var randomList = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                // No context capture!
                Func<int, int> lambda = (int x) => x;
                l += lambda(i);
            }

        }

        watch.Stop();
        watch.AssertNoAlloc($"Closure with single capture creation for at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    private interface IDelegate
    {
        void OnCalled();
    }

    private class DelegateListener : IDelegate
    {
        public void OnCalled() { }
    }



    [Test]
    public void ForeachLambdaAllocation()
    {
        MemoryWatch watch = new MemoryWatch();
        var delegates = new List<IDelegate>();
        delegates.Add(new DelegateListener());
        delegates.Add(new DelegateListener());

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                delegates.ForEach(x => x.OnCalled());
                l += i;
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Foreach lambda allocation for at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void ForeachLoopList()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                foreach (var x in list)
                {
                    l += x;
                }
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"For each per frame on list at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void ForeachLoopArray()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList().ToArray();

        watch.Start();

        long l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                foreach (var x in list)
                {
                    l += x;
                }
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"For each per frame on array at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void LinqSelectAndSumFilter()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                l += list.Select(x => x / 2).Sum();
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Each frame select + sum on list of length 100 at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    private IEnumerator SimpleCoroutine(int xx)
    {
        int result = 0;
        for (int i = 0; i < 10; ++i)
        {
            result += xx;
            yield return null;
        }
    }

    [Test]
    public void Coroutine()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                // I needed to add an argument or it was optimized out
                var coroutine = SimpleCoroutine(i);
                while (coroutine.MoveNext())
                {
                    if (coroutine.Current == null)
                    {
                        l += 1;
                    }
                }
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Coroutine start per frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void StringBuilder()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                var builder = new StringBuilder();

                for (int ii = 0; ii < 10; ++ii)
                {
                    builder.Append(list[i]);
                }

                // We need string at the end
                var s = builder.ToString();
                l += s.Length;
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Builder combining 10 integers per frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void StringBuilderCapacity()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                var builder = new StringBuilder(10 * 4);

                for (int ii = 0; ii < 10; ++ii)
                {
                    builder.Append(list[i]);
                }

                // We need string at the end
                var s = builder.ToString();
                l += s.Length;
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Builder combining 10 integers with capacity 400 per frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void StringBuilderReuse()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();
        var builder = new StringBuilder();
        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                builder.Clear();

                for (int ii = 0; ii < 10; ++ii)
                {
                    builder.Append(list[i]);
                }

                // We need string at the end
                var s = builder.ToString();
                l += s.Length;
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Builder reuse clear combining 10 integers per frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void StringNormalConcat()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                var s = "";

                for (int ii = 0; ii < 10; ++ii)
                {
                    s += list[i].ToString();
                }

                l += s.Length;
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"String combining 10 integers per frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    private class Controller
    {
        int data1;
        int data2 = 1;
        int data3 = 21;
        long data4 = 4;
        int data5 = 3123;

        public Controller()
        {

        }

        public void Reuse(int newData)
        {
            data1 = newData;
        }

        public long Sum => data1 + data2 + data3 + data4 + data5;
    }

    [Test]
    public void CreatingLightweightController()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        long l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                var controller = new Controller();
                l += controller.Sum;
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Creating a 24 byte controller each frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    [Test]
    public void ReusingController()
    {
        MemoryWatch watch = new MemoryWatch();
        var list = GetRandomList();

        watch.Start();

        long l = 0;
        {
            var controller = new Controller();

            for (int j = 0; j < Seconds; ++j)
            {
                for (int i = 0; i < FramesPerSecond; ++i)
                {
                    controller.Reuse(i);
                    l += controller.Sum;
                }
            }
        }

        watch.Stop();
        watch.AssertNoAlloc($"Reusing a 24 byte controller each frame at {FramesPerSecond} FPS for {Seconds} s of game");

        // Just so we force l to be used
        Assert.Less(0, l);
    }


}
