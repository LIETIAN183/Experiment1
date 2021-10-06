using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SetupGridSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var settingEntity = GetSingletonEntity<FlowFieldSettingData>();
        var settingComponent = GetSingleton<FlowFieldSettingData>();

        // EntityCommandBuffer
        EntityCommandBuffer commandBuffer = World.GetOrCreateSystem<EntityCommandBufferSystem>().CreateCommandBuffer();

        // DynamicBuffer
        DynamicBuffer<EntityBufferElement> buffer = GetBuffer<EntityBufferElement>(settingEntity);
        DynamicBuffer<Entity> entityBuffer = buffer.Reinterpret<Entity>();

        // Add All CellData into Dynamic Buffer, GridCreateTag denote weather the Celldata is added into the Dynamic Buffer
        Entities.ForEach((Entity entity, in CellData cellData, in GridCreatedTag flag) =>
        {
            entityBuffer.Add(entity);
            commandBuffer.RemoveComponent<GridCreatedTag>(entity);
        }).Run();

        var gridSize = settingComponent.gridSize;
        if (entityBuffer.Length == gridSize.x * gridSize.y)
        {
            float cellRadius = settingComponent.cellRadius;
            float cellDiameter = cellRadius * 2;

            // Setup Grid
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    float3 cellWorldPos = new float3(cellDiameter * x + cellRadius, 0, cellDiameter * y + cellRadius);
                    int2 gridIndex = new int2(x, y);
                    int flatIndex = FlowFieldHelper.ToFlatIndex(gridIndex, gridSize.y);

                    CellData curCellData = GetComponentDataFromEntity<CellData>(true)[entityBuffer[flatIndex]];

                    curCellData.worldPos = cellWorldPos;
                    curCellData.gridIndex = gridIndex;
                    curCellData.cost = 1; // 同下理，每次都需要计算，且要重置后计算，不如每次计算都考虑初始值 1，也无需初始化   // 验证时出现过等于 1 的情况，赋值一下以防出错。虽然无法理解为什么出现 0
                    // curCellData.bestCost = ushort.MaxValue; 每次计算 Intergration Field 前都需要重置 bestCost 障碍物的代价，因此这条代码放到计算 CostField 时，顺便重置，因此无需初始化
                    // curCellData.bestDirection = int2.zero; // 默认值就是 int2.zero ，无需初始化

                    commandBuffer.SetComponent(entityBuffer[flatIndex], curCellData);
                }
            }
            // 保证在计算障碍物代价前，完成 Grid 的初始化
            commandBuffer.AddComponent<CalculateFlowFieldTag>(settingEntity);

            this.Enabled = false;
        }
    }
}