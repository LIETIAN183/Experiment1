using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Authoring;

[RequireComponent(typeof(PhysicsShapeAuthoring), typeof(PhysicsBodyAuthoring))]
public class SubFCDataAuthoring : MonoBehaviour
{
    public float height;

    public GameObject parent;

    class Baker : Baker<SubFCDataAuthoring>
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
}

public struct SubFCData : IComponentData
{
    public float3 orgPos;

    public quaternion orgRot;

    public float height;

    public Entity parent;
}