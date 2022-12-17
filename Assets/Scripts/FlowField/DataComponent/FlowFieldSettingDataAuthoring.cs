using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct FlowFieldSettingData : IComponentData
{
    public float3 originPoint;
    public int2 gridSize;
    public float3 cellRadius;
    public float3 destination;

    public float3 displayHeightOffset;
}

public class FlowFieldSettingDataAuthoring : MonoBehaviour { }

public class FlowFieldSettingDataAuthoringBaker : Baker<FlowFieldSettingDataAuthoring>
{
    public override void Bake(FlowFieldSettingDataAuthoring authoring)
    {
        AddComponent<FlowFieldSettingData>();
    }
}