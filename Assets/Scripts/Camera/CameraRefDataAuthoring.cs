
using System;
using UnityEngine;
using Unity.Entities;

public class CameraRefDataAuthoring : MonoBehaviour
{
    public Camera mainCamera;
    public Camera overHeadCamera;

    private EntityManager entityManager;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = entityManager.CreateEntity();
        entityManager.SetName(entity, "ManagedEntity");
        entityManager.AddComponentData<CameraRefData>(entity, new CameraRefData { mainCamera = this.mainCamera, overHeadCamera = this.overHeadCamera });
    }
}

public class CameraRefData : IComponentData
{
    public Camera mainCamera;

    public Camera overHeadCamera;
}