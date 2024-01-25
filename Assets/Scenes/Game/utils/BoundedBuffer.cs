using Unity.VisualScripting;
using UnityEngine.Assertions;

public class BoundedBuffer<T>
{
    private T[] data;
    private uint n;
    private uint nextSlot;
    private int counter;
    
    public BoundedBuffer(uint n)
    {
        data = new T[n];
        this.n = n;
        nextSlot = 0;
        counter = 0;
    }

    public void Add(T elem)
    {
        counter++;
        data[nextSlot] = elem;
        nextSlot++;
        if (nextSlot == n)
        {
            nextSlot = 0;
        }
    }

    public T Get(uint i)
    {
        Assert.IsTrue(Has(i));
        return data[i % n];
    }

    public bool Has(uint i)
    {
        return counter - n <= i && i < counter;
    }
}
