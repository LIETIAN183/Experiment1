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
    private NativeArray<int> localDests;
    private NativeParallelMultiHashMap<Entity, float2> bigObstacleHashMap;

    //-------------辅助变量------------------------
    private float gridVolume, pgaInms2;

    private PhysicsWorld physicsWorld;

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
            // displayOffset = math.up(),
            displayOffset = new float3(0, -0.9f, 0),
            index = 2
            // agentIndex = -1,
            // index = -1
        });
        state.EntityManager.AddBuffer<CellBuffer>(entity);
        var desBuffer = state.EntityManager.AddBuffer<DestinationBuffer>(entity);
        desBuffer.Add(117);
        // desBuffer.Add(1657);

        localTransformList = SystemAPI.GetComponentLookup<LocalTransform>(true);
        physicsMassList = SystemAPI.GetComponentLookup<PhysicsMass>(true);
        bigObstacleHashMap = new NativeParallelMultiHashMap<Entity, float2>(1000, Allocator.Persistent);
        gridVolume = 0;
        // state.Enabled = false;
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

        localDests = SystemAPI.GetSingletonBuffer<DestinationBuffer>(true).Reinterpret<int>().AsNativeArray();

        // 更新辅助变量
        if (gridVolume == 0)
        {
            gridVolume = settingData.GetCellGridVolume();
        }
        pgaInms2 = SystemAPI.GetSingleton<TimerData>().curPGA * Constants.gravity;
        physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        //----------END----------------

        state.Dependency = CostJob(ref state, settingData);
        state.Dependency = IntegrationJob(ref state, settingData);
        state.Dependency = FlowFieldJob(ref state, settingData);
        var updateJob = new UpdateDataToBufferJob
        {
            source = localArray,
            target = SystemAPI.GetSingletonBuffer<CellBuffer>().Reinterpret<CellData>().AsNativeArray()
        }.Schedule(localArray.Length, localArray.Length / 4, state.Dependency);

        state.Dependency = updateJob;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        if (localArray.IsCreated) localArray.Dispose();
        if (bigObstacleHashMap.IsCreated) bigObstacleHashMap.Dispose();
        if (localDests.IsCreated) localDests.Dispose();
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

    [BurstCompile]
    public JobHandle CostJob(ref SystemState state, FlowFieldSettingData settingData)
    {
        // 判断数量有无超出
        if (bigObstacleHashMap.Count() > 990)
        {
            SystemAPI.SetSingleton(new MessageEvent
            {
                isActivate = true,
                message = $"bigObstacleHashMap Length Not Enough,Max:1000,Now:{bigObstacleHashMap.Count()}",
                displayForever = true
            });
        }
        bigObstacleHashMap.Clear();
        var costJob4 = new JobHandle();
        if (settingData.index == 3)
        {
            costJob4 = new BasicCalculateCostJob
            {
                cells = localArray,
                physicsWorld = this.physicsWorld,
                cellRadius = settingData.cellRadius
            }.Schedule(localArray.Length, localArray.Length / 4, state.Dependency);
        }
        else
        {
            var costJob1 = new CalculateCostStep1Job
            {
                cells = localArray,
                physicsWorld = this.physicsWorld,
                cellRadius = settingData.cellRadius,
                localTransformList = localTransformList,
                physicsMassList = physicsMassList,
                bigObstacleHashMapWriter = bigObstacleHashMap.AsParallelWriter()
            }.Schedule(localArray.Length, localArray.Length / 4, state.Dependency);

            var costJob2 = new CalculateCostStep2Job
            {
                cells = localArray,
                fluidPosArray = SystemAPI.GetSingletonBuffer<Pos2DBuffer>().Reinterpret<float2>().AsNativeArray(),
                settingData = settingData,
                bigObstacleHashMap = bigObstacleHashMap
            }.Schedule(costJob1);

            var costJob3 = new CalculateCostStep3Job
            {
                cells = localArray,
                pgaInms2 = this.pgaInms2,
                gridVolume = this.gridVolume
            }.Schedule(localArray.Length, localArray.Length / 4, costJob2);

            costJob4 = new CalculateCostStep4Job
            {
                cells = localArray,
                physicsWorld = this.physicsWorld,
                dests = localDests,
                detectArea = math.PI * Constants.destinationAgentOverlapRadius * Constants.destinationAgentOverlapRadius
            }.Schedule(localDests.Length, localDests.Length / 4, costJob3);
        }
        return costJob4;
    }

    public JobHandle IntegrationJob(ref SystemState state, FlowFieldSettingData settingData)
    {
        var integrationJob1 = new CalculateIntegration_destsInit
        {
            dests = localDests,
            cells = localArray
        }.Schedule(localDests.Length, localArray.Length / 4, state.Dependency);

        var integrationJob3 = new JobHandle();
        if (settingData.index == 3)
        {
            integrationJob3 = new CalCulateIntegration_DijkstraJob
            {
                cells = localArray,
                dests = localDests,
                gridSetSize = settingData.gridSetSize
            }.Schedule(integrationJob1);
        }
        else
        {
            var integrationJob2 = new CalculateIntegration_FloodingJob
            {
                cells = localArray,
                dests = localDests,
                halfExtents = settingData.cellRadius,
                physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld
            }.Schedule(localArray.Length, localArray.Length / 4, integrationJob1);

            integrationJob3 = new CalCulateIntegration_FSMJob
            {
                cells = localArray,
                settingData = settingData
            }.Schedule(integrationJob2);
        }
        // 并行快速扫描法，虽然实行并行化，但相比非并行版本，需要迭代更多次，因此放弃并行快速扫描法
        // var integrationJob3 = PFSMExtension.CalculateIntegration_PFSM(integrationJob1, localArray, settingData);

        // 快速行进法
        // {
        //     var integrationJob = new CalCulateIntegration_FMMJob()
        //     {
        //         cells = cells,
        //         settingData = settingData
        //     }.Schedule(costJob);
        //     state.Dependency = integrationJob;
        // }

        // Dijkstra 
        // integrationJob3 = new CalCulateIntegration_DjistraJob
        // {
        //     cells = localArray,
        //     dests = localDests,
        //     gridSetSize = settingData.gridSetSize
        // }.Schedule(integrationJob1);

        return integrationJob3;
    }

    public JobHandle FlowFieldJob(ref SystemState state, FlowFieldSettingData settingData)
    {
        var flowFieldJob1 = new CalculateGlobalFlowFieldJob_NotDestGrid
        {
            cells = localArray,
            gridSetSize = settingData.gridSetSize,
            pgaInms2 = 0
        }.Schedule(localArray.Length, localArray.Length / 4, state.Dependency);


        var flowFieldJob2 = new CalculateGlobalFlowFieldJob_DestGrid
        {
            cells = localArray,
            dests = localDests
        }.Schedule(localDests.Length, localArray.Length / 4, flowFieldJob1);

        var flowFieldJob3 = new CalculateLocalFlowFieldJob_NotDestGrid
        {
            cells = localArray,
            pgaInms2 = 0,
            gridSetSize = settingData.gridSetSize
        }.Schedule(localArray.Length, localArray.Length / 4, flowFieldJob2);

        var flowFieldJob4 = new CalculateLocalFlowFieldJob_DestGrid
        {
            cells = localArray,
            dests = localDests,
            pgaInms2 = 0,
            gridVolume = this.gridVolume,
            gridSetSize = settingData.gridSetSize
        }.Schedule(flowFieldJob3);

        return flowFieldJob4;
    }
}

