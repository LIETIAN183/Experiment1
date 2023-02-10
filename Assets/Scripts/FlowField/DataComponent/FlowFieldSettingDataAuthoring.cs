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
            AddComponent(new FlowFieldSettingData
            {
                originPoint = authoring.originPoint,
                gridSetSize = authoring.gridSize,
                cellRadius = authoring.cellRadius,
                destination = authoring.destination,
                displayOffset = authoring.displayOffset,
                index = 2
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

    public float3 rotation;

    public int index;

    public float debugValue;
}

