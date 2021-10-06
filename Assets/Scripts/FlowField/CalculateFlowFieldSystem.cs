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
    private EntityCommandBufferSystem _ecbSystem;
    private BuildPhysicsWorld buildPhysicsWorld;

    public static readonly float3 one = new float3(1, 1, 1);

    private Camera _mainCamera;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnStartRunning()
    {
        _mainCamera = Camera.main;
    }

    protected override void OnUpdate()
    {
        // 通过鼠标设置目标点
        if (Input.GetMouseButtonDown(0))
        {
            var settingComponent = GetSingleton<FlowFieldSettingData>();
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            Vector3 worldMousePos = _mainCamera.ScreenToWorldPoint(mousePos);
            settingComponent.destination = worldMousePos;
            SetSingleton<FlowFieldSettingData>(settingComponent);
        }

        var commandBuffer = _ecbSystem.CreateCommandBuffer();
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        Entities.WithAll<CalculateFlowFieldTag>().ForEach((Entity entity, ref DynamicBuffer<EntityBufferElement> buffer, ref FlowFieldSettingData flowFieldSettingData) =>
        {
            DynamicBuffer<Entity> entityBuffer = buffer.Reinterpret<Entity>();
            NativeArray<CellData> cellDataContainer = new NativeArray<CellData>(entityBuffer.Length, Allocator.TempJob);
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);

            int2 gridSize = flowFieldSettingData.gridSize;

            // cost Field
            for (int i = 0; i < entityBuffer.Length; i++)
            {
                CellData curCellData = GetComponentDataFromEntity<CellData>()[entityBuffer[i]];

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
                cellDataContainer[i] = curCellData;
            }

            outHits.Dispose();

            // Calculate DestinationIndex
            flowFieldSettingData.destinationIndex = FlowFieldHelper.GetCellIndexFromWorldPos(flowFieldSettingData.destination, gridSize, flowFieldSettingData.cellRadius * 2);
            // Update Destination Cell's cost and bestCost
            int flatDestinationIndex = FlowFieldHelper.ToFlatIndex(flowFieldSettingData.destinationIndex, gridSize.y);
            CellData destinationCell = cellDataContainer[flatDestinationIndex];
            destinationCell.cost = 0;
            destinationCell.bestCost = 0;
            cellDataContainer[flatDestinationIndex] = destinationCell;

            // Prepare for Integration Field Calculate
            NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.TempJob);
            NativeList<int2> neighborIndices = new NativeList<int2>(Allocator.TempJob);

            indicesToCheck.Enqueue(flowFieldSettingData.destinationIndex);

            // Integration Field
            while (indicesToCheck.Count > 0)
            {
                int2 cellIndex = indicesToCheck.Dequeue();
                int cellFlatIndex = FlowFieldHelper.ToFlatIndex(cellIndex, gridSize.y);
                CellData curCellData = cellDataContainer[cellFlatIndex];
                neighborIndices.Clear();
                FlowFieldHelper.GetNeighborIndices(cellIndex, GridDirection.CardinalAndIntercardinalDirections, gridSize, ref neighborIndices);
                foreach (int2 neighborIndex in neighborIndices)
                {
                    int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
                    CellData neighborCellData = cellDataContainer[flatNeighborIndex];
                    if (neighborCellData.cost == byte.MaxValue)
                    {
                        continue;
                    }

                    if (neighborCellData.cost + curCellData.bestCost < neighborCellData.bestCost)
                    {
                        neighborCellData.bestCost = (ushort)(neighborCellData.cost + curCellData.bestCost);
                        cellDataContainer[flatNeighborIndex] = neighborCellData;
                        indicesToCheck.Enqueue(neighborIndex);
                    }
                }
            }

            // // Flow Field
            // // TODO: Combine with Integration Field
            for (int i = 0; i < cellDataContainer.Length; i++)
            {
                CellData curCellData = cellDataContainer[i];
                neighborIndices.Clear();
                FlowFieldHelper.GetNeighborIndices(curCellData.gridIndex, GridDirection.CardinalAndIntercardinalDirections, gridSize, ref neighborIndices);
                ushort bestCost = curCellData.bestCost;
                int2 bestDirection = int2.zero;
                foreach (int2 neighborIndex in neighborIndices)
                {
                    int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
                    CellData neighborCellData = cellDataContainer[flatNeighborIndex];
                    if (neighborCellData.bestCost < bestCost)
                    {
                        bestCost = neighborCellData.bestCost;
                        bestDirection = neighborCellData.gridIndex - curCellData.gridIndex;
                    }
                }
                curCellData.bestDirection = bestDirection;
                cellDataContainer[i] = curCellData;
            }

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                commandBuffer.SetComponent(entityBuffer[i], cellDataContainer[i]);
            }

            neighborIndices.Dispose();
            cellDataContainer.Dispose();
            indicesToCheck.Dispose();
        }).Run();


    }
}
