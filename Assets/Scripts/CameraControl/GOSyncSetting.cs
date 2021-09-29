using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class GOSyncSetting : MonoBehaviour
{
    public Entity dataInEntity;
    private EntityManager manager;
    public float3 vel;

    void Awake()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        Vector3 temp = vel;
        transform.position += temp * 0.01f * 5;
    }

    void FixedUpdate()
    {
        if (dataInEntity == null)
        {
            Debug.Log("GOSyncSetting.cs : Need Attach the Entity for Sync");
            return;
        }
        SyncTag target = manager.GetComponentData<SyncTag>(dataInEntity);
        vel -= target.acc * 0.01f;
    }
}
