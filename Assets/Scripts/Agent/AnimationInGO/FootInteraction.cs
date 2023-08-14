using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Drawing;
using FischlWorks;
using Unity.Transforms;
using RootMotion.Dynamics;

// 实现行人跨越障碍物以及与地面障碍物之间的物理交互
[RequireComponent(typeof(Animator))]
public class FootInteraction : MonoBehaviour
{
    public Animator animator;

    private csHomebrewIK ik;

    private float3 leftFootPos, rightFootPos, leftToes, rightToes, leftCenter, rightCenter;

    private EntityManager entityManager;

    private EntityQuery physicsWorldQuery;

    private EntityQuery physicsVelocityQuery;

    public float massCenterHeight;

    [SerializeField]
    private PhysicsCategoryTags detectLayers = PhysicsCategoryTags.Everything;
    private CollisionFilter detectFilter = CollisionFilter.Default;

    [SerializeField]
    private float footOverlapSphereRadius = 0.15f;


    public bool debug = false;

    public Entity entity;


    public PuppetMaster master;

    private float aniSpeedBackup;

    private bool generateFlag = false;

    private List<ReturnToPoolInTime> poolList;

    private float3[] lastVelArray;

    private float3 boxSize = new float3(0.5f, 0.5f, 0.5f);

    public Vector3 offset;

    public Transform headTransform;


    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        ik = this.GetComponent<csHomebrewIK>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        physicsWorldQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        physicsVelocityQuery = entityManager.CreateEntityQuery(typeof(PhysicsVelocity));
        detectFilter.CollidesWith = detectLayers.Value;
        poolList = new List<ReturnToPoolInTime>();
        // lastVelArray = new float3[1];
        // master.mode = PuppetMaster.Mode.Disabled;

        // headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    // Update is called once per frame
    void Update() { }

    void LateUpdate()
    {
        if (debug)
        {
            using (Draw.ingame.WithLineWidth(3))
            {
                Draw.ingame.WireSphere(leftCenter, footOverlapSphereRadius, Color.red);
                Draw.ingame.WireSphere(rightCenter, footOverlapSphereRadius, Color.red);
            }
        }
    }

    /// <summary>
    /// 实现行人踩踏障碍物时与周围障碍物之间的物理交互，以及行人摔倒、抱头行为
    /// </summary>
    void FixedUpdate()
    {
        var physicsSingleton = physicsWorldQuery.GetSingleton<PhysicsWorldSingleton>();
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);

