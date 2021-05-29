using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

/// <summary>
/// Base controller for character movement.
/// Is not physics-based, but uses physics to check for collisions.
/// </summary>
// [UpdateInGroup(typeof(SimulationSystemGroup)), UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
[DisableAutoCreation]
public sealed class Backup : SystemBase
{
    private const float Epsilon = 0.001f;

    private BuildPhysicsWorld buildPhysicsWorld;
    private ExportPhysicsWorld exportPhysicsWorld;
    private EndFramePhysicsSystem endFramePhysicsSystem;
    [ReadOnly] ComponentDataFromEntity<PhysicsCollider> ColliderData;




    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        exportPhysicsWorld = World.GetOrCreateSystem<ExportPhysicsWorld>();
        endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();

    }

    protected override void OnUpdate()
    {
        float DeltaTime = Time.DeltaTime;
        float3 gravityNormolize = new float3(0, -1, 0);//math.normalize(characterControllerData.gravity)
        ColliderData = GetComponentDataFromEntity<PhysicsCollider>(true);
        var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        Entities
        .WithAll<PlayerTag>().WithoutBurst()
        .ForEach((Entity entity, ref CharacterControllerData characterControllerData, ref Translation translation, ref Rotation rotation, ref PhysicsCollider physicsCollider) =>
        {
            // UnityEngine.Debug.Log(characterControllerData.gravity);
            float3 epsilon = new float3(0.0f, Epsilon, 0.0f) * -gravityNormolize;
            float3 currPos = translation.Value + epsilon;
            quaternion currRot = rotation.Value;

            float3 gravityVelocity = characterControllerData.gravity * DeltaTime;
            float3 verticalVelocity = (characterControllerData.verticalVelocity + gravityVelocity);
            float3 horizontalVelocity = (characterControllerData.currentDirection * characterControllerData.currentMagnitude * characterControllerData.speed * DeltaTime);

            if (characterControllerData.isGrounded)
            {
                if (characterControllerData.jump)
                {
                    verticalVelocity = characterControllerData.jumpStrength * -gravityNormolize;
                }
                else
                {
                    float3 gravityDir = math.normalize(gravityVelocity);
                    float3 verticalDir = math.normalize(verticalVelocity);

                    if (MathUtils.FloatEquals(math.dot(gravityDir, verticalDir), 1.0f))
                    {
                        verticalVelocity = new float3();
                    }
                }
            }


            HandleHorizontalMovement(ref horizontalVelocity, ref entity, ref currPos, ref currRot, ref characterControllerData, ref physicsCollider, ref collisionWorld);
            currPos += horizontalVelocity;

            HandleVerticalMovement(ref verticalVelocity, ref entity, ref currPos, ref currRot, ref characterControllerData, ref physicsCollider, ref collisionWorld, DeltaTime);
            currPos += verticalVelocity;

            CorrectForCollision(ref entity, ref currPos, ref currRot, ref characterControllerData, ref physicsCollider, ref collisionWorld);
            DetermineIfGrounded(entity, ref currPos, ref epsilon, ref characterControllerData, ref physicsCollider, ref collisionWorld);

            translation.Value = currPos - epsilon;
        }).Run();
    }

    /// <summary>
    /// Performs a collision correction at the specified position.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="currPos"></param>
    /// <param name="currRot"></param>
    /// <param name="controller"></param>
    /// <param name="collider"></param>
    /// <param name="collisionWorld"></param>
    private void CorrectForCollision(ref Entity entity, ref float3 currPos, ref quaternion currRot, ref CharacterControllerData controller, ref PhysicsCollider collider, ref CollisionWorld collisionWorld)
    {
        RigidTransform transform = new RigidTransform()
        {
            pos = currPos,
            rot = currRot
        };

        // Use a subset sphere within our collider to test against.
        // We do not use the collider itself as some intersection (such as on ramps) is ok.

        var offset = -math.normalize(controller.gravity) * 0.1f;
        var sampleCollider = new PhysicsCollider()
        {
            Value = SphereCollider.Create(new SphereGeometry()
            {
                Center = currPos + offset,
                Radius = 0.1f
            })
        };

        if (PhysicsUtils.ColliderDistance(out DistanceHit smallestHit, sampleCollider, 0.1f, transform, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, null, ColliderData, Allocator.Temp))
        {
            if (smallestHit.Distance < 0.0f)
            {
                currPos += math.abs(smallestHit.Distance) * smallestHit.SurfaceNormal;
            }
        }
    }

    /// <summary>
    /// Handles horizontal movement on the XZ plane.
    /// </summary>
    /// <param name="horizontalVelocity"></param>
    /// <param name="entity"></param>
    /// <param name="currPos"></param>
    /// <param name="currRot"></param>
    /// <param name="controller"></param>
    /// <param name="collider"></param>
    /// <param name="collisionWorld"></param>
    private void HandleHorizontalMovement(
        ref float3 horizontalVelocity,
        ref Entity entity,
        ref float3 currPos,
        ref quaternion currRot,
        ref CharacterControllerData controller,
        ref PhysicsCollider collider,
        ref CollisionWorld collisionWorld)
    {
        if (MathUtils.IsZero(horizontalVelocity))
        {
            return;
        }

        float3 targetPos = currPos + horizontalVelocity;

        NativeList<ColliderCastHit> horizontalCollisions = PhysicsUtils.ColliderCastAll(collider, currPos, targetPos, ref collisionWorld, entity, Allocator.Temp);
        PhysicsUtils.TrimByFilter(ref horizontalCollisions, ColliderData, PhysicsCollisionFilters.DynamicWithPhysical);

        if (horizontalCollisions.Length > 0)
        {
            // We either have to step or slide as something is in our way.
            float3 step = new float3(0.0f, controller.maxStep, 0.0f);
            PhysicsUtils.ColliderCast(out ColliderCastHit nearestStepHit, collider, targetPos + step, targetPos, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, null, ColliderData, Allocator.Temp);

            if (!MathUtils.IsZero(nearestStepHit.Fraction))
            {
                // We can step up.
                targetPos += (step * (1.0f - nearestStepHit.Fraction));
                horizontalVelocity = targetPos - currPos;
            }
            else
            {
                // We can not step up, so slide.
                NativeList<DistanceHit> horizontalDistances = PhysicsUtils.ColliderDistanceAll(collider, 1.0f, new RigidTransform() { pos = currPos + horizontalVelocity, rot = currRot }, ref collisionWorld, entity, Allocator.Temp);
                PhysicsUtils.TrimByFilter(ref horizontalDistances, ColliderData, PhysicsCollisionFilters.DynamicWithPhysical);

                for (int i = 0; i < horizontalDistances.Length; ++i)
                {
                    if (horizontalDistances[i].Distance >= 0.0f)
                    {
                        continue;
                    }

                    horizontalVelocity += (horizontalDistances[i].SurfaceNormal * -horizontalDistances[i].Distance);
                }

                horizontalDistances.Dispose();
            }
        }

        horizontalCollisions.Dispose();
    }

    /// <summary>
    /// Handles vertical movement from gravity and jumping.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="currPos"></param>
    /// <param name="currRot"></param>
    /// <param name="controller"></param>
    /// <param name="collider"></param>
    /// <param name="collisionWorld"></param>
    private void HandleVerticalMovement(
        ref float3 verticalVelocity,
        ref Entity entity,
        ref float3 currPos,
        ref quaternion currRot,
        ref CharacterControllerData controller,
        ref PhysicsCollider collider,
        ref CollisionWorld collisionWorld, float DeltaTime)
    {
        controller.verticalVelocity = verticalVelocity;

        if (MathUtils.IsZero(verticalVelocity))
        {
            return;
        }

        verticalVelocity *= DeltaTime;

        NativeList<ColliderCastHit> verticalCollisions = PhysicsUtils.ColliderCastAll(collider, currPos, currPos + verticalVelocity, ref collisionWorld, entity, Allocator.Temp);
        PhysicsUtils.TrimByFilter(ref verticalCollisions, ColliderData, PhysicsCollisionFilters.DynamicWithPhysical);

        if (verticalCollisions.Length > 0)
        {
            RigidTransform transform = new RigidTransform()
            {
                pos = currPos + verticalVelocity,
                rot = currRot
            };

            if (PhysicsUtils.ColliderDistance(out DistanceHit verticalPenetration, collider, 1.0f, transform, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, null, ColliderData, Allocator.Temp))
            {
                if (verticalPenetration.Distance < -0.01f)
                {
                    verticalVelocity += (verticalPenetration.SurfaceNormal * verticalPenetration.Distance);

                    if (PhysicsUtils.ColliderCast(out ColliderCastHit adjustedHit, collider, currPos, currPos + verticalVelocity, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, null, ColliderData, Allocator.Temp))
                    {
                        verticalVelocity *= adjustedHit.Fraction;
                    }
                }
            }
        }

        verticalVelocity = MathUtils.ZeroOut(verticalVelocity, 0.0001f);
        verticalCollisions.Dispose();
    }

    /// <summary>
    /// Determines if the character is resting on a surface.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="currPos"></param>
    /// <param name="epsilon"></param>
    /// <param name="collider"></param>
    /// <param name="collisionWorld"></param>
    /// <returns></returns>
    private unsafe static void DetermineIfGrounded(Entity entity, ref float3 currPos, ref float3 epsilon, ref CharacterControllerData controller, ref PhysicsCollider collider, ref CollisionWorld collisionWorld)
    {
        var aabb = collider.ColliderPtr->CalculateAabb();
        float mod = 0.15f;

        float3 samplePos = currPos + new float3(0.0f, aabb.Min.y, 0.0f);
        float3 gravity = math.normalize(controller.gravity);
        float3 offset = (gravity * 0.1f);

        float3 posLeft = samplePos - new float3(aabb.Extents.x * mod, 0.0f, 0.0f);
        float3 posRight = samplePos + new float3(aabb.Extents.x * mod, 0.0f, 0.0f);
        float3 posForward = samplePos + new float3(0.0f, 0.0f, aabb.Extents.z * mod);
        float3 posBackward = samplePos - new float3(0.0f, 0.0f, aabb.Extents.z * mod);

        controller.isGrounded = PhysicsUtils.Raycast(out RaycastHit centerHit, samplePos, samplePos + offset, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, Allocator.Temp) ||
                                PhysicsUtils.Raycast(out RaycastHit leftHit, posLeft, posLeft + offset, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, Allocator.Temp) ||
                                PhysicsUtils.Raycast(out RaycastHit rightHit, posRight, posRight + offset, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, Allocator.Temp) ||
                                PhysicsUtils.Raycast(out RaycastHit forwardHit, posForward, posForward + offset, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, Allocator.Temp) ||
                                PhysicsUtils.Raycast(out RaycastHit backwardHit, posBackward, posBackward + offset, ref collisionWorld, entity, PhysicsCollisionFilters.DynamicWithPhysical, Allocator.Temp);
    }
}

