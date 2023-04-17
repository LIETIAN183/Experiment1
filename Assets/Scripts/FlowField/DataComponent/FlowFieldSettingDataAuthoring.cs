using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FlowFieldSettingDataAuthoring : MonoBehaviour
{
    public float3 originPoint;
    public int2 gridSize;
    public float3 cellRadius;
    public float3 destination;
    public float3 displayOffset;
    class Baker : Baker<FlowFieldSettingDataAuthoring>
    {
        public override void Bake(FlowFieldSettingDataAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new FlowFieldSettingData
            {
                originPoint = authoring.originPoint,
                gridSetSize = authoring.gridSize,
                cellRadius = authoring.cellRadius,
                destination = authoring.destination,
                displayOffset = authoring.displayOffset
            });
        }
    }
}

public struct FlowFieldSettingData : IComponentData
{
    public float3 originPoint;
    public int2 gridSetSize;
    public float3 cellRadius;
    public float3 destination;
    public float3 displayOffset;

    public int index;
}