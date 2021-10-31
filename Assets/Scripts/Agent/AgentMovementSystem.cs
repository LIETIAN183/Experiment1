using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
// [DisableAutoCreation]
public class AgentMovementSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate() => buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    protected override void OnUpdate()
    {
        DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();
        if (cellBuffer.Length == 0) return;
        var settingData = GetSingleton<FlowFieldSettingData>();

        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        Entities.WithReadOnly(cellBuffer).ForEach((Entity entity, ref PhysicsVelocity velocity, ref AgentMovementData movementData, in Translation translation, in PhysicsMass mass) =>
        {
            if (movementData.state == AgentState.Escape)
            {
                // 获得当前所在位置的网格 Index
                int2 localCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(translation.Value, settingData.originPoint, settingData.gridSize, settingData.cellRadius * 2);
                // 获得当前的期望方向
                int flatLocalCellIndex = FlowFieldHelper.ToFlatIndex(localCellIndex, settingData.gridSize.y);
                float2 desireDirection = math.normalizesafe(cellBuffer[flatLocalCellIndex].bestDirection);

                // 计算附近的障碍物与智能体
                // NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
                // physicsWorld.OverlapSphere(translation.Value, 1, ref outHits, CollisionFilter.Default);
                // float3 interactionForce = 0;
                // foreach (var hit in outHits)
                // {
                //     if (hit.Material.CustomTags.Equals(2) || hit.Material.CustomTags.Equals(4))//00000010 障碍物
                //     {
                //         if (hit.Entity.Equals(entity)) continue;
                //         var direction = translation.Value - hit.Position;
                //         direction.y = 0;
                //         direction = math.normalize(direction);
                //         interactionForce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                //     }
                // }
                velocity.Linear.xz += (desireDirection * movementData.desireSpeed - velocity.Linear.xz) / 0.5f * deltaTime;

                // + interactionForce* mass.InverseMass * deltaTime;
                // outHits.Dispose();
            }
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
