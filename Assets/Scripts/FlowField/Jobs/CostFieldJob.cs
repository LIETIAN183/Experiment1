using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

/// <summary>
/// 进行第一部分的代价统计，网格内小商品的质量变量和，并存储大型障碍物的相关信息用于第二部分的计算
/// 使用前 Clear bigObstacleHashMapWriter
/// </summary>
[BurstCompile]
public struct CalculateCostStep1Job : IJobParallelFor
{
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public float3 cellRadius;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformList;
    [ReadOnly] public ComponentLookup<PhysicsMass> physicsMassList;
    public NativeArray<CellData> cells;
    public NativeParallelMultiHashMap<Entity, float2>.ParallelWriter bigObstacleHashMapWriter;

    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapBox(curCell.worldPos, quaternion.identity, cellRadius, ref outHits, Constants.ignorAgentGroundFilter);

        // costField 参数初始化
        curCell.massVariable = 0;
        curCell.maxHeight = 0;
        curCell.localCost = 0;
        curCell.fluidElementCount = 0;
        foreach (var hit in outHits)
        {
            // 墙壁等障碍物
            if ((hit.Material.CustomTags & 0b_0100_0000) != 0)
            {
                curCell.localCost = Constants.T_c;
                // curCell.localCost = float.MaxValue;
                break;
            }

            // c1表示物体外形系数 1为粗糙，2为光滑，4为锋利
            int c1 = (hit.Material.CustomTags & 0b_0001_1100) >> 2;
            // c2代表物体是否为实心，实心值为1，空心值为2
            int c2 = (hit.Material.CustomTags & 0b_0010_0000) != 0 ? 1 : 2;

            // 小型障碍物
            if ((hit.Material.CustomTags & 0b_0000_0001) != 0)
            {
                // 表明该小型障碍物的中心处于本地网格内
                var entityPos = localTransformList[hit.Entity].Position;
                if (math.abs(entityPos.x - curCell.worldPos.x) < cellRadius.x && math.abs(entityPos.z - curCell.worldPos.z) < cellRadius.z)
                {
                    curCell.massVariable += (c1 * c2) / physicsMassList[hit.Entity].InverseMass;
                    curCell.maxHeight = math.max(curCell.maxHeight, hit.Position.y);
                }
            }
            // 大型障碍物
            if ((hit.Material.CustomTags & 0b_0000_0010) != 0)
            {
                bigObstacleHashMapWriter.Add(hit.Entity, new float2(flatIndex, (c1 * c2) / physicsMassList[hit.Entity].InverseMass));
                curCell.maxHeight = math.max(curCell.maxHeight, hit.Position.y);
            }
        }
        cells[flatIndex] = curCell;
        outHits.Dispose();
    }
}

/// <summary>
/// 第二部分的代价计算，计算大障碍物的代价时依据其占据的网格等分，统计流体影响
/// </summary>
[BurstCompile]
public struct CalculateCostStep2Job : IJob
{
    public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<float2> fluidPosArray;
    [ReadOnly] public FlowFieldSettingData settingData;
    [ReadOnly] public NativeParallelMultiHashMap<Entity, float2> bigObstacleHashMap;
    public void Execute()
    {
        var cellDiameter = settingData.cellRadius * 2;
        // 遍历所有液体坐标，并对本地网格赋值
        foreach (var itemPos in fluidPosArray.Reinterpret<float2>())
        {
            var flatIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(itemPos, settingData.originPoint, settingData.gridSetSize, cellDiameter);
            if (flatIndex < 0) continue;
            var curCell = cells[flatIndex];
            curCell.fluidElementCount++;
            cells[flatIndex] = curCell;
        }

        //遍历所有大障碍物，并对所处的网格赋值
        // 之所以要返回 length 的原因是 entityArray 返回了所有的 Key,但 length后的都是重复的
        var (entityArray, length) = bigObstacleHashMap.GetUniqueKeyArray(Allocator.Temp);
        for (int i = 0; i < length; ++i)
        {
            var curEntity = entityArray[i];
            var curFlatIndexCount = bigObstacleHashMap.CountValuesForKey(curEntity);
            var flatIndexArray = bigObstacleHashMap.GetValuesForKey(curEntity);
            foreach (var item in flatIndexArray)
            {
                int flatIndex = (int)item.x;
                var curCell = cells[flatIndex];
                curCell.massVariable += item.y / curFlatIndexCount;
                // curCell.massVariable += item.y;
                cells[flatIndex] = curCell;
            }
        }
        entityArray.Dispose();
    }
}

/// <summary>
/// 最终计算每个网格的 localCost
/// </summary>
[BurstCompile]
public struct CalculateCostStep3Job : IJobParallelFor
{
    // 其余网格
    [ReadOnly] public float pgaInms2, gridVolume;
    public NativeArray<CellData> cells;

    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        // 不超出阈值，即不存在大型障碍物时才计算
        if (curCell.localCost == 0)
        {
            // curCell.localCost = (curCell.massVariable + Constants.c2_fluid * curCell.fluidElementCount * 0.0083f) / gridVolume + math.exp(curCell.maxHeight) + curCell.maxHeight * Constants.w_s;
            curCell.localCost = (uint)(math.exp(-pgaInms2) * (curCell.massVariable + Constants.c2_fluid * curCell.fluidElementCount * 0.0083f) / gridVolume + math.exp(curCell.maxHeight) + curCell.maxHeight * Constants.c_s);
        }
        if (curCell.localCost > Constants.T_c)
        {
            curCell.localCost = Constants.T_c;
        }

        // integration Field & Flow Field参数初始化
        curCell.globalDir = float2.zero;
        curCell.integrationCost = Constants.T_i;
        cells[flatIndex] = curCell;
    }
}

/// <summary>
/// 计算目标网格的总代价
/// </summary>
[BurstCompile]
public struct CalculateCostStep4Job : IJobParallelFor
{
    // 用于处理目标网格
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public NativeArray<int> dests;
    [ReadOnly] public float detectArea;
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;
    public void Execute(int index)
    {
        var curCell = cells[dests[index]];
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapSphere(curCell.worldPos, Constants.destinationAgentOverlapRadius, ref outHits, Constants.agentOnlyFilter);
        var agentNumber = outHits.Length;
        // curCell.localCost += Constants.w_a * outHits.Length / detectArea;
        curCell.localCost += Constants.w_a * outHits.Length / detectArea;
        outHits.Dispose();
        cells[dests[index]] = curCell;
    }
}