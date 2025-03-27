using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool implementation for reusing game objects
/// </summary>
/// <typeparam name="T">Type of objects to pool</typeparam>
public class ObjectPool<T> where T : UnityEngine.Object
{
    private readonly Func<T> createFunc;
    private readonly Action<T> getAction;
    private readonly Action<T> releaseAction;
    private readonly int maxPoolSize;
    private readonly Stack<T> pool;
    
    /// <summary>
    /// Create a new object pool
    /// </summary>
    /// <param name="createFunc">Function to create new instances when pool is empty</param>
    /// <param name="initialPoolSize">Initial capacity of the pool</param>
    /// <param name="maxPoolSize">Maximum number of objects to keep in the pool</param>
    /// <param name="getAction">Optional action to perform when object is retrieved from pool</param>
    /// <param name="releaseAction">Optional action to perform when object is returned to pool</param>
    public ObjectPool(Func<T> createFunc, int initialPoolSize = 0, int maxPoolSize = 100, 
                      Action<T> getAction = null, Action<T> releaseAction = null)
    {
        this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        this.getAction = getAction;
        this.releaseAction = releaseAction;
        this.maxPoolSize = maxPoolSize;
        
        // Initialize the pool
        pool = new Stack<T>(initialPoolSize);
        
        // Pre-populate the pool if initial size is specified
        for (int i = 0; i < initialPoolSize; i++)
        {
            var obj = createFunc();
            pool.Push(obj);
        }
    }
    
    /// <summary>
    /// Get an object from the pool or create a new one if pool is empty
    /// </summary>
    /// <returns>The pooled object</returns>
    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Pop() : createFunc();
        
        // Perform any setup needed when getting from pool
        getAction?.Invoke(obj);
        
        return obj;
    }
    
    /// <summary>
    /// Return an object to the pool
    /// </summary>
    /// <param name="obj">The object to return</param>
    public void Release(T obj)
    {
        if (obj == null)
            return;
            
        // If the pool is full, just let the object be garbage collected
        if (pool.Count >= maxPoolSize)
            return;
            
        // Perform any cleanup needed before returning to pool
        releaseAction?.Invoke(obj);
        
        pool.Push(obj);
    }
    
    /// <summary>
    /// Clear the pool and release all resources
    /// </summary>
    public void Clear()
    {
        pool.Clear();
    }
}

/// <summary>
/// Manager for multiple object pools
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    [Serializable]
    public class PoolInfo
    {
        public string tag;
        public GameObject prefab;
        public int initialPoolSize = 10;
        public int maxPoolSize = 100;
    }
    
    [SerializeField] private List<PoolInfo> pools;
    
    private Dictionary<string, ObjectPool<GameObject>> poolDictionary;
    
    private void Awake()
    {
        poolDictionary = new Dictionary<string, ObjectPool<GameObject>>();
        
        // Set up each pool
        foreach (var poolInfo in pools)
        {
            // Create a local reference for the createFunc
            GameObject prefab = poolInfo.prefab;
            
            // Create the pool
            var pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab),
                initialPoolSize: poolInfo.initialPoolSize,
                maxPoolSize: poolInfo.maxPoolSize,
                getAction: (obj) => obj.SetActive(true),
                releaseAction: (obj) => obj.SetActive(false)
            );
            
            poolDictionary[poolInfo.tag] = pool;
        }
    }
    
    /// <summary>
    /// Get a game object from the specified pool
    /// </summary>
    /// <param name="tag">Pool identifier</param>
    /// <param name="parent">Optional parent transform</param>
    /// <returns>Pooled game object</returns>
    public GameObject GetFromPool(string tag, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Object pool with tag {tag} doesn't exist!");
            return null;
        }
        
        GameObject obj = poolDictionary[tag].Get();
        
        // Set parent if provided
        if (parent != null)
            obj.transform.SetParent(parent, false);
            
        return obj;
    }
    
    /// <summary>
    /// Return a game object to its pool
    /// </summary>
    /// <param name="tag">Pool identifier</param>
    /// <param name="obj">Object to return</param>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Object pool with tag {tag} doesn't exist!");
            return;
        }
        
        poolDictionary[tag].Release(obj);
    }
} 