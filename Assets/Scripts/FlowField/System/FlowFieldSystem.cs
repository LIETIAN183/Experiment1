using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
[BurstCompile]
public partial struct FlowFieldSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformList;
    private ComponentLookup<PhysicsMass> physicsMassList;

    private NativeArray<CellData> localArray;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.SetName(entity, "FlowFieldEntity");
        state.EntityManager.AddComponentData<FlowFieldSettingData>(entity, new FlowFieldSettingData
        {
            originPoint = new float3(-12, 1, -10),
            gridSetSize = new int2(44, 40),
            cellRadius = new float3(0.25f, 1, 0.25f),
            destination = new float3(-10.8f, 0, 8.8f),
            displayOffset = math.up(),
            index = 2
        });
        state.EntityManager.AddBuffer<CellBuffer>(entity);

        localTransformList = SystemAPI.GetComponentLookup<LocalTransform>(true);
        physicsMassList = SystemAPI.GetComponentLookup<PhysicsMass>(true);

        state.Enabled = false;
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var settingData = SystemAPI.GetSingleton<FlowFieldSettingData>();

        state.Dependency = JobHandle.CombineDependencies(state.Dependency, ResizeData(settingData));

        if (localArray.Length <= 0) return;

        state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();

        localTransformList.Update(ref state);
        physicsMassList.Update(ref state);

        var costJob = new CalculateCostJob()
        {
            cells = localArray,
            physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            pgaInms2 = SystemAPI.GetSingleton<TimerData>().curPGA * Constants.gravity,
            cellRadius = settingData.cellRadius,
            localTransformList = localTransformList,
            physicsMassList = physicsMassList
        }.Schedule(localArray.Length, 32, state.Dependency);

        // costJob.Complete();

        // 并行快速扫描法，虽然实行并行化，但相比非并行版本，需要迭代更多次，因此放弃并行快速扫描法
        // {
        //     state.Dependency = PFSMExtension.CalculateIntegration_PFSM(costJob, cells, settingData);
        //     for (int i = 0; i < 10; i++)
        //     {
        //         state.Dependency = PFSMExtension.CalculateIntegration_PFSM(state.Dependency, cells, settingData);
        //     }
        // }

        // 快速行进法
        // {
        //     var integrationJob = new CalCulateIntegration_FMMJob()
        //     {
        //         cells = cells,
        //         settingData = settingData
        //     }.Schedule(costJob);
        //     state.Dependency = integrationJob;
        // }

        // Djistra
        // {
        //     var integrationJob = new CalCulateIntegration_DjistraJob()
        //     {
        //         cells = cells,
        //         settingData = settingData
        //     }.Schedule(costJob);
        //     state.Dependency = integrationJob;
        // }

        var integrationJob = new CalCulateIntegration_FSMJob()
        {
            cells = localArray,
            settingData = settingData
        }.Schedule(costJob);

        var flowFieldJob = new CalculateFlowFieldJob()
        {
            cells = localArray,
            gridSetSize = settingData.gridSetSize
        }.Schedule(localArray.Length, 32, integrationJob);

        var cells = SystemAPI.GetSingletonBuffer<CellBuffer>().Reinterpret<CellData>().AsNativeArray();
        var updateJob = new UpdateDataToBufferJob
        {
            source = localArray,
            target = cells
        }.Schedule(localArray.Length, 32, flowFieldJob);

        state.Dependency = updateJob;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        localArray.Dispose();
    }

    [BurstCompile]
    public JobHandle ResizeData(FlowFieldSettingData settingData)
    {
        int2 gridSetSize = settingData.gridSetSize;
        int gridCount = gridSetSize.x * gridSetSize.y;

        if (!localArray.IsCreated || localArray.Length != gridCount)
        {
            localArray = new NativeArray<CellData>(gridCount, Allocator.Persistent);
            var cellBuffer = SystemAPI.GetSingletonBuffer<CellBuffer>().Reinterpret<CellData>();
            cellBuffer.Clear();

            float3 originPoint = settingData.originPoint;
            float3 cellRadius = settingData.cellRadius;
            for (int x = 0; x < gridSetSize.x; x++)
            {
                for (int y = 0; y < gridSetSize.y; y++)
                {
                    float3 cellWorldPos = new float3(originPoint.x + (2 * x + 1) * cellRadius.x, originPoint.y, originPoint.z + (2 * y + 1) * cellRadius.z);
                    CellData newCellData = new CellData
                    {
                        worldPos = cellWorldPos,
                        gridIndex = new int2(x, y)
                    };
                    localArray[FlowFieldUtility.ToFlatIndex(x, y, gridSetSize.y)] = newCellData;
                    cellBuffer.Add(newCellData);
                }
            }
        }
        return new JobHandle();
    }
}