// using Unity.Entities;
// using Unity.Mathematics;

// [GenerateAuthoringComponent]
// public struct CharacterControllerData : IComponentData
// {
//     // -------------------------------------------------------------------------------------
//     // Current Movement
//     // -------------------------------------------------------------------------------------

//     /// <summary>
//     /// The current direction that the character is moving.
//     /// </summary>
//     public float3 currentDirection;

//     /// <summary>
//     /// The current magnitude of the character movement.
//     /// If <c>0.0</c>, then the character is not being directly moved by the controller but residual forces may still be active.
//     /// 速度倍数
//     /// </summary>
//     public float currentMagnitude;

//     /// <summary>
//     /// Is the character requesting to jump?
//     /// Used in conjunction with <see cref="IsGrounded"/> to determine if the <see cref="JumpStrength"/> should be used to make the entity jump.
//     /// </summary>
//     public bool jump;

//     // -------------------------------------------------------------------------------------
//     // Control Properties
//     // -------------------------------------------------------------------------------------

//     /// <summary>
//     /// Gravity force applied to the character.
//     /// </summary>
//     public float3 gravity;// new float3(0,-9.81f,0)

//     /// <summary>
//     /// The maximum speed at which this character moves.
//     /// </summary>
//     public float maxSpeed;//7.5

//     /// <summary>
//     /// The current speed at which the player moves.
//     /// </summary>
//     public float speed;//5

//     /// <summary>
//     /// The jump strength which controls how high a jump is, in conjunction with <see cref="Gravity"/>.
//     /// </summary>
//     public float jumpStrength;//9

//     /// <summary>
//     /// The maximum height the character can step up, in world units.
//     /// </summary>
//     public float maxStep;//0.35

//     /// <summary>
//     /// Drag value applied to reduce the <see cref="VerticalVelocity"/>.
//     /// </summary>
//     public float drag;//0.2

//     // -------------------------------------------------------------------------------------
//     // Control State
//     // -------------------------------------------------------------------------------------

//     /// <summary>
//     /// True if the character is on the ground.
//     /// </summary>
//     public bool isGrounded;

//     /// <summary>
//     /// The current horizontal velocity of the character.
//     /// </summary>
//     public float3 horizontalVelocity;

//     /// <summary>
//     /// The current jump velocity of the character.
//     /// </summary>
//     public float3 verticalVelocity;
// }