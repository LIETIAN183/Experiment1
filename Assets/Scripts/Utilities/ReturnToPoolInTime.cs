using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ReturnToPoolInTime : MonoBehaviour
{
    private float timer = 0;
    public float remainTime = 0.5f;
    public bool flag = false;

    void Awake()
    {
        timer = 0;
        flag = false;
    }

    void ReturnToPool()
    {
        timer = 0;
        flag = false;
        ObjectPool.instance.pool.Release(this.gameObject);
    }

    void FixedUpdate()
    {
        if (flag)
        {
            timer += Time.fixedDeltaTime;
            if (timer > remainTime)
            {
                ReturnToPool();
            }
        }
    }
}
