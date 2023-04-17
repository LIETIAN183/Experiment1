using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using FischlWorks;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class AnimationSyncSystem : SystemBase
{
    public Object[] prefabsInGO;
    protected override void OnCreate()
    {
        prefabsInGO = new Object[4];
        prefabsInGO[0] = Resources.Load("AniPrefab1");
        prefabsInGO[1] = Resources.Load("AniPrefab2");
        prefabsInGO[2] = Resources.Load("AniPrefab3");
        prefabsInGO[3] = Resources.Load("AniPrefab4");
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Persistent);

        var randomSeed = SystemAPI.GetSingleton<RandomSeed>();
        var random = Unity.Mathematics.Random.CreateFromIndex((uint)(SystemAPI.GetSingleton<RandomSeed>().seed + SystemAPI.Time.ElapsedTime.GetHashCode()));

        this.EntityManager.CompleteDependencyBeforeRW<LocalTransform>();
        this.EntityManager.CompleteDependencyBeforeRO<PhysicsVelocity>();

        // 检测没有对应渲染 GO 的 Entity，并生成渲染 GO
        foreach (var (localTransform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<AgentMovementData>().WithNone<GOReference>().WithEntityAccess())
        {
            GameObject aniGO = GameObject.Instantiate(prefabsInGO[random.NextInt(0, 4)]) as GameObject;
            aniGO.transform.position = localTransform.ValueRO.Position - new float3(0, 1, 0);
            aniGO.GetComponent<FootInteraction>().entity = entity;
            var animator = aniGO.GetComponent<UnityEngine.Animator>();
            aniGO.transform.forward = localTransform.ValueRO.Forward();
            // 配置 offset 值让其设置初始帧不同，多个智能体的移动动画也会产生差异
            animator.SetFloat("offset", random.NextFloat(0, 1));
            ecb.AddComponent<GOReference>(entity, new GOReference { transform = aniGO.transform, animator = animator, information = aniGO.GetComponent<csHomebrewIK>(), aniSpeed = 0 });
        }
        ecb.Playback(this.EntityManager);
        ecb.Dispose();

        // 检测已经成功疏散的 Entity，关闭对应 GO 的渲染
        foreach (var reference in SystemAPI.Query<GOReference>().WithAll<AgentMovementData, Escaped>().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            if (reference.transform.gameObject.activeInHierarchy)
            {
                reference.animator.SetFloat("Velocity", 0);
                reference.transform.gameObject.SetActive(false);
            }
        }

        // 同步渲染动画、方向以及 Entity 高度
        new AnimationSyncJob
        {
            accData = SystemAPI.GetSingleton<TimerData>()
        }.Run();
    }


}


[WithAll(typeof(AgentMovementData)), WithAny(typeof(Escaping), typeof(Idle))]
partial struct AnimationSyncJob : IJobEntity
{
    [ReadOnly] public TimerData accData;
    void Execute(ref LocalTransform localTransform, ref AgentMovementData data, in PhysicsVelocity velocity, in GOReference reference)
    {
        if (!reference.transform.gameObject.activeInHierarchy)
        {
            reference.transform.gameObject.SetActive(true);
        }
        float3 temp = localTransform.Position;
        temp.y = reference.transform.position.y;
        reference.transform.position = temp;
        var horiVel = velocity.Linear;
        horiVel.y = 0;
        // 速度不为 0 时调整行人朝向
        if (!horiVel.Equals(float3.zero))
        {
            reference.transform.forward = localTransform.Forward();
        }

        // 调整动画速度
        reference.aniSpeed = math.lerp(reference.aniSpeed, math.length(velocity.Linear.xz + accData.curVel.xz), 0.5f);
        reference.animator.SetFloat("Velocity", reference.aniSpeed);

        // 同步人物高度以及高度差数据
        data.deltaHeight = reference.information.deltaHeight;
        localTransform.Position.y = reference.information.curHeight;
    }
}