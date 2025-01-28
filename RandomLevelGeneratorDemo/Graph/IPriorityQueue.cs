using System.Collections.Generic;

public interface IPriorityQueue<T>
{
    public abstract int Count {get;}
    public IEnumerable<T> Elements {get;}

    public void Insert(T element, int key);
    public void Extract(out T element, out int key);
    public T Extract();
    public void Peek(out T element, out int key);
    public T Peek();
    public void Update(T element, int key);
    public void Clear();
}