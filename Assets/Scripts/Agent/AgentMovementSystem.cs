using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
public class AgentMovementSystem : SystemBase
{
    private float accInMenmory;
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();
        if (cellBuffer.Length == 0) return;
        var settingData = GetSingleton<FlowFieldSettingData>();

        // 记录仿真以来的最大地震强度，以此来进行延迟时间的计算
        float acc = math.length(GetSingleton<AccTimerData>().acc);
        if (accInMenmory < acc) accInMenmory = acc;
        float accTemp = accInMenmory;
        float elapsedTime = GetSingleton<AccTimerData>().elapsedTime;
        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = Time.DeltaTime;
        // 用于判断智能体是否到达终点
        int2 destinationIndex = FlowFieldHelper.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, settingData.gridSize, settingData.cellRadius * 2);
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        var SFMtarget = settingData.destination;

        Entities.WithReadOnly(cellBuffer).WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, ref AgentMovementData movementData, in Translation translation, in PhysicsMass mass) =>
        {
            switch (movementData.state)
            {
                case AgentState.NotActive:
                    velocity.Linear = float3.zero;
                    return;

                case AgentState.Delay:
                    if (movementData.reactionTimeVariable * 25 * math.exp(-accTemp) < elapsedTime)
                    {
                        movementData.state = AgentState.Escape;
                        movementData.reactionTime = movementData.reactionTimeVariable * 25 * math.exp(-accTemp);
                    }
                    return;
                case AgentState.Escape:
                    // 获得当前所在位置的坐标
                    int2 localCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(translation.Value, settingData.originPoint, settingData.gridSize, settingData.cellRadius * 2);

                    // 到达目的地，停止运动
                    if (localCellIndex.Equals(destinationIndex))
                    {
                        movementData.state = AgentState.NotActive;
                        return;
                    }

                    // 获得当前的期望方向
                    int flatLocalCellIndex = FlowFieldHelper.ToFlatIndex(localCellIndex, settingData.gridSize.y);
                    var tempDirection = cellBuffer[flatLocalCellIndex].bestDirection;
                    float3 desireDirection = math.normalize(new float3(tempDirection.x, 0, tempDirection.y));

                    // 计算附近的障碍物与智能体
                    NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
                    physicsWorld.OverlapSphere(translation.Value, 1, ref outHits, CollisionFilter.Default);
                    float3 agentforce = 0;
                    float3 obstacleforce = 0;
                    foreach (var hit in outHits)
                    {
                        if (hit.Material.CustomTags.Equals(2))//00000010 障碍物
                        {
                            var direction = translation.Value - hit.Position;
                            direction.y = 0;
                            direction = math.normalize(direction);
                            obstacleforce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                        }
                        else if (hit.Material.CustomTags.Equals(4))//00000100 智能体
                        {
                            // 如果为自身，跳过
                            if (hit.Entity.Equals(entity)) continue;
                            var direction = translation.Value - hit.Position;
                            direction.y = 0;
                            direction = math.normalize(direction);

                            agentforce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                        }
                    }

                    var SFMDirection = SFMtarget - translation.Value;
                    SFMDirection.y = 0;
                    SFMDirection = math.normalize(SFMDirection);
                    velocity.Linear += (SFMDirection * movementData.desireSpeed - velocity.Linear) + (obstacleforce + agentforce) * mass.InverseMass * deltaTime;
                    //TODO: 到终点附近时agentForce暴涨，会把自身弹开
                    outHits.Dispose();
                    return;
                default:
                    return;
            }
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
