
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System.Linq;
using System.Collections;
using System.Text;
using UnityEngine;
using System.IO;

public class AllocationsTest
{
    private List<string> results = new List<string>(1000);


    [TearDown]
    public void SaveResults()
    {
        File.WriteAllLines("results.txt", results.ToArray());
    }

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

        public string GetAllocationDesc(string contextMessage)
        {
            string message = GetMessage(Allocations);
            Debug.Log(message);

            //Assert.AreEqual(0, Allocations, $"Allocated {Allocations / (1024 * 1024.0):F2} MB: {contextMessage}");
            return message;

            string GetMessage(long allocations)
            {
                if (0 != allocations)
                {
                    return $"- Allocated {allocations / (1024 * 1024.0):F2} MB: {contextMessage}\n";
                }
                else
                {
                    return $"- No allocations: {contextMessage}\n";
                }
            }
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
        results.Add(watch.GetAllocationDesc($"One Int to string conversion per frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Closure with single capture creation for at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Lambda creation for at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Foreach lambda allocation for at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"For each per frame on list at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"For each per frame on array at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Each frame select + sum on list of length 100 at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Coroutine start per frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Builder combining 10 integers per frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Builder combining 10 integers with capacity 400 per frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Builder reuse clear combining 10 integers per frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"String combining 10 integers per frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Creating a 24 byte controller each frame at {FramesPerSecond} FPS for {Seconds} s of game"));

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
        results.Add(watch.GetAllocationDesc($"Reusing a 24 byte controller each frame at {FramesPerSecond} FPS for {Seconds} s of game"));

        // Just so we force l to be used
        Assert.Less(0, l);
    }



    [Test]
    public void LocalClosureAllocation()
    {
        MemoryWatch watch = new MemoryWatch();
        var randomList = GetRandomList();

        watch.Start();

        int l = 0;
        int ii = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                ii = i;
                l += LocalClosue();
            }

        }

        watch.Stop();
        results.Add(watch.GetAllocationDesc($"Local closure with single capture creation for at {FramesPerSecond} FPS for {Seconds} s of game"));

        // Just so we force l to be used
        Assert.Less(0, l);

        int LocalClosue()
        {
            return randomList[ii % randomList.Count];
        }
    }

    [Test]
    public void LocalFunctionAllocation()
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
                l += LocalFunction(i);
            }

        }

        watch.Stop();
        results.Add(watch.GetAllocationDesc($"Local function creation for each frame at {FramesPerSecond} FPS for {Seconds} s of game"));

        // Just so we force l to be used
        Assert.Less(0, l);

        int LocalFunction(int i)
        {
            return i;
        }
    }

    private event Action<bool> OnEvent;

    private void EventHandler( bool data)
    {
    }

    [Test]
    public void EventsAllocations()
    {
        MemoryWatch watch = new MemoryWatch();
        var randomList = GetRandomList();

        OnEvent += EventHandler;

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                OnEvent += EventHandler;
                OnEvent(true);
                OnEvent -= EventHandler;

                l += 1;
            }

        }

        watch.Stop();
        results.Add(watch.GetAllocationDesc($"Event add/remove for each frame at {FramesPerSecond} FPS for {Seconds} s of game"));

        // Just so we force l to be used
        Assert.Less(0, l);
    }

    interface IEvent
    {
        void EventHandler(bool data);
    }

    class HandlerEvent : IEvent
    {
        public void EventHandler(bool data)
        {
        }
    }

    [Test]
    public void InterfaceEventsAllocations()
    {
        MemoryWatch watch = new MemoryWatch();
        var randomList = GetRandomList();

        List<IEvent> listeners = new List<IEvent>(10);

        var handler = new HandlerEvent();
        listeners.Add(handler);

        watch.Start();

        int l = 0;
        for (int j = 0; j < Seconds; ++j)
        {
            for (int i = 0; i < FramesPerSecond; ++i)
            {
                listeners.Add(handler);
                listeners.ForEach(x => x.EventHandler(true));
                listeners.Remove(handler);

                l += 1;
            }

        }

        watch.Stop();
        results.Add(watch.GetAllocationDesc($"Event add/remove for each frame at {FramesPerSecond} FPS for {Seconds} s of game"));

        // Just so we force l to be used
        Assert.Less(0, l);
    }



}