/// <summary>
/// 计算目标网格的总代价
/// </summary>
[BurstCompile]
public struct CalculateIntegration_destsInit : IJobParallelFor
{
    [ReadOnly] public NativeArray<int> dests;
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;

    public void Execute(int index)
    {
        var curCell = cells[dests[index]];
        curCell.integrationCost = curCell.localCost;
        cells[dests[index]] = curCell;
    }
}

[BurstCompile]
public struct CalculateIntegration_FloodingJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;

    [ReadOnly] public NativeArray<int> dests;
    [ReadOnly] public float3 halfExtents;
    [ReadOnly] public PhysicsWorld physicsWorld;

    public void Execute(int flatIndex)
    {
        CellData curCellData = cells[flatIndex];
        if (curCellData.localCost == 1)
        {
            int minIndex = 0;
            float minDisSquare = float.MaxValue, temp, dis;
            float3 curDir, minDir = float3.zero;
            foreach (var index in dests)
            {
                curDir = cells[index].worldPos - curCellData.worldPos;
                temp = math.lengthsq(curDir);
                if (temp < minDisSquare)
                {
                    minDisSquare = temp;
                    minDir = curDir;
                    minIndex = index;
                }
            }

            dis = math.sqrt(minDisSquare);
            if (!physicsWorld.BoxCast(curCellData.worldPos, quaternion.identity, halfExtents, minDir, dis, Constants.ignorAgentGroundFilter))
            {
                if (curCellData.integrationCost > dis + cells[minIndex].integrationCost)
                {
                    curCellData.integrationCost = dis + cells[minIndex].integrationCost;
                    cells[flatIndex] = curCellData;
                }
            }
        }
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
        int row = gridSetSize.x, column = gridSetSize.y;
        int3x4 horizontal = new int3x4(new int3(0, column - 1, 1), new int3(column - 1, 0, -1), new int3(column - 1, 0, -1), new int3(0, column - 1, 1));
        int3x4 vertical = new int3x4(new int3(0, row - 1, 1), new int3(0, row - 1, 1), new int3(row - 1, 0, -1), new int3(row - 1, 0, -1));

        float2 midValue = float2.zero;
        float newIntCost = 0;
        int i, j;
        float width = settingData.cellRadius.x * 2;
        float twoWidth2 = 2 * width * width;

        bool changed;
        uint count = 0;

        do
        {
            count++;
            changed = false;
            for (int iter = 0; iter < 4; iter++)
            {
                for (i = vertical[iter].x; vertical[iter].z * i <= vertical[iter].y; i += vertical[iter].z)
                {
                    for (j = horizontal[iter].x; horizontal[iter].z * j <= horizontal[iter].y; j += horizontal[iter].z)
                    {
                        var currentIndex = FlowFieldUtility.ToFlatIndex(i, j, column);
                        var current = cells[currentIndex];

                        if (current.localCost < Constants.T_c)
                        {
                            float left, right;
                            left = (i == 0 ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i - 1, j, column)].integrationCost);
                            right = (i == (row - 1) ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i + 1, j, column)].integrationCost);
                            midValue.y = math.min(left, right);

                            left = (j == 0 ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i, j - 1, column)].integrationCost);
                            right = (j == (column - 1) ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i, j + 1, column)].integrationCost);
                            midValue.x = math.min(left, right);

                            if (math.abs(midValue.x - midValue.y) < width * current.localCost)
                            {
                                newIntCost = (midValue.x + midValue.y + math.sqrt(twoWidth2 * current.localCost * current.localCost - (midValue.x - midValue.y) * (midValue.x - midValue.y))) * 0.5f;
                            }
                            else
                            {
                                newIntCost = math.min(midValue.x, midValue.y) + width * current.localCost;
                            }

                            if (newIntCost < current.integrationCost)
                            {
                                current.integrationCost = newIntCost;
                                if (changed == false)
                                {
                                    changed = true;
                                }
                            }
                            // current.integrationCost = math.min(newCost, current.integrationCost);
                            cells[currentIndex] = current;
                        }
                    }
                }
            }
        } while (changed);
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