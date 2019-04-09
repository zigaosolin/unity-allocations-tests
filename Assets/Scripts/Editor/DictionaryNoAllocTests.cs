﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryNoAllocTests 
{
    [Test]
    public void Add()
    {
        var dictionary = new DictionaryNoAlloc<string, int>(10);
        dictionary.Add("MyValue", 10);
    }

    [Test]
    public void Get()
    {
        var dictionary = new DictionaryNoAlloc<string, int>(10);
        dictionary.Add("MyValue", 10);

        Assert.AreEqual(10, dictionary["MyValue"]);
    }

    [Test]
    public void Remove()
    {
        var dictionary = new DictionaryNoAlloc<string, int>(10);
        dictionary.Add("MyValue", 10);
        Assert.False(dictionary.Remove("NotUsedValue"));
        Assert.True(dictionary.Remove("MyValue"));
    }

    [Test]
    public void AddOverRange()
    {
        var dictionary = new DictionaryNoAlloc<string, int>(2);
        dictionary.Add("A", 1);
        dictionary.Add("B", 1);
        Assert.Throws<OverflowException>(() => dictionary.Add("C", 1));
    }

    [Test]
    public void AddThis()
    {
        var dictionary = new DictionaryNoAlloc<string, int>(2);
        dictionary["A"] = 5;
        Assert.AreEqual(5, dictionary["A"]);
    }

    [Test]
    public void AddMany()
    {
        var dictionary = new DictionaryNoAlloc<int, int>(100);
        for(int i = 0; i < 100; ++i)
        {
            dictionary.Add(i * i, i);
        }

        for(int i = 0; i < 100; ++i)
        {
            Assert.AreEqual(i, dictionary[i * i]);
        }
    }

    [Test]
    public void Clear()
    {
        var dictionary = new DictionaryNoAlloc<int, int>(100);
        for (int i = 0; i < 100; ++i)
        {
            dictionary.Add(i * i, i);
        }

        dictionary.Clear();
        Assert.AreEqual(0, dictionary.Count);
        Assert.Throws<KeyNotFoundException>(() => { int x = dictionary[4]; });
    }

    struct HashableKey : IEquatable<HashableKey>
    {
        string keyValue;

        public HashableKey(string keyValue)
        {
            this.keyValue = keyValue;
        }

        public bool Equals(HashableKey other)
        {
            return keyValue.Equals(other.keyValue);
        }

        public override int GetHashCode()
        {
            // Worst possible hash, always the same
            return 1;
        }
    }

    [Test]
    public void HashClashSevere()
    {
        var dictionary = new DictionaryNoAlloc<HashableKey, int>(100);

        for(int i = 0; i< 100; ++i)
        {
            dictionary.Add(new HashableKey(i.ToString()), i);
        }

        for(int i = 0; i < 100; ++i)
        {
            var key = new HashableKey(i.ToString());
            Assert.AreEqual(i, dictionary[key]);
        }
    }
}
