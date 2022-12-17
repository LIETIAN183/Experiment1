using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class CalculateIntegrationFieldSystem : SystemBase
{
    // 在这里修改 Enable 当前 Update运行完生效
    protected override void OnStartRunning() => this.Enabled = false;
    protected override void OnUpdate()
    {
        DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBuffer>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();

        if (cellBuffer.Length == 0) return;

        var settingData = GetSingleton<FlowFieldSettingData>();
        var gridSize = settingData.gridSize;

        // Calculate DestinationIndex
        var destinationIndex = FlowFieldUtility.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSize, settingData.cellRadius * 2);
        // Update Destination Cell's cost and bestCost
        int flatDestinationIndex = FlowFieldUtility.ToFlatIndex(destinationIndex, gridSize.y);
        CellData destinationCell = cellBuffer[flatDestinationIndex];
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;
        destinationCell.tempCost = 0;
        cellBuffer[flatDestinationIndex] = destinationCell;

        // Integration Field, Flow Field
        NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
        indicesToCheck.Enqueue(destinationIndex);
        while (indicesToCheck.Count > 0)
        {
            int2 cellIndex = indicesToCheck.Dequeue();
            int cellFlatIndex = FlowFieldUtility.ToFlatIndex(cellIndex, gridSize.y);
            CellData curCellData = cellBuffer[cellFlatIndex];

            foreach (int2 neighborIndex in FlowFieldUtility.GetNeighborIndices(cellIndex, gridSize))
            {
                int flatNeighborIndex = FlowFieldUtility.ToFlatIndex(neighborIndex, gridSize.y);
                CellData neighborCellData = cellBuffer[flatNeighborIndex];
                // 更新第一层障碍物的最佳方向
                if (neighborCellData.cost == byte.MaxValue)
                {
                    // if (neighborCellData.bestDirection.Equals(GridDirection.None))
                    // if (neighborCellData.updated == false)
                    // {
                    //     neighborCellData.bestDirection = cellIndex - neighborIndex;
                    //     neighborCellData.updated = true;
                    //     cellBuffer[flatNeighborIndex] = neighborCellData;
                    // }
                    continue;
                }
                // 更新 bestCost 和 最佳方向
                // ushort targetBestCost = (ushort)(neighborCellData.cost + curCellData.bestCost);
                // if (targetBestCost < neighborCellData.bestCost)
                // {
                //     neighborCellData.bestCost = targetBestCost;
                //     // neighborCellData.bestDirection = cellIndex - neighborIndex;
                //     cellBuffer[flatNeighborIndex] = neighborCellData;
                //     indicesToCheck.Enqueue(neighborIndex);
                // }

                var dir = curCellData.gridIndex - neighborCellData.gridIndex;
                float targetBestCost;
                if (dir.x == 0 || dir.y == 0)
                {
                    targetBestCost = (neighborCellData.cost + curCellData.tempCost) + 1;
                }
                else
                {
                    targetBestCost = (neighborCellData.cost + curCellData.tempCost) + math.sqrt(2);
                    // targetBestCost = (ushort)(neighborCellData.cost + curCellData.tempCost) + math.sqrt(2);
                }
                if (targetBestCost < neighborCellData.tempCost)
                {
                    neighborCellData.tempCost = targetBestCost;
                    // neighborCellData.bestDirection = cellIndex - neighborIndex;
                    cellBuffer[flatNeighborIndex] = neighborCellData;
                    indicesToCheck.Enqueue(neighborIndex);
                }
            }
        }

        // Release Native Container
        indicesToCheck.Dispose();

        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CalculateFlowFieldSystem>().Enabled = true;
    }
}