        // 行人踩踏障碍物时，实现与其之间的物理交互
        if (master.state == PuppetMaster.State.Alive)
        {
            leftFootPos = ik.leftFootTransform.position;
            rightFootPos = ik.rightFootTransform.position;
            leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes).position;
            rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes).position;
            leftCenter = (leftFootPos + leftToes) / 2;
            rightCenter = (rightFootPos + rightToes) / 2;

            if (entity != Entity.Null)
            {
                var sumforce = entityManager.GetComponentData<AgentMovementData>(entity).forceForFootInteraction;

                // 左脚
                physicsSingleton.OverlapSphere(leftCenter, footOverlapSphereRadius, ref outHits, detectFilter);
                foreach (var i in outHits)
                {
                    float3 dir = math.normalizesafe(i.Position - leftCenter);
                    var velocity = entityManager.GetComponentData<PhysicsVelocity>(i.Entity);
                    var mass = entityManager.GetComponentData<PhysicsMass>(i.Entity);
                    var force = math.dot(sumforce * (0.5f / outHits.Length), dir) * dir;
                    if (outHits.Length < 5)
                    {
                        velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime * 0.1f);
                    }
                    else
                    {
                        velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime);
                    }
                    entityManager.SetComponentData<PhysicsVelocity>(i.Entity, velocity);
                }

                // 右脚
                physicsSingleton.OverlapSphere(rightCenter, footOverlapSphereRadius, ref outHits, detectFilter);
                foreach (var i in outHits)
                {
                    float3 dir = math.normalizesafe(i.Position - rightCenter);
                    var velocity = entityManager.GetComponentData<PhysicsVelocity>(i.Entity);
                    var mass = entityManager.GetComponentData<PhysicsMass>(i.Entity);
                    var force = math.dot(sumforce * (0.5f / outHits.Length), dir) * dir;
                    if (outHits.Length < 5)
                    {
                        velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime * 0.1f);
                    }
                    else
                    {
                        velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime);
                    }
                    entityManager.SetComponentData<PhysicsVelocity>(i.Entity, velocity);
                }
            }

            // if (master.mode == PuppetMaster.Mode.Active)
            // {
            //     master.mode = PuppetMaster.Mode.Disabled;
            // }
        }

        // 实现行人的摔倒 RagDoll 效果，并与周围环境发生物理交互
        if (master.state == PuppetMaster.State.Dead || master.state == PuppetMaster.State.Frozen)
        {
            if (lastVelArray == null || lastVelArray.Length != master.muscles.Length)
            {
                lastVelArray = new float3[master.muscles.Length];
            }

            for (int i = 0; i < master.muscles.Length; ++i)
            {
                if (master.muscles[i].props.group == Muscle.Group.Foot)
                {
                    continue;
                }
                var acc = math.length((float3)master.muscles[i].rigidbody.velocity - lastVelArray[i]) / Time.fixedDeltaTime;
                lastVelArray[i] = master.muscles[i].rigidbody.velocity;
                if (acc < 0.05f)
                {
                    continue;
                }

                var pos = master.muscles[i].rigidbody.position;
                if (pos.Equals(float3.zero))
                {
                    continue;
                }
                float radius = 0.1f;

                if (master.muscles[i].props.group == Muscle.Group.Head) radius = 0.2f;
                else if (master.muscles[i].props.group == Muscle.Group.Hips) radius = 0.3f;
                else radius = 0.1f;

                physicsSingleton.OverlapSphere(pos, radius, ref outHits, detectFilter);

                foreach (var hit in outHits)
                {
                    float3 dir = math.normalizesafe(hit.Position - (float3)master.muscles[i].rigidbody.position);
                    var velocity = entityManager.GetComponentData<PhysicsVelocity>(hit.Entity);
                    var mass = entityManager.GetComponentData<PhysicsMass>(hit.Entity);
                    var length = outHits.Length;
                    if (length < 5) length = 5;
                    var forceValue = acc * master.muscles[i].rigidbody.mass * (1 - hit.Fraction) / length;
                    var force = dir * forceValue;
                    velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime);

                    entityManager.SetComponentData<PhysicsVelocity>(hit.Entity, velocity);
                }
            }

            // foreach (Muscle m in master.muscles)
            // {
            //     physicsSingleton.OverlapSphere(m.transform.position, 0.3f, ref outHits, detectFilter);

            //     foreach (var hit in outHits)
            //     {
            //         float3 dir = math.normalizesafe(hit.Position - (float3)m.transform.position);
            //         var velocity = entityManager.GetComponentData<PhysicsVelocity>(hit.Entity);
            //         var mass = entityManager.GetComponentData<PhysicsMass>(hit.Entity);
            //         var force = dir * 0.4f;
            //         velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime);

            //         entityManager.SetComponentData<PhysicsVelocity>(hit.Entity, velocity);
            //     }
            // }

            // if (!generateFlag)
            // {
            //     bool flag = false;
            //     var minFootPos = math.select(leftCenter, rightCenter, rightCenter.y > leftCenter);
            //     if (physicsSingleton.OverlapSphere(minFootPos, 2f, ref outHits, detectFilter))
            //     {
            //         foreach (var hit in outHits)
            //         {
            //             flag = !flag;
            //             if (flag) continue;

            //             var go = ObjectPool.instance.pool.Get();
            //             go.transform.position = hit.Position;
            //             poolList.Add(go.GetComponent<ReturnToPoolInTime>());
            //         }
            //         generateFlag = true;
            //     }
            // }
        }

        // 实现行人的抱头动画
        if (physicsSingleton.CheckBox(animator.GetBoneTransform(HumanBodyBones.Head).position, transform.rotation, boxSize, detectFilter))
        {
            animator.SetBool("HugHead", true);
        }
        else
        {
            animator.SetBool("HugHead", false);
        }

        outHits.Dispose();
    }

    /// <summary>
    /// Callback for setting up animation IK (inverse kinematics).
    /// </summary>
    /// <param name="layerIndex">Index of the layer on which the IK solver is called.</param>
    void OnAnimatorIK()
    {
        massCenterHeight = animator.bodyPosition.y;

        // 调节抱头动作中手部的位置
        if (animator.GetBool("HugHead"))
        {
            // var leftHandPos = animator.GetIKPosition(AvatarIKGoal.LeftHand);
            // var rightHandPos = animator.GetIKPosition(AvatarIKGoal.RightHand);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, headTransform.position + new Vector3(0, ik.deltaHeight, 0) - offset.x * transform.right + offset.y * transform.up + offset.z * transform.forward);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, headTransform.position + new Vector3(0, ik.deltaHeight, 0) + offset.x * transform.right + offset.y * transform.up + offset.z * transform.forward);
        }
    }

    // 行人摔倒时同步场景物理信息
    public void LostBalance()
    {
        aniSpeedBackup = animator.GetFloat("Velocity");
        this.animator.SetFloat("Velocity", 0);
        this.animator.SetBool("isBalance", false);
        this.animator.SetBool("HugHead", false);

        var physicsSingleton = physicsWorldQuery.GetSingleton<PhysicsWorldSingleton>();
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        bool flag = false;
        var minFootPos = math.select(leftCenter, rightCenter, rightCenter.y > leftCenter);
        if (physicsSingleton.OverlapSphere(minFootPos, 1.5f, ref outHits, detectFilter))
        {
            foreach (var hit in outHits)
            {
                flag = !flag;
                if (flag) continue;

                var go = ObjectPool.instance.pool.Get();
                go.transform.position = hit.Position;
                poolList.Add(go.GetComponent<ReturnToPoolInTime>());
            }
        }
        outHits.Dispose();

        // master.state = PuppetMaster.State.Frozen;
    }

    // 行人恢复站立后恢复摔倒前的动画速度
    public void GetUp()
    {
        this.animator.SetFloat("Velocity", aniSpeedBackup);
        generateFlag = false;

        foreach (var p in poolList)
        {
            p.flag = true;
        }
        poolList.Clear();
    }
}