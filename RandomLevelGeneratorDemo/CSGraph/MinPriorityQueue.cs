using System.Collections.Generic;
using System.Collections;
using System;

public class MinPriorityQueue<T> : IPriorityQueue<T>
{
    public int Count => heapSize;
    public IEnumerable<T> Elements => new Enumerable(this);

    private List<Node> nodes = new();
    private int heapSize = 0;

    public void Insert(T element, int key)
    {
        ++heapSize;

        if (nodes.Count < heapSize)
            nodes.Add(new Node(element, int.MaxValue));
        else
            nodes[heapSize - 1] = new Node(element, int.MaxValue);

        DecreaseKey(heapSize - 1, key);
    }

    public void Extract(out T element, out int key)
    {
        if (heapSize == 0)
            throw new Exception("Queue is empty");
        
        element = nodes[0].Element;
        key = nodes[0].Key;

        --heapSize;
        nodes[0] = nodes[heapSize];
        nodes.RemoveAt(heapSize);
        MinHeapify(0);
    }

    public T Extract()
    {
        Extract(out T element, out int key);
        return element;
    }

    public void Peek(out T element, out int key)
    {
        if (heapSize == 0)
            throw new Exception("Queue is empty");
        
        element = nodes[0].Element;
        key = nodes[0].Key;
    }

    public T Peek()
    {
        Peek(out T element, out int key);
        return element;
    }

    public void Update(T element, int key)
    {
        int toUpdate = nodes.FindIndex(n => n.Element.Equals(element));

        DecreaseKey(toUpdate, key);
    }

    public bool TryUpdate(T element, int key)
    {
        int toUpdate = nodes.FindIndex(n => n.Element.Equals(element));

        if (toUpdate != -1)
        {
            DecreaseKey(toUpdate, key);
            return true;
        }

        return false;
    }

    public void Clear()
    {
        nodes.Clear();
        heapSize = 0;
    }

    private int Parent(int idx)
    {
        return (idx - 1) / 2;
    }

    private int Left(int idx)
    {
        return idx * 2 + 1;
    }

    private int Right(int idx)
    {
        return idx * 2 + 2;
    }

    private void DecreaseKey(int idx, int key)
    {
        if (nodes[idx].Key < key)
            throw new Exception("New key must be less than current key");

        nodes[idx].Key = key;

        int i = idx;
        while(i > 0 && nodes[Parent(i)].Key > nodes[i].Key)
        {
            Node temp = nodes[Parent(i)];
            nodes[Parent(i)] = nodes[i];
            nodes[i] = temp;

            i = Parent(i);
        }
    }

    private void MinHeapify(int idx)
    {
        int least = idx;

        int left = Left(idx);

        if (left < heapSize && nodes[left].Key < nodes[least].Key)
            least = left;
        
        int right = Right(idx);

        if (right < heapSize && nodes[right].Key < nodes[least].Key)
            least = right;
        
        if (least != idx)
        {
            Node temp = nodes[idx];
            nodes[idx] = nodes[least];
            nodes[least] = temp;

            MinHeapify(least);
        }
    }

    public class Enumerable : IEnumerable<T>
    {
        private MinPriorityQueue<T> queue;

        public Enumerable(MinPriorityQueue<T> queue)
        {
            this.queue = queue;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(queue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }
    }

    public class Enumerator : IEnumerator<T>
    {
        private int idx = -1;
        private MinPriorityQueue<T> queue;

        public Enumerator(MinPriorityQueue<T> queue)
        {
            this.queue = queue;
        }

        public T Current
        {
            get
            {
                return queue.nodes[idx].Element;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            return ++idx < queue.heapSize;
        }

        public void Reset()
        {
            idx = -1;
        }

        public void Dispose() {}
    }

    private class Node
    {
        public T Element;
        public int Key;

        public Node(T element, int key)
        {
            Element = element;
            Key = key;
        }
    }
}