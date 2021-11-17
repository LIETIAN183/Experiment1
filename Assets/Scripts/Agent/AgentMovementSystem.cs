using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;

// TODO: 添加货架的排斥力
[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
// [DisableAutoCreation]
public class AgentMovementSystem : SystemBase
{
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

        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        var acc = GetSingleton<AccTimerData>().acc;

        var horizontalAcc = acc.xz;

        var vertiAcc = math.abs(acc.y);

        var accMagnitude = math.length(acc);

        var _pga = GetSingleton<AccTimerData>().pga;

        Entities.WithReadOnly(cellBuffer).WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, ref AgentMovementData movementData, in Translation translation, in PhysicsMass mass) =>
        {
            if (movementData.state == AgentState.Escape)
            {
                // 获得当前所在位置的网格 Index
                int2 localCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(translation.Value, settingData.originPoint, settingData.gridSize, settingData.cellRadius * 2);
                // 获得当前的期望方向
                int flatLocalCellIndex = FlowFieldHelper.ToFlatIndex(localCellIndex, settingData.gridSize.y);
                float2 desireDirection = math.normalizesafe(cellBuffer[flatLocalCellIndex].bestDirection);

                float2 interactionForce = 0;
                // pga 超过2没必要计算排斥力了
                if (_pga < 2)
                {
                    // 计算和其他智能体的排斥力
                    NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
                    physicsWorld.OverlapSphere(translation.Value, 1, ref outHits, CollisionFilter.Default);
                    foreach (var hit in outHits)
                    {
                        if ((hit.Material.CustomTags & 0b_0001_0000) != 0)
                        {
                            if (hit.Entity.Equals(entity)) continue;
                            var direction = math.normalizesafe(translation.Value.xz - hit.Position.xz);
                            interactionForce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f - _pga) * direction;
                        }
                    }

                    outHits.Dispose();
                }
                var desireSpeed = math.exp(-vertiAcc - translation.Value.y + 1.05f) * movementData.stdVel;// originPosition.y 取代 1.05f

                velocity.Linear.xz += ((desireDirection * desireSpeed - velocity.Linear.xz) / 0.5f + horizontalAcc + interactionForce * mass.InverseMass) * deltaTime;
            }
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
