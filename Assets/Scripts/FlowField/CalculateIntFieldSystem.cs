using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

// [DisableAutoCreation]
[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
[UpdateAfter(typeof(CalculateCostFieldSystem))]
public class CalculateIntFieldSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {

        DynamicBuffer<CellBufferElement> buffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>());
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();

        if (cellBuffer.Length == 0)
        {
            return;
        }

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

        // Integration Field
        NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
        NativeList<int2> neighborIndices = new NativeList<int2>(Allocator.TempJob);
        indicesToCheck.Enqueue(destinationIndex);
        while (indicesToCheck.Count > 0)
        {
            int2 cellIndex = indicesToCheck.Dequeue();
            int cellFlatIndex = FlowFieldHelper.ToFlatIndex(cellIndex, gridSize.y);
            CellData curCellData = cellBuffer[cellFlatIndex];
            neighborIndices.Clear();
            FlowFieldHelper.GetNeighborIndices(cellIndex, gridSize, ref neighborIndices);
            foreach (int2 neighborIndex in neighborIndices)
            {
                int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
                CellData neighborCellData = cellBuffer[flatNeighborIndex];
                if (neighborCellData.cost == byte.MaxValue)
                {
                    continue;
                }

                if (neighborCellData.cost + curCellData.bestCost < neighborCellData.bestCost)
                {
                    neighborCellData.bestCost = (ushort)(neighborCellData.cost + curCellData.bestCost);
                    cellBuffer[flatNeighborIndex] = neighborCellData;
                    indicesToCheck.Enqueue(neighborIndex);
                }
            }
        }

        // // Flow Field
        // // TODO: Combine with Integration Field
        for (int i = 0; i < cellBuffer.Length; i++)
        {
            CellData curCellData = cellBuffer[i];
            neighborIndices.Clear();
            FlowFieldHelper.GetNeighborIndices(curCellData.gridIndex, gridSize, ref neighborIndices);
            ushort bestCost = curCellData.bestCost;
            int2 bestDirection = int2.zero;
            foreach (int2 neighborIndex in neighborIndices)
            {
                int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
                CellData neighborCellData = cellBuffer[flatNeighborIndex];
                if (neighborCellData.bestCost < bestCost)
                {
                    bestCost = neighborCellData.bestCost;
                    bestDirection = neighborCellData.gridIndex - curCellData.gridIndex;
                }
            }
            curCellData.bestDirection = bestDirection;
            cellBuffer[i] = curCellData;
        }

        // Release Native Container
        neighborIndices.Dispose();
        indicesToCheck.Dispose();

        this.Enabled = false;
    }
}
