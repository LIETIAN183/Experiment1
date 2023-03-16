using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public static class DjistraExtension
{
}

public struct CalCulateIntegration_DjistraJob : IJob
{
    public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;
    public void Execute()
    {
        var gridSize = settingData.gridSetSize;

        // Calculate DestinationIndex
        var destinationIndex = FlowFieldUtility.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSize, settingData.cellRadius * 2);
        // Update Destination Cell's cost and bestCost
        int flatDestinationIndex = FlowFieldUtility.ToFlatIndex(destinationIndex, gridSize.y);
        CellData destinationCell = cells[flatDestinationIndex];
        destinationCell.localCost = 0;
        destinationCell.integrationCost = 0;
        cells[flatDestinationIndex] = destinationCell;

        // Integration Field, Flow Field
        NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
        indicesToCheck.Enqueue(destinationIndex);
        while (indicesToCheck.Count > 0)
        {
            int2 cellIndex = indicesToCheck.Dequeue();
            int cellFlatIndex = FlowFieldUtility.ToFlatIndex(cellIndex, gridSize.y);
            CellData curCellData = cells[cellFlatIndex];

            var neighborIndexList = FlowFieldUtility.Get8NeighborIndices(cellIndex, gridSize);
            foreach (int2 neighborIndex in neighborIndexList)
            {
                int flatNeighborIndex = FlowFieldUtility.ToFlatIndex(neighborIndex, gridSize.y);
                CellData neighborCellData = cells[flatNeighborIndex];
                // 更新第一层障碍物的最佳方向
                if (neighborCellData.localCost >= Constants.T_c)
                {
                    continue;
                }
                // 更新 bestCost
                var dir = curCellData.gridIndex - neighborCellData.gridIndex;
                float targetBestCost;
                if (dir.x == 0 || dir.y == 0)
                {
                    targetBestCost = (neighborCellData.localCost + curCellData.integrationCost) + 0.5f;
                }
                else
                {
                    targetBestCost = (neighborCellData.localCost + curCellData.integrationCost) + math.sqrt(2) * 0.5f;
                    // targetBestCost = (ushort)(neighborCellData.cost + curCellData.tempCost) + math.sqrt(2);
                }
                if (targetBestCost < neighborCellData.integrationCost)
                {
                    neighborCellData.integrationCost = targetBestCost;
                    // neighborCellData.bestDirection = cellIndex - neighborIndex;
                    cells[flatNeighborIndex] = neighborCellData;
                    indicesToCheck.Enqueue(neighborIndex);
                }
            }
            neighborIndexList.Dispose();
        }
        indicesToCheck.Dispose();
    }
}