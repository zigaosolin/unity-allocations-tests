using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class DictionaryNoAlloc<TKey, TValue>
    where TKey : IEquatable<TKey>
{
    private struct KeyValue
    {
        public bool IsUsed;
        public TKey Key;
        public TValue Value;
    }

    public struct DictionaryNoAllocIterator
    {
        private int index;
        private KeyValue[] array;
        private int arrayLength;

        internal DictionaryNoAllocIterator(DictionaryNoAlloc<TKey, TValue> dictionary)
        {
            array = dictionary.array;
            arrayLength = array.Length;
            index = -1;
        }

        public bool MoveNext()
        {
            ++index;

            // Skip through all unused
            while (index < arrayLength)
            {
                ref var current = ref array[index];
                if (current.IsUsed)
                {
                    return true;
                }

                ++index;
            }
            return false;
        }

        public TKey CurrentKey => array[index].Key;

        public TValue CurrentValue => array[index].Value;

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                ref var current = ref array[index];
                return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
            }
        }
    }

    const int DictionaryMaxFillPercent = 50;

    int count;
    int maxSize;
    KeyValue[] array;

    public DictionaryNoAlloc(int maxSize)
    {
        if (maxSize < 1)
        {
            throw new ArgumentException("maxSize");
        }

        count = 0;
        this.maxSize = maxSize;
        array = new KeyValue[(maxSize * 100) / DictionaryMaxFillPercent];
    }

    public void Add(TKey key, TValue value)
    {
        int index = FindIndex(key);

        ref var keyValue = ref array[index];
        if (keyValue.IsUsed) {
            throw new ArgumentException($"Key {key} already exists");
        }

        Reserve(1);

        keyValue.IsUsed = true;
        keyValue.Key = key;
        keyValue.Value = value;
    }

    public bool Remove(TKey key)
    {
        int index = FindIndex(key);

        ref var keyValue = ref array[index];
        if (keyValue.IsUsed)
        {
            RemoveIndex(index);
            return true;
        }

        return false;
    }

    public TValue this[TKey key]
    {
        get
        {
            int index = FindIndex(key);
            ref var current = ref array[index];
            if(!current.IsUsed)
            {
                throw new KeyNotFoundException(key.ToString());
            }

            return current.Value;
        }

        set
        {
            int index = FindIndex(key);
            ref var current = ref array[index];

            // Check if overwriting or adding
            if(!current.IsUsed)
            {
                Reserve(1);
                current.Key = key;
            } 

            current.IsUsed = true;            
            current.Value = value;
        }
    }

    public void Clear()
    {
        int n = array.Length;
        for(int i = 0; i < array.Length; ++i)
        {
            array[i] = new KeyValue();
        }

        count = 0;
    }

    public int Count => count;

    public DictionaryNoAllocIterator GetIteratorNoAlloc()
    {
        return new DictionaryNoAllocIterator(this);
    }

    private int FindIndex(TKey key)
    {
        int index = GetHasInRange(key.GetHashCode());

        // Find first non-empty key matching the hash, or exact match
        while (true)
        {
            ref var keyValue = ref array[index];
            if (!keyValue.IsUsed)
            {
                return index;
            }

            if (keyValue.Key.Equals(key))
            {
                return index;
            }

            index = (index + 1) % array.Length;
        }
    }

    private void RemoveIndex(int index) {

        // Move all elements following one to the left
        while (true)
        {
            ref var current = ref array[index];
            int currentHash = GetHasInRange(current.Key.GetHashCode());

            int nextIndex = (index + 1) % array.Length;
            ref var next = ref array[nextIndex];

            if (!next.IsUsed)
            {
                break;
            }

            var nextHash = GetHasInRange(next.Key.GetHashCode());
            if (nextHash != currentHash)
            {
                break;
            }

            current.Key = next.Key;
            current.Value = next.Value;

            index = nextIndex;
        }

        // Remove current
        {
            ref var current = ref array[index];
            current.IsUsed = false;
            current.Key = default;
            current.Value = default;
        }
    }

    private int GetHasInRange(int hash)
    {
        int arrayLength = array.Length;
        hash = hash % arrayLength;

        return hash >= 0 ? hash : hash + array.Length;
    }

    private void Reserve(int size)
    {
        if(count + size > maxSize)
        {
            throw new OverflowException($"Required size {count + size}, maxSize {maxSize}");
        }
        count += size;
    }
}