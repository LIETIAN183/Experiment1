using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct BasicSFM_LocalFFJob : IJobEntity
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float2 des;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;
    void Execute(Entity e, ref PhysicsVelocity velocity, in PhysicsMass mass, in LocalTransform localTransform, in AgentMovementData movementData)
    {
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapSphere(localTransform.Position, 1, ref outHits, Constants.agentWallOnlyFilter);
        float2 interactionForce = 0;

        foreach (var hit in outHits)
        {
            if ((hit.Material.CustomTags & 0b_1100_0000) != 0)
            {
                if (hit.Entity.Equals(e)) continue;
                var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                interactionForce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
            }
        }

        // 行人局部指导方向
        // float2 localGuidanceDir = float2.zero;
        // var res = FlowFieldUtility.Get4GridFlatIndexFromWorldPos(localTransform.Position, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        // foreach (var item in res)
        // {
        //     var delta = math.abs(cells[item].worldPos.x - localTransform.Position.x) + math.abs(cells[item].worldPos.z - localTransform.Position.z);
        //     localGuidanceDir += (1 - delta) * cells[item].localDir;
        // }
        // desireDir = math.normalizesafe(math.normalizesafe(desireDir) + 0.75f * math.normalizesafe(localGuidanceDir / res.Length));
        var desireDir = math.normalizesafe(math.normalizesafe(des - localTransform.Position.xz) + 0.75f * cells.GetPedestrainLocalDir(localTransform.Position, settingData));
        //-------------------------

        velocity.Linear.xz += ((desireDir * movementData.stdVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass) * deltaTime;
        outHits.Dispose();
    }
}