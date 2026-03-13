using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> availableObjects;
    private readonly HashSet<T> activeObjects;
    private readonly int initialSize;
    private readonly bool expandable;

    public ObjectPool(T prefab, int initialSize = 10, bool expandable = true, Transform parent = null)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        this.expandable = expandable;
        this.parent = parent;
        
        availableObjects = new Queue<T>(initialSize);
        activeObjects = new HashSet<T>();

        PreWarm();
    }

    private void PreWarm()
    {
        for (int i = 0; i < initialSize; i++)
        {
            availableObjects.Enqueue(CreateNewObject());
        }
    }

    private T CreateNewObject()
    {
        T newObj = Object.Instantiate(prefab, parent);
        newObj.gameObject.SetActive(false);
        return newObj; // Caller is responsible for enqueuing or using directly
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj;

        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        else if (expandable)
        {
            obj = CreateNewObject(); // Expand: create without adding to queue
        }
        else
        {
            return null;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);

        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null || !activeObjects.Contains(obj))
            return;

        obj.gameObject.SetActive(false);
        activeObjects.Remove(obj);
        availableObjects.Enqueue(obj);
    }

    public void ReturnAll()
    {
        foreach (T obj in activeObjects)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(false);
                availableObjects.Enqueue(obj);
            }
        }
        activeObjects.Clear();
    }

    public int CountActive => activeObjects.Count;
    public int CountAvailable => availableObjects.Count;
    public int CountTotal => activeObjects.Count + availableObjects.Count;
}
