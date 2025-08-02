using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        float bestPriority = elements[0].priority;

        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < bestPriority)
            {
                bestPriority = elements[i].priority;
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public float PeekPriority()
    {
        float bestPriority = elements[0].priority;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < bestPriority)
                bestPriority = elements[i].priority;
        }
        return bestPriority;
    }

    public bool Contains(T item)
    {
        foreach (var element in elements)
        {
            if (EqualityComparer<T>.Default.Equals(element.item, item))
                return true;
        }
        return false;
    }

    public void Remove(T item)
    {
        elements.RemoveAll(e => EqualityComparer<T>.Default.Equals(e.item, item));
    }
}