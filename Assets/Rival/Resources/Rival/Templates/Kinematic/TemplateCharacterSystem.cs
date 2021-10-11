using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Rival;
using Unity.Collections.LowLevel.Unsafe;

namespace Rival.Templates.Kinematic //CODEGEN(Namespace)
{ //CODEGEN(NamespaceOpen)
    [DisableAutoCreation] //CODEGEN(RemoveLine)
    [UpdateInGroup(typeof(KinematicCharacterUpdateGroup))]
    public class TemplateCharacterSystem : SystemBase
    {
        public BuildPhysicsWorld BuildPhysicsWorldSystem;
        public EndFramePhysicsSystem EndFramePhysicsSystem;
        public EntityQuery CharacterQuery;

        [BurstCompile]
        public struct TemplateCharacterJob : IJobEntityBatchWithIndex
        {
            public float DeltaTime;
            [ReadOnly]
            public CollisionWorld CollisionWorld;

            [ReadOnly]
            public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityFromEntity;
            [ReadOnly]
            public ComponentDataFromEntity<PhysicsMass> PhysicsMassFromEntity;
            [NativeDisableParallelForRestriction] // we need to write to our own characterBody, but we might also need to read from other characterBodies during the update (for dynamics handling)
            public ComponentDataFromEntity<KinematicCharacterBody> CharacterBodyFromEntity;
            [ReadOnly]
            public ComponentDataFromEntity<TrackedTransform> TrackedTransformFromEntity;

            [ReadOnly]
            public EntityTypeHandle EntityType;
            public ComponentTypeHandle<Translation> TranslationType;
            public ComponentTypeHandle<Rotation> RotationType;
            public ComponentTypeHandle<PhysicsCollider> PhysicsColliderType;
            public BufferTypeHandle<KinematicCharacterHit> CharacterHitsBufferType;
            public BufferTypeHandle<KinematicVelocityProjectionHit> VelocityProjectionHitsBufferType;
            public BufferTypeHandle<KinematicCharacterDeferredImpulse> CharacterDeferredImpulsesBufferType;
            public BufferTypeHandle<StatefulKinematicCharacterHit> StatefulCharacterHitsBufferType;

            public ComponentTypeHandle<TemplateCharacterComponent> TemplateCharacterType;
            public ComponentTypeHandle<TemplateCharacterInputs> TemplateCharacterInputsType;

            [NativeDisableContainerSafetyRestriction]
            public NativeList<int> TmpRigidbodyIndexesProcessed;
            [NativeDisableContainerSafetyRestriction]
            public NativeList<Unity.Physics.RaycastHit> TmpRaycastHits;
            [NativeDisableContainerSafetyRestriction]
            public NativeList<ColliderCastHit> TmpColliderCastHits;
            [NativeDisableContainerSafetyRestriction]
            public NativeList<DistanceHit> TmpDistanceHits;