[BurstCompile]
public struct CalculateCostJob : IJobParallelFor
{
    public NativeArray<CellData> cells;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public float pgaInms2;
    [ReadOnly] public float3 cellRadius;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformList;
    [ReadOnly] public ComponentLookup<PhysicsMass> physicsMassList;
    public void Execute(int index)
    {
        var curCell = cells[index];
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapBox(curCell.worldPos, quaternion.identity, cellRadius, ref outHits, CollisionFilter.Default);
        float cost = 0, max_height = 0, sum_mass = 0;
        foreach (var hit in outHits)
        {
            if (cost < 255)
            {
                if ((hit.Material.CustomTags & 0b_0000_1000) != 0)
                {
                    cost = 255;
                    break;
                }

                var entityPos = localTransformList[hit.Entity].Position;
                if ((hit.Material.CustomTags & 0b_0000_0011) != 0)
                {
                    // 小障碍物的customtags值为1，所以无影响，中等障碍物的customtags值为2，所以计算高度时为其坐标×2
                    if (entityPos.y * hit.Material.CustomTags > max_height) max_height = hit.Material.CustomTags * entityPos.y;

                    var component = physicsMassList[hit.Entity];
                    sum_mass += 1 / component.InverseMass;
                    // max_height = 1;
                    // sum_mass += 1;
                }
            }
        }
        // cost += math.exp(-pgaInms2) * sum_mass * max_height * 2 + math.exp(max_height);
        cost += sum_mass * max_height * 2 + math.exp(max_height);
        // cost += math.exp(-pgaInms2) * sum_mass * max_height * 2;
        if (cost > 255) cost = 255;
        curCell.cost = (byte)cost;
        curCell.bestCost = ushort.MaxValue;
        // curCell.updated = false;
        curCell.bestDir = float2.zero;
        curCell.tempCost = float.MaxValue;
        // curCell.state = State.Far;
        cells[index] = curCell;
        outHits.Dispose();
    }
}

[BurstCompile]
public struct CalCulateIntegration_FSMJob : IJob
{
    public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;

    public void Execute()
    {
        var gridSetSize = settingData.gridSetSize;

        var destinationIndex = FlowFieldUtility.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSetSize, settingData.cellRadius * 2);
        // Update Destination Cell's cost and bestCost
        int flatDestinationIndex = FlowFieldUtility.ToFlatIndex(destinationIndex, gridSetSize.y);
        CellData destinationCell = cells[flatDestinationIndex];
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;
        destinationCell.tempCost = 0;
        cells[flatDestinationIndex] = destinationCell;

        int row = gridSetSize.x, column = gridSetSize.y;
        int3x4 horizontal = new int3x4(new int3(0, column - 1, 1), new int3(column - 1, 0, -1), new int3(column - 1, 0, -1), new int3(0, column - 1, 1));
        int3x4 vertical = new int3x4(new int3(0, row - 1, 1), new int3(0, row - 1, 1), new int3(row - 1, 0, -1), new int3(row - 1, 0, -1));

