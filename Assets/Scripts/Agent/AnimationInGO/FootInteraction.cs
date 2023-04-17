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

[RequireComponent(typeof(Animator))]
public class FootInteraction : MonoBehaviour
{
    private Animator animator;

    private csHomebrewIK ik;

    private float3 leftFootPos, rightFootPos, leftToes, rightToes, leftCenter, rightCenter;

    private EntityManager entityManager;

    private EntityQuery physicsWorldQuery;

    private EntityQuery physicsVelocityQuery;

    [SerializeField]
    private PhysicsCategoryTags detectLayers = PhysicsCategoryTags.Everything;
    private CollisionFilter detectFilter = CollisionFilter.Default;

    [SerializeField]
    private float footOverlapSphereRadius = 0.15f;


    public bool debug = false;

    public Entity entity;


    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        ik = this.GetComponent<csHomebrewIK>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        physicsWorldQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        physicsVelocityQuery = entityManager.CreateEntityQuery(typeof(PhysicsVelocity));
        detectFilter.CollidesWith = detectLayers.Value;
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
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        leftFootPos = ik.leftFootTransform.position;
        rightFootPos = ik.rightFootTransform.position;
        leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes).position;
        rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes).position;
        leftCenter = (leftFootPos + leftToes) / 2;
        rightCenter = (rightFootPos + rightToes) / 2;

        var physicsSingleton = physicsWorldQuery.GetSingleton<PhysicsWorldSingleton>();
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);

        if (entity != null)
        {
            var sumforce = entityManager.GetComponentData<AgentMovementData>(entity).forceForFootInteraction;

            // 左脚
            physicsSingleton.OverlapSphere(leftCenter, footOverlapSphereRadius, ref outHits, detectFilter);
            foreach (var i in outHits)
            {
                float3 dir = math.normalizesafe(i.Position - rightCenter);
                var velocity = entityManager.GetComponentData<PhysicsVelocity>(i.Entity);
                var mass = entityManager.GetComponentData<PhysicsMass>(i.Entity);
                var force = math.dot(sumforce / (2 * outHits.Length), dir) * dir;
                if (outHits.Length < 5)
                {
                    velocity.ApplyLinearImpulse(mass, force * (Time.fixedDeltaTime / 5) / 5);
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
                var force = math.dot(sumforce / (2 * outHits.Length), dir) * dir;
                if (outHits.Length < 5)
                {
                    velocity.ApplyLinearImpulse(mass, force * (Time.fixedDeltaTime / 5) / 5);
                }
                else
                {
                    velocity.ApplyLinearImpulse(mass, force * Time.fixedDeltaTime);
                }
                entityManager.SetComponentData<PhysicsVelocity>(i.Entity, velocity);
            }
        }
        outHits.Dispose();
    }

    /// <summary>
    /// Callback for setting up animation IK (inverse kinematics).
    /// </summary>
    /// <param name="layerIndex">Index of the layer on which the IK solver is called.</param>
    void OnAnimatorIK() { }
}
