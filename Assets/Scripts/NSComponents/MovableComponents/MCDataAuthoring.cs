using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;

[RequireComponent(typeof(PhysicsShapeAuthoring), typeof(PhysicsBodyAuthoring))]
public class MCDataAuthoring : MonoBehaviour
{
    class Baker : Baker<MCDataAuthoring>
    {
        public override void Bake(MCDataAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            AddComponent<MCData>(entity);
        }
    }
}

public struct MCData : IComponentData
{
    // 用于判断空中状态
    public float preVelinY;
    public bool inAir;
}