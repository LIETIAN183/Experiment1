using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SubFCData : IComponentData
{
    public float3 originLocalPosition;

    public quaternion originalRotation;

    public float height;

    public Entity parent;
}

public class SubFCDataAuthoring : MonoBehaviour
{
    public float height;

    public GameObject parent;
}

public class SubFCDataAuthoringBaker : Baker<SubFCDataAuthoring>
{
    public override void Bake(SubFCDataAuthoring authoring)
    {
        AddComponent(new SubFCData
        {
            height = authoring.height,
            parent = GetEntity(authoring.parent)
        });
    }
}