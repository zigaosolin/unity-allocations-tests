using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

public class QuizTests 
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

        public void AssertSize(string contextMessage)
        {
            string message = GetMessage(Allocations);
            Debug.Log(message);

            Assert.AreEqual(0, Allocations, $"Allocated {Allocations / (1024 * 1024.0):F2} MB: {contextMessage}");

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

    const int Iterations = 120 * 60 * 100;


    public class A
    {
        public int Value;
    }

    [Test]
    public void Q1()
    {
        MemoryWatch watch = new MemoryWatch();
        watch.Start();

        for(int i = 0; i < Iterations; ++i)
        {
            A a = new A();
        }

        watch.Stop();
        watch.AssertSize("class");
    }





    public struct B
    {
        public int Value;
    }

    [Test]
    public void Q2()
    {
        MemoryWatch watch = new MemoryWatch();
        watch.Start();

        for (int i = 0; i < Iterations; ++i)
        {
            B b = new B();
        }

        watch.Stop();
        watch.AssertSize("struct");
    }




    static void F(int x)
    {
    }

    [Test]
    public void Q3()
    {
        MemoryWatch watch = new MemoryWatch();

        var list = new List<int>() { 1, 4, 5, 2, 12 };

        watch.Start();


        for (int i = 0; i < Iterations; ++i)
        {
            list.ForEach(x => F(x));
        }

        watch.Stop();
        watch.AssertSize("lambda");
    }








    void f(int x)
    {
    }

    [Test]
    public void Q4()
    {
        MemoryWatch watch = new MemoryWatch();

        var list = new List<int>() { 1, 4, 5, 2, 12 };

        watch.Start();


        for (int i = 0; i < Iterations; ++i)
        {
            list.ForEach(x => f(x));
        }

        watch.Stop();
        watch.AssertSize("lambda 2");
    }










    [Test]
    public void Q5()
    {
        MemoryWatch watch = new MemoryWatch();

        int sum = 0;
        var list = new List<int>() { 1, 4, 5, 2, 12 };

        watch.Start();


        for (int i = 0; i < Iterations; ++i)
        {
            list.ForEach(x => { sum += x; });
        }

        watch.Stop();
        watch.AssertSize("closure");
        Assert.AreNotEqual(0, sum);
    }













    [Test]
    public void Q6()
    {
        MemoryWatch watch = new MemoryWatch();

        int sum = 0;
        var list = new List<int>() { 1, 4, 5, 2, 12 };

        watch.Start();


        for (int i = 0; i < Iterations; ++i)
        {
            list.ForEach(x => { sum += (x + i); });
        }

        watch.Stop();
        watch.AssertSize("closure 2");
        Assert.AreNotEqual(0, sum);
    }











    [Test]
    public void Q7()
    {
        MemoryWatch watch = new MemoryWatch();

        int sum = 0;
        watch.Start();


        for (int i = 0; i < Iterations; ++i)
        {
            string s = $"Iteration {i}";
            sum += s.Length;             
        }

        watch.Stop();
        watch.AssertSize("string");
        Assert.AreNotEqual(0, sum);
    }













    [Test]
    public void Q8()
    {
        MemoryWatch watch = new MemoryWatch();

        int sum = 0;
        watch.Start();

        for (int i = 0; i < Iterations; ++i)
        {
            var builder = new StringBuilder();
            builder.Append("Iterations ");
            builder.Append(i);
            sum += builder.Length;
        }

        watch.Stop();
        watch.AssertSize("builder");
        Assert.AreNotEqual(0, sum);
    }












    [Test]
    public void Q9()
    {
        MemoryWatch watch = new MemoryWatch();

        int sum = 0;
        var list = new List<int>() { 1, 4, 5, 2, 12 };

        watch.Start();


        for (int i = 0; i < Iterations; ++i)
        {
            list.ForEach(x => Sum(x));
        }

        watch.Stop();
        watch.AssertSize("lambda + local func");
        Assert.AreNotEqual(0, sum);

        void Sum(int x)
        {
            sum += x + list[x % list.Count];
        }
    }










    [UnityTest]
    public IEnumerator Q10()
    {
        MemoryWatch watch = new MemoryWatch();

        int sum = 0;

        watch.Start();

        for (int i = 0; i < 100; ++i)
        {
            Task t = Sum(i);
            yield return null;
        }

        watch.Stop();
        watch.AssertSize("local func");
        Assert.AreNotEqual(0, sum);

        async Task Sum(int x)
        {
            await Task.Yield();

            // Will always be continued on Unity thread due to scheduler
            sum += x;
        }
    }
}
