using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class GridInitiializeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var settingEntity = GetSingletonEntity<FlowFieldSettingData>();
        var settingComponent = GetSingleton<FlowFieldSettingData>();

        // DynamicBuffer
        DynamicBuffer<CellBufferElement> buffer = GetBuffer<CellBufferElement>(settingEntity);
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();

        int2 gridSize = settingComponent.gridSize;
        float3 originPoint = settingComponent.originPoint;
        float3 cellRadius = settingComponent.cellRadius;
        // Create Grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                float3 cellWorldPos = new float3(originPoint.x + (2 * x + 1) * cellRadius.x, originPoint.y, originPoint.z + (2 * y + 1) * cellRadius.z);
                CellData newCellData = new CellData
                {
                    worldPos = cellWorldPos,
                    gridIndex = new int2(x, y)
                };
                cellBuffer.Add(newCellData);
            }
        }

        this.Enabled = false;
    }
}