            public void Execute(ArchetypeChunk chunk, int batchIndex, int indexOfFirstEntityInQuery)
            {
                NativeArray<Entity> chunkEntities = chunk.GetNativeArray(EntityType);
                NativeArray<Translation> chunkTranslations = chunk.GetNativeArray(TranslationType);
                NativeArray<Rotation> chunkRotations = chunk.GetNativeArray(RotationType);
                NativeArray<PhysicsCollider> chunkPhysicsColliders = chunk.GetNativeArray(PhysicsColliderType);
                BufferAccessor<KinematicCharacterHit> chunkCharacterHitBuffers = chunk.GetBufferAccessor(CharacterHitsBufferType);
                BufferAccessor<KinematicVelocityProjectionHit> chunkVelocityProjectionHitBuffers = chunk.GetBufferAccessor(VelocityProjectionHitsBufferType);
                BufferAccessor<KinematicCharacterDeferredImpulse> chunkCharacterDeferredImpulsesBuffers = chunk.GetBufferAccessor(CharacterDeferredImpulsesBufferType);
                BufferAccessor<StatefulKinematicCharacterHit> chunkStatefulCharacterHitsBuffers = chunk.GetBufferAccessor(StatefulCharacterHitsBufferType);
                NativeArray<TemplateCharacterComponent> chunkTemplateCharacters = chunk.GetNativeArray(TemplateCharacterType);
                NativeArray<TemplateCharacterInputs> chunkTemplateCharacterInputs = chunk.GetNativeArray(TemplateCharacterInputsType);

                // Initialize the Temp collections
                if (!TmpRigidbodyIndexesProcessed.IsCreated)
                {
                   TmpRigidbodyIndexesProcessed = new NativeList<int>(24, Allocator.Temp);
                }
                if (!TmpRaycastHits.IsCreated)
                {
                    TmpRaycastHits = new NativeList<Unity.Physics.RaycastHit>(24, Allocator.Temp);
                }
                if (!TmpColliderCastHits.IsCreated)
                {
                    TmpColliderCastHits = new NativeList<ColliderCastHit>(24, Allocator.Temp);
                }
                if (!TmpDistanceHits.IsCreated)
                {
                    TmpDistanceHits = new NativeList<DistanceHit>(24, Allocator.Temp);
                }

                // Assign the global data of the processor
                TemplateCharacterProcessor processor = default;
                processor.DeltaTime = DeltaTime;
                processor.CollisionWorld = CollisionWorld;
                processor.CharacterBodyFromEntity = CharacterBodyFromEntity;
                processor.PhysicsMassFromEntity = PhysicsMassFromEntity;
                processor.PhysicsVelocityFromEntity = PhysicsVelocityFromEntity;
                processor.TrackedTransformFromEntity = TrackedTransformFromEntity;
                processor.TmpRigidbodyIndexesProcessed = TmpRigidbodyIndexesProcessed;
                processor.TmpRaycastHits = TmpRaycastHits;
                processor.TmpColliderCastHits = TmpColliderCastHits;
                processor.TmpDistanceHits = TmpDistanceHits;

                // Iterate on individual characters
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = chunkEntities[i];

                    // Assign the per-character data of the processor
                    processor.Entity = entity;
                    processor.Translation = chunkTranslations[i].Value;
                    processor.Rotation = chunkRotations[i].Value;
                    processor.PhysicsCollider = chunkPhysicsColliders[i];
                    processor.CharacterBody = CharacterBodyFromEntity[entity];
                    processor.CharacterHitsBuffer = chunkCharacterHitBuffers[i];
                    processor.CharacterDeferredImpulsesBuffer = chunkCharacterDeferredImpulsesBuffers[i];
                    processor.VelocityProjectionHitsBuffer = chunkVelocityProjectionHitBuffers[i];
                    processor.StatefulCharacterHitsBuffer = chunkStatefulCharacterHitsBuffers[i];
                    processor.TemplateCharacter = chunkTemplateCharacters[i];
                    processor.TemplateCharacterInputs = chunkTemplateCharacterInputs[i];

                    // Update character
                    processor.OnUpdate();

                    // Write back updated data
                    // The core character update loop only writes to Translation, Rotation, KinematicCharacterBody, and the various character DynamicBuffers. 
                    // You must remember to write back any extra data you modify in your own code
                    chunkTranslations[i] = new Translation { Value = processor.Translation };
                    chunkRotations[i] = new Rotation { Value = processor.Rotation };
                    CharacterBodyFromEntity[entity] = processor.CharacterBody;
                    chunkPhysicsColliders[i] = processor.PhysicsCollider; // safe to remove if not needed. This would be needed if you resize the character collider, for example
                    chunkTemplateCharacters[i] = processor.TemplateCharacter; // safe to remove if not needed. This would be needed if you changed data in your own character component
                    chunkTemplateCharacterInputs[i] = processor.TemplateCharacterInputs; // safe to remove if not needed. This would be needed if you changed data in your own character component
                }
            }
        }

        protected override void OnCreate()
        {
            BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            EndFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();

            CharacterQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = MiscUtilities.CombineArrays(
                    KinematicCharacterUtilities.GetCoreCharacterComponentTypes(),
                    new ComponentType[]
                    {
                        typeof(TemplateCharacterComponent),
                        typeof(TemplateCharacterInputs),
                    }),
            });

            RequireForUpdate(CharacterQuery);
        }

        protected unsafe override void OnUpdate()
        {
            Dependency = JobHandle.CombineDependencies(EndFramePhysicsSystem.GetOutputDependency(), Dependency);

            Dependency = new TemplateCharacterJob
            {
                DeltaTime = Time.DeltaTime,
                CollisionWorld = BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,

                PhysicsVelocityFromEntity = GetComponentDataFromEntity<PhysicsVelocity>(true),
                PhysicsMassFromEntity = GetComponentDataFromEntity<PhysicsMass>(true),
                CharacterBodyFromEntity = GetComponentDataFromEntity<KinematicCharacterBody>(false),
                TrackedTransformFromEntity = GetComponentDataFromEntity<TrackedTransform>(true),

                EntityType = GetEntityTypeHandle(),
                TranslationType = GetComponentTypeHandle<Translation>(false),
                RotationType = GetComponentTypeHandle<Rotation>(false),
                PhysicsColliderType = GetComponentTypeHandle<PhysicsCollider>(false),
                CharacterHitsBufferType = GetBufferTypeHandle<KinematicCharacterHit>(false),
                VelocityProjectionHitsBufferType = GetBufferTypeHandle<KinematicVelocityProjectionHit>(false),
                CharacterDeferredImpulsesBufferType = GetBufferTypeHandle<KinematicCharacterDeferredImpulse>(false),
                StatefulCharacterHitsBufferType = GetBufferTypeHandle<StatefulKinematicCharacterHit>(false),

                TemplateCharacterType = GetComponentTypeHandle<TemplateCharacterComponent>(false),
                TemplateCharacterInputsType = GetComponentTypeHandle<TemplateCharacterInputs>(false),
            }.ScheduleParallel(CharacterQuery, 1, Dependency);

            Dependency = KinematicCharacterUtilities.ScheduleDeferredImpulsesJob(this, CharacterQuery, Dependency);

            BuildPhysicsWorldSystem.AddInputDependencyToComplete(Dependency);
        }
    }
} //CODEGEN(NamespaceClose)
