using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct BasicSFMJob : IJobEntity
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float2 des;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public float standardVel;
    void Execute(Entity e, ref PhysicsVelocity velocity, in PhysicsMass mass, in LocalTransform localTransform, in AgentMovementData movementData)
    {
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapSphere(localTransform.Position, 1f, ref outHits, Constants.agentWallOnlyFilter);
        float2 interactionForce = 0;

        foreach (var hit in outHits)
        {
            if ((hit.Material.CustomTags & 0b_1100_0000) != 0)
            {
                if (hit.Entity.Equals(e)) continue;
                var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                interactionForce += 2000 * math.exp((0.25f - hit.Fraction) / 0.08f) * direction;
            }
        }
        var desireDir = math.normalizesafe(des - localTransform.Position.xz);
        velocity.Linear.xz += ((desireDir * standardVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass) * deltaTime;
        outHits.Dispose();
    }
}