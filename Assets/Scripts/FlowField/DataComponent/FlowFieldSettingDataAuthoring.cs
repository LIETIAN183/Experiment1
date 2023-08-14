using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// 实现 FlowFieldSettingData 通过 Inspector 手动挂载
public class FlowFieldSettingDataAuthoring : MonoBehaviour
{
    public float3 originPoint;
    public int2 gridSize;
    public float3 cellRadius;
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
                displayOffset = authoring.displayOffset
            });
        }
    }
}

// 网格集合的整体数据
public struct FlowFieldSettingData : IComponentData
{
    // 网格集合的起始点
    public float3 originPoint;
    // 网格集合的二维尺寸
    public int2 gridSetSize;
    // 单个网格的三维大小
    public float3 cellRadius;
    // 可视化阶段使用的渲染三维偏移量
    public float3 displayOffset;
}