        float2 midValue = float2.zero;
        float newCost = 0;
        int i, j;
        // double h = 0.5, f = 1.0;

        for (int iter = 0; iter < 4; iter++)
        {

            for (i = vertical[iter].x; vertical[iter].z * i <= vertical[iter].y; i += vertical[iter].z)
            {
                for (j = horizontal[iter].x; horizontal[iter].z * j <= horizontal[iter].y; j += horizontal[iter].z)
                {
                    var currentIndex = FlowFieldUtility.ToFlatIndex(i, j, gridSetSize.y);
                    var current = cells[currentIndex];

                    if (current.cost < 255)
                    {
                        // === neighboring cells (Upwind Godunov) ===
                        if (i == 0)
                        {
                            midValue.y = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i + 1, j, gridSetSize.y)].tempCost);
                        }
                        else if (i == (row - 1))
                        {
                            midValue.y = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i - 1, j, gridSetSize.y)].tempCost);
                        }
                        else
                        {
                            midValue.y = midValue.y = math.min(cells[FlowFieldUtility.ToFlatIndex(i - 1, j, gridSetSize.y)].tempCost, cells[FlowFieldUtility.ToFlatIndex(i + 1, j, gridSetSize.y)].tempCost);
                        }

                        if (j == 0)
                        {
                            midValue.x = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i, j + 1, gridSetSize.y)].tempCost);
                        }
                        else if (j == (column - 1))
                        {
                            midValue.x = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i, j - 1, gridSetSize.y)].tempCost);
                        }
                        else
                        {
                            midValue.x = math.min(cells[FlowFieldUtility.ToFlatIndex(i, j - 1, gridSetSize.y)].tempCost, cells[FlowFieldUtility.ToFlatIndex(i, j + 1, gridSetSize.y)].tempCost);
                        }

                        if (math.abs(midValue.x - midValue.y) < 0.5f * current.cost)
                        {
                            newCost = (midValue.x + midValue.y + math.sqrt(0.5f * current.cost * current.cost - (midValue.x - midValue.y) * (midValue.x - midValue.y))) * 0.5f;
                        }
                        else
                        {
                            newCost = math.min(midValue.x, midValue.y) + 0.5f;
                        }

                        current.tempCost = math.min(newCost, current.tempCost);
                        cells[currentIndex] = current;
                    }
                }
            }
        }
    }
}



[BurstCompile]
public struct CalculateFlowFieldJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public int2 gridSetSize;
    public void Execute(int index)
    {
        var curCell = cells[index];
        if (curCell.tempCost == float.MaxValue)
        {
            return;
        }
        foreach (int2 neighborIndex in FlowFieldUtility.GetNeighborIndices(curCell.gridIndex, gridSetSize))
        {
            int flatNeighborIndex = FlowFieldUtility.ToFlatIndex(neighborIndex, gridSetSize.y);
            CellData neighborCellData = cells[flatNeighborIndex];
            // if (neighborCellData.bestCost.Equals(ushort.MaxValue)) continue;
            if (neighborCellData.tempCost.Equals(float.MaxValue)) continue;
            // var temp = curCell.bestCost - neighborCellData.bestCost;
            var gradient = curCell.tempCost - neighborCellData.tempCost;
            if (gradient == 0) continue;
            var dir = (float2)(neighborCellData.gridIndex - curCell.gridIndex);
            if (dir.x == 0 || dir.y == 0)
            {
                curCell.bestDir += gradient * dir;
            }
            else
            {
                curCell.bestDir += gradient * dir / 2;
            }
        }
        cells[index] = curCell;
    }
}

[BurstCompile]
public struct UpdateDataToBufferJob : IJobParallelFor
{

    [ReadOnly] public NativeArray<CellData> source;
    public NativeArray<CellData> target;
    public void Execute(int index)
    {
        target[index] = source[index];
    }
}
