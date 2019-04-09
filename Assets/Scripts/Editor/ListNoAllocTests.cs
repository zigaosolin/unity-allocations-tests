using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ListNoAllocTests 
{
    
    [Test]
    public void Add()
    {
        var list = new ListNoAlloc<int>(3);
        list.Add(2);
        list.Add(1);
    }

    [Test]
    public void Index()
    {
        var list = new ListNoAlloc<int>(3);
        list.Add(2);
        list.Add(1);

        Assert.AreEqual(2, list[0]);
        Assert.AreEqual(1, list[1]);
    }

    public void Count()
    {
        var list = new ListNoAlloc<int>(3);
        Assert.AreEqual(0, list.Count);
        list.Add(2);
        Assert.AreEqual(2, list.Count);
        list.Add(1);
        Assert.AreEqual(1, list.Count);
    }

    [Test]
    public void Remove()
    {
        var list = new ListNoAlloc<int>(3);
        Assert.AreEqual(0, list.Count);
        list.Add(2);
        list.Remove(2);
        Assert.AreEqual(0, list.Count);
    }

    [Test]
    public void RemoveFail()
    {
        var list = new ListNoAlloc<int>(3);
        Assert.AreEqual(0, list.Count);
        list.Add(2);
        Assert.False(list.Remove(3));
        Assert.AreEqual(1, list.Count);
    }

    [Test]
    public void RemoveAt()
    {
        var list = new ListNoAlloc<int>(3);

        list.Add(2);
        list.Add(5);
        list.Add(7);
        list.RemoveAt(1);
        Assert.AreEqual(2, list.Count);

        Assert.AreEqual(2, list[0]);
        Assert.AreEqual(7, list[1]);
    }

    [Test]
    public void RemoveEmpty()
    {
        var list = new ListNoAlloc<int>(3);

        Assert.Throws<ArgumentException>(() => list.RemoveAt(0));
    }

    [Test]
    public void AddTooMany()
    {
        var list = new ListNoAlloc<int>(10);

        for(int i = 0; i < 10; ++i)
        {
            list.Add(i * i);
        }

        Assert.Throws<OverflowException>(() => list.Add(10));
    }

    [Test]
    public void Clear()
    {
        var list = new ListNoAlloc<int>(10);

        for (int i = 0; i < 10; ++i)
        {
            list.Add(i * i);
        }

        list.Clear();

        Assert.AreEqual(0, list.Count);
        Assert.Throws<ArgumentException>(() => { int x = list[1]; });
    }



}
