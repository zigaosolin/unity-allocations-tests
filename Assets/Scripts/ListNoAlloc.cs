using System;

public sealed class ListNoAlloc<T>
{
    int count;
    T[] array;

    public ListNoAlloc(int maxSize)
    {
        array = new T[maxSize];
    }

    public T this[int index]
    {
        get
        {
            CheckInRange(index);
            return array[index];
        }
        set
        {
            CheckInRange(index);
            array[index] = value;
        }
    }

    public int Count => count;

    public void Add(T element)
    {
        Reserve(1);
        array[count - 1] = element;
    }

    public bool Remove(T element)
    {
        for (int i = 0; i < count; ++i)
        {
            if (element.Equals(array[i]))
            {
                RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        CheckInRange(index);

        int n = count - 1;

        for (int i = index; i < n; ++i)
        {
            array[i] = array[i + 1];
        }

        array[n] = default;
        --count;
    }


    void CheckInRange(int index)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentException("index");
        }
    }

    void Reserve(int amount)
    {
        if (this.count + amount > array.Length)
        {
            throw new OverflowException("List reached maximum size");
        }

        this.count += amount;
    }

    public void ForEach(Action<T> action)
    {
        for(int i = 0; i < count; ++i)
        {
            action(array[i]);
        }
    }

}
