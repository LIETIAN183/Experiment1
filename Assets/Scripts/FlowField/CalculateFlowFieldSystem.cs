using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

// [DisableAutoCreation]
[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
public class CalculateFlowFieldSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    public static readonly float3 one = new float3(1, 1, 1);

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        Entities.WithAll<GridFinishTag>().ForEach((Entity entity, ref DynamicBuffer<CellBufferElement> buffer, ref FlowFieldSettingData flowFieldSettingData) =>
        {
            // 通过鼠标设置目标点
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
                Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
                flowFieldSettingData.destination = worldMousePos;
            }

            // Cost Field
            DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);
            int2 gridSize = flowFieldSettingData.gridSize;
            for (int i = 0; i < cellBuffer.Length; i++)
            {
                CellData curCellData = cellBuffer[i];

                // 计算网格内障碍物
                outHits.Clear();
                physicsWorld.OverlapBox(curCellData.worldPos, quaternion.identity, flowFieldSettingData.cellRadius * one, ref outHits, CollisionFilter.Default);

                // 计算前重置初始值
                curCellData.cost = 1;
                foreach (var hit in outHits)
                {
                    if (hit.Material.CustomTags.Equals(1))//00000001
                    {
                        curCellData.cost++;
                    }
                    else if (hit.Material.CustomTags.Equals(2))//00000010
                    {
                        curCellData.cost = byte.MaxValue;
                        break;
                    }
                }

                // 计算 Integration Field 前重置 bestCost
                curCellData.bestCost = ushort.MaxValue;
                cellBuffer[i] = curCellData;
            }

            outHits.Dispose();

            // Calculate DestinationIndex
            flowFieldSettingData.destinationIndex = FlowFieldHelper.GetCellIndexFromWorldPos(flowFieldSettingData.originPoint, flowFieldSettingData.destination, gridSize, flowFieldSettingData.cellRadius * 2);
            // Update Destination Cell's cost and bestCost
            int flatDestinationIndex = FlowFieldHelper.ToFlatIndex(flowFieldSettingData.destinationIndex, gridSize.y);
            CellData destinationCell = cellBuffer[flatDestinationIndex];
            destinationCell.cost = 0;
            destinationCell.bestCost = 0;
            cellBuffer[flatDestinationIndex] = destinationCell;

            // Integration Field
            NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
            NativeList<int2> neighborIndices = new NativeList<int2>(Allocator.TempJob);
            indicesToCheck.Enqueue(flowFieldSettingData.destinationIndex);
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
        }).Run();
    }
}
