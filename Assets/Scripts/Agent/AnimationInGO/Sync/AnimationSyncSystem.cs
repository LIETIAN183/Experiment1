using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using FischlWorks;

// 渲染行人动画效果
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class AnimationSyncSystem : SystemBase
{
    public Object[] prefabsInGO;
    protected override void OnCreate()
    {
        prefabsInGO = new Object[4];
        // prefabsInGO[0] = Resources.Load("AniPrefab1");
        // prefabsInGO[1] = Resources.Load("AniPrefab2");
        // prefabsInGO[2] = Resources.Load("AniPrefab3");
        // prefabsInGO[3] = Resources.Load("AniPrefab4");
        // prefabsInGO[4] = Resources.Load("AniPrefab0");
        prefabsInGO[0] = Resources.Load("RagDollAgent1Root");
        prefabsInGO[1] = Resources.Load("RagDollAgent2Root");
        prefabsInGO[2] = Resources.Load("RagDollAgent3Root");
        prefabsInGO[3] = Resources.Load("RagDollAgent4Root");
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        // UnityEngine.Debug.Log(SystemAPI.Time.DeltaTime);
        var ecb = new EntityCommandBuffer(Allocator.Persistent);

        var randomSeed = SystemAPI.GetSingleton<RandomSeed>();
        var random = Unity.Mathematics.Random.CreateFromIndex((uint)(SystemAPI.GetSingleton<RandomSeed>().seed + SystemAPI.Time.ElapsedTime.GetHashCode()));

        this.EntityManager.CompleteDependencyBeforeRW<LocalTransform>();
        this.EntityManager.CompleteDependencyBeforeRO<PhysicsVelocity>();

        // 检测没有对应渲染 GO 的 Entity，并生成渲染 GO
        foreach (var (localTransform, data, entity) in SystemAPI.Query<RefRO<LocalTransform>, AgentMovementData>().WithAll<AgentMovementData>().WithNone<GOReference>().WithEntityAccess())
        {
            GameObject aniGO = GameObject.Instantiate(prefabsInGO[random.NextInt(0, 4)]) as GameObject;
            aniGO.transform.position = localTransform.ValueRO.Position - new float3(0, 1, 0);
            aniGO.GetComponentInChildren<FootInteraction>().entity = entity;
            var animator = aniGO.GetComponentInChildren<UnityEngine.Animator>();
            aniGO.transform.forward = localTransform.ValueRO.Forward();
            // 配置 offset 值让其设置初始帧不同，多个智能体的移动动画也会产生差异
            animator.SetFloat("offset", random.NextFloat(0, 1));
            ecb.AddComponent<GOReference>(entity, new GOReference { transform = aniGO.transform, animator = animator, information = aniGO.GetComponentInChildren<csHomebrewIK>(), ragdoll = aniGO.GetComponentInChildren<FootInteraction>(), aniSpeed = 0 });
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
            accData = SystemAPI.GetSingleton<TimerData>(),
            randomSeed = random.NextUInt()
        }.Run();
    }


}

/// <summary>
/// 同步渲染动画、方向以及 Entity 高度
/// </summary>
[WithAll(typeof(AgentMovementData)), WithAny(typeof(Escaping))]
partial struct AnimationSyncJob : IJobEntity
{
    [ReadOnly] public TimerData accData;

    [ReadOnly] public uint randomSeed;
    void Execute([EntityIndexInQuery] int index, ref LocalTransform localTransform, ref AgentMovementData data, in LocalToWorld worldTransform, in PhysicsVelocity velocity, in GOReference reference)
    {
        if (!reference.transform.gameObject.activeInHierarchy)
        {
            reference.transform.gameObject.SetActive(true);
        }
        float3 temp = worldTransform.Position;
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
        reference.aniSpeed = math.lerp(reference.aniSpeed, math.length(velocity.Linear.xz + accData.curVel.xz), 1f);
        // reference.aniSpeed = 0;
        // if (math.lengthsq(accData.curVel.xz) > math.lengthsq(velocity.Linear.xz))
        // 改用重心判定的方法
        // 行人摇摆动画
        // if (math.length(accData.curAcc) > 3f)
        // {
        //     reference.animator.SetBool("isBalance", true);
        //     var random = Unity.Mathematics.Random.CreateFromIndex(randomSeed + (uint)index);
        //     var x = random.NextInt(0, 4);
        //     reference.animator.SetInteger("ShakingIndex", x);
        // }
        // else
        // {
        //     reference.animator.SetBool("isBalance", false);
        // }
        // 行人摔倒动画
        bool falling = !(reference.ragdoll.master.state == RootMotion.Dynamics.PuppetMaster.State.Alive);
        if (!falling && data.isFall)
        {
            reference.ragdoll.master.state = RootMotion.Dynamics.PuppetMaster.State.Dead;
            reference.aniSpeed = 0;
        }
        else if (falling && !data.isFall)
        {
            reference.ragdoll.master.state = RootMotion.Dynamics.PuppetMaster.State.Alive;
        }

        // 同步行人动画速度
        reference.animator.SetFloat("Velocity", reference.aniSpeed);

        // 同步人物高度以及高度差数据
        data.deltaHeight = reference.information.deltaHeight;
        // localTransform.Position.y = reference.information.curHeight;
    }
}