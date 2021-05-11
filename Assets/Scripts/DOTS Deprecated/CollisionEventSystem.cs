using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(StepPhysicsWorld))]
// [UpdateAfter(typeof(EndFramePhysicsSystem))]
public class CollisionEventSystem : SystemBase
{
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    StepPhysicsWorld m_StepPhysicsWorldSystem;

    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();


    }
    protected override void OnUpdate()
    {
        // 初始化 isGround 状态
        // Entities.WithAll<ComsTag>().WithName("InitCollisionState").ForEach((ref AccData accData) =>
        // {
        //     accData.applyAcc = float3.zero;
        //     // collisionStateData.isGround = false;
        // }).ScheduleParallel();
        // Dependency.Complete();

        Dependency = new CollisionEventJob
        {
            PhysicsCustomTagGroup = GetComponentDataFromEntity<PhysicsCustomTags>(true),
            // CollisionStateDataGroup = GetComponentDataFromEntity<CollisionStateData>(),
            translationGroup = GetComponentDataFromEntity<Translation>(),
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, Dependency);
        Dependency.Complete();
    }

    [BurstCompile]
    struct CollisionEventJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<PhysicsCustomTags> PhysicsCustomTagGroup;

        public ComponentDataFromEntity<Translation> translationGroup;
        // public ComponentDataFromEntity<CollisionStateData> CollisionStateDataGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;


            bool isBodyAComs = PhysicsCustomTagGroup.HasComponent(entityA);
            bool isBodyBComs = PhysicsCustomTagGroup.HasComponent(entityB);
            // 只有两个碰撞体都有 Physics Custom Tag 时才不返回，只检测物体间的碰撞
            if (isBodyAComs || isBodyBComs)
            {
            }
            // else
            // {
            // if (CollisionStateDataGroup.HasComponent(entityA))
            // {
            //     var stateData = CollisionStateDataGroup[entityA];
            //     stateData.isGround = true;
            //     CollisionStateDataGroup[entityA] = stateData;
            // }
            // if (CollisionStateDataGroup.HasComponent(entityB))
            // {
            //     var stateData = CollisionStateDataGroup[entityB];
            //     stateData.isGround = true;
            //     CollisionStateDataGroup[entityB] = stateData;
            // }
            // }

            // if (isBodyAComs && PhysicsCustomTagGroup[entityA].Value == 2)
            // {// EntityA 为地面
            //     var stateData = CollisionStateDataGroup[entityB];
            //     stateData.isGround = true;
            //     CollisionStateDataGroup[entityB] = stateData;

            // }
            // else if (isBodyBComs && PhysicsCustomTagGroup[entityB].Value == 2)
            // {// EntityB 为地面
            //     var stateData = CollisionStateDataGroup[entityA];
            //     stateData.isGround = true;
            //     CollisionStateDataGroup[entityA] = stateData;
            // }
            // else if (CollisionStateDataGroup[entityA].isGround)
            // {// EntityA 接触地面
            //     var stateData = CollisionStateDataGroup[entityB];
            //     stateData.isGround = true;
            //     CollisionStateDataGroup[entityB] = stateData;
            // }
            // else if (CollisionStateDataGroup[entityB].isGround)
            // {// EntityB 接触地面
            //     var stateData = CollisionStateDataGroup[entityA];
            //     stateData.isGround = true;
            //     CollisionStateDataGroup[entityA] = stateData;
            // }
            // else
            // {

            // }
        }
    }
}
