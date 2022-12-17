using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class CalculateFlowFieldSystem : SystemBase
{
    // 在这里修改 Enable 当前 Update运行完生效
    protected override void OnStartRunning() => this.Enabled = false;
    protected override void OnUpdate()
    {
        DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBuffer>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();

        if (cellBuffer.Length == 0) return;

        var settingData = GetSingleton<FlowFieldSettingData>();
        var gridSize = settingData.gridSize;

        for (int i = 0; i < cellBuffer.Length; i++)
        {
            CellData curCell = cellBuffer[i];
            // if (curCell.bestCost.Equals(ushort.MaxValue))
            // {
            //     continue;
            // }
            if (curCell.tempCost.Equals(float.MaxValue))
            {
                continue;
            }
            curCell.bestDir = float2.zero;
            foreach (int2 neighborIndex in FlowFieldUtility.GetNeighborIndices(curCell.gridIndex, gridSize))
            {
                int flatNeighborIndex = FlowFieldUtility.ToFlatIndex(neighborIndex, gridSize.y);
                CellData neighborCellData = cellBuffer[flatNeighborIndex];
                // if (neighborCellData.bestCost.Equals(ushort.MaxValue)) continue;
                if (neighborCellData.tempCost.Equals(float.MaxValue)) continue;
                // var temp = curCell.bestCost - neighborCellData.bestCost;
                var temp = curCell.tempCost - neighborCellData.tempCost;
                if (temp == 0) continue;
                curCell.bestDir += temp * (float2)(neighborCellData.gridIndex - curCell.gridIndex);
            }

            cellBuffer[i] = curCell;
        }

        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FlowFieldVisulizeSystem>().UpdateData();
    }
}

// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;

// public partial class CalculateFlowFieldSystem : SystemBase
// {
//     // 在这里修改 Enable 当前 Update运行完生效
//     protected override void OnStartRunning() => this.Enabled = false;
//     protected override void OnUpdate()
//     {
//         DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();

//         if (cellBuffer.Length == 0) return;

//         var settingData = GetSingleton<FlowFieldSettingData>();
//         var gridSize = settingData.gridSize;

//         // Calculate DestinationIndex
//         var destinationIndex = FlowFieldUtility.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSize, settingData.cellRadius * 2);
//         // Update Destination Cell's cost and bestCost
//         int flatDestinationIndex = FlowFieldUtility.ToFlatIndex(destinationIndex, gridSize.y);
//         CellData destinationCell = cellBuffer[flatDestinationIndex];
//         destinationCell.cost = 0;
//         destinationCell.bestCost = 0;
//         cellBuffer[flatDestinationIndex] = destinationCell;

//         // Integration Field, Flow Field
//         NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
//         indicesToCheck.Enqueue(destinationIndex);
//         while (indicesToCheck.Count > 0)
//         {
//             int2 cellIndex = indicesToCheck.Dequeue();
//             int cellFlatIndex = FlowFieldUtility.ToFlatIndex(cellIndex, gridSize.y);
//             CellData curCellData = cellBuffer[cellFlatIndex];

//             foreach (int2 neighborIndex in FlowFieldUtility.GetNeighborIndices(cellIndex, gridSize))
//             {
//                 int flatNeighborIndex = FlowFieldUtility.ToFlatIndex(neighborIndex, gridSize.y);
//                 CellData neighborCellData = cellBuffer[flatNeighborIndex];
//                 // 更新第一层障碍物的最佳方向
//                 if (neighborCellData.cost == byte.MaxValue)
//                 {
//                     // if (neighborCellData.bestDirection.Equals(GridDirection.None))
//                     if (neighborCellData.updated == false)
//                     {
//                         neighborCellData.bestDirection = cellIndex - neighborIndex;
//                         neighborCellData.updated = true;
//                         cellBuffer[flatNeighborIndex] = neighborCellData;
//                     }
//                     continue;
//                 }
//                 // 更新 bestCost 和 最佳方向
//                 ushort targetBestCost = (ushort)(neighborCellData.cost + curCellData.bestCost);
//                 if (targetBestCost < neighborCellData.bestCost)
//                 {
//                     neighborCellData.bestCost = targetBestCost;
//                     neighborCellData.bestDirection = cellIndex - neighborIndex;
//                     cellBuffer[flatNeighborIndex] = neighborCellData;
//                     indicesToCheck.Enqueue(neighborIndex);
//                 }
//             }
//         }

//         // Release Native Container
//         indicesToCheck.Dispose();
//         World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FlowFieldVisulizeSystem>().UpdateData();
//     }
// }
