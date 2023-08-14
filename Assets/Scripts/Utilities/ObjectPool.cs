using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    public IObjectPool<GameObject> pool;

    [SerializeField]
    private GameObject prefab;

#if UNITY_EDITOR
    public int remainObject;
# endif

    void Awake()
    {
        if (instance == null) instance = this;

        if (pool == null) pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false, 100, 10000);
    }

    // Called when no iteem in pool
    GameObject CreatePooledItem() => GameObject.Instantiate(prefab);

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(GameObject go) => go.SetActive(false);

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(GameObject go) => go.SetActive(true);

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(GameObject go) => Destroy(go);

#if UNITY_EDITOR
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        remainObject = pool.CountInactive;
    }
#endif
}