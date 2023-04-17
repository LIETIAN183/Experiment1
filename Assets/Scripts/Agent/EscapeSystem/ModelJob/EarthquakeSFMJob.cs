using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct EarthquakeSFMJob : IJobEntity
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float2 des;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public TimerData accData;
    [ReadOnly] public float standardVel;
    void Execute(Entity e, ref PhysicsVelocity velocity, in PhysicsMass mass, in LocalTransform localTransform, in AgentMovementData movementData)
    {
        // 计算附近的障碍物与智能体
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapSphere(localTransform.Position, 1, ref outHits, CollisionFilter.Default);
        float2 interactionForce = 0;
        foreach (var hit in outHits)
        {
            if ((hit.Material.CustomTags & 0b_0100_0000) != 0)
            {
                var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                interactionForce += 20 * math.exp((0.25f - hit.Fraction) / 0.3f) * direction;
            }
            else if (((hit.Material.CustomTags & 0b_1000_0000) != 0))
            {
                if (hit.Entity.Equals(e)) continue;
                var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                interactionForce += 20 * math.exp((0.25f - hit.Fraction) / 0.5f) * direction;
            }
        }

        var SFMDirection = math.normalizesafe(des - localTransform.Position.xz);
        var acc = (SFMDirection * standardVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass;
        var flag1 = math.abs(acc.x) > math.abs(accData.curAcc.x);
        var flag2 = math.abs(acc.y) > math.abs(accData.curAcc.y);
        if (flag1 && flag2)
        {
            velocity.Linear.xz += ((SFMDirection * standardVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass) * deltaTime;
        }
        else if (!flag1 && !flag2)
        {
            velocity.Linear.xz = 0;
        }
        else if (!flag1 && flag2)
        {
            velocity.Linear.x = 0;
            velocity.Linear.z += ((SFMDirection * standardVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass).y * deltaTime;
        }
        else
        {
            velocity.Linear.z = 0;
            velocity.Linear.x += ((SFMDirection * standardVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass).x * deltaTime;
        }

        outHits.Dispose();
    }
}