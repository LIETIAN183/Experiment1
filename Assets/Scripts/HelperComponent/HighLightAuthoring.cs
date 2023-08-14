using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;

public class HighLightAuthoring : MonoBehaviour
{
    class Baker : Baker<HighLightAuthoring>
    {
        public override void Bake(HighLightAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            AddComponent<HighLightTag>(entity);
        }
    }
}

public struct HighLightTag : IComponentData { }