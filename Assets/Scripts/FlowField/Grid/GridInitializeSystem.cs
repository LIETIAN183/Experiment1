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

#if UNITY_EDITOR
        // Sync SettingData to DisplayDebug
        GridDebug.instance.debugFlowFieldSetting = settingComponent;
#endif

        // DynamicBuffer
        DynamicBuffer<CellBufferElement> buffer = GetBuffer<CellBufferElement>(settingEntity);
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();

        int2 gridSize = settingComponent.gridSize;
        float3 originPoint = settingComponent.originPoint;
        float cellRadius = settingComponent.cellRadius;
        float cellDiameter = cellRadius * 2;
        // Create Grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                float3 cellWorldPos = new float3(originPoint.x + cellDiameter * x + cellRadius, originPoint.y, originPoint.z + cellDiameter * y + cellRadius);
                // int2 gridIndex = new int2(x, y);
                CellData newCellData = new CellData
                {
                    worldPos = cellWorldPos,
                    gridIndex = new int2(x, y),
                    // cost = 1
                    // curCellData.cost = 1; // 同下理，每次都需要计算，且要重置后计算，不如每次计算都考虑初始值 1，也无需初始化   // 验证时出现过等于 1 的情况，赋值一下以防出错。虽然无法理解为什么出现 0
                    // curCellData.bestCost = ushort.MaxValue; 每次计算 Intergration Field 前都需要重置 bestCost 障碍物的代价，因此这条代码放到计算 CostField 时，顺便重置，因此无需初始化
                    // curCellData.bestDirection = int2.zero; // 默认值就是 int2.zero ，无需初始化
                };
                cellBuffer.Add(newCellData);
            }
        }

        EntityManager.AddComponent<GridFinishTag>(settingEntity);

        this.Enabled = false;
    }
}