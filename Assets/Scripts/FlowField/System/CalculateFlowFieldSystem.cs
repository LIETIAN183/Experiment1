using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

// [UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
// [UpdateAfter(typeof(CalculateCostFieldSystem))]
public class CalculateFlowFieldSystem : SystemBase
{
    public float timer = 1;
    protected override void OnUpdate()
    {
        timer -= Time.DeltaTime;
        if (timer > 0) return;
        timer = 1;

        DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();

        if (cellBuffer.Length == 0) return;

        var settingData = GetSingleton<FlowFieldSettingData>();
        var gridSize = settingData.gridSize;

        // Calculate DestinationIndex
        var destinationIndex = FlowFieldHelper.GetCellIndexFromWorldPos(settingData.originPoint, settingData.destination, gridSize, settingData.cellRadius * 2);
        // Update Destination Cell's cost and bestCost
        int flatDestinationIndex = FlowFieldHelper.ToFlatIndex(destinationIndex, gridSize.y);
        CellData destinationCell = cellBuffer[flatDestinationIndex];
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;
        cellBuffer[flatDestinationIndex] = destinationCell;

        // Integration Field, Flow Field
        NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
        NativeList<int2> neighborIndices = new NativeList<int2>(Allocator.TempJob);
        indicesToCheck.Enqueue(destinationIndex);
        while (indicesToCheck.Count > 0)
        {
            int2 cellIndex = indicesToCheck.Dequeue();
            int cellFlatIndex = FlowFieldHelper.ToFlatIndex(cellIndex, gridSize.y);
            CellData curCellData = cellBuffer[cellFlatIndex];

            FlowFieldHelper.GetNeighborIndices(cellIndex, gridSize, ref neighborIndices);
            foreach (int2 neighborIndex in neighborIndices)
            {
                int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
                CellData neighborCellData = cellBuffer[flatNeighborIndex];
                // 更新第一层障碍物的最佳方向
                if (neighborCellData.cost == byte.MaxValue)
                {
                    if (neighborCellData.bestDirection.Equals(GridDirection.None))
                    {
                        neighborCellData.bestDirection = cellIndex - neighborIndex;
                        cellBuffer[flatNeighborIndex] = neighborCellData;
                    }
                    continue;
                }
                // 更新 bestCost 和 最佳方向
                if (neighborCellData.cost + curCellData.bestCost < neighborCellData.bestCost)
                {
                    neighborCellData.bestCost = (ushort)(neighborCellData.cost + curCellData.bestCost);
                    neighborCellData.bestDirection = cellIndex - neighborIndex;
                    cellBuffer[flatNeighborIndex] = neighborCellData;
                    indicesToCheck.Enqueue(neighborIndex);
                }
            }
        }

        // Release Native Container
        neighborIndices.Dispose();
        indicesToCheck.Dispose();
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<FlowFieldDebugSystem>().UpdateData();
    }
}
