using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct GlobalFlowFieldJob : IJobEntity
{
    [ReadOnly] public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;
    void Execute(ref PhysicsVelocity velocity, in LocalTransform localTransform, in AgentMovementData movementData)
    {
        // float2 globalGuidanceDir = float2.zero;

        // var res = FlowFieldUtility.Get4GridFlatIndexFromWorldPos(localTransform.Position, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        // foreach (var item in res)
        // {
        //     var delta = math.abs(cells[item].worldPos.x - localTransform.Position.x) + math.abs(cells[item].worldPos.z - localTransform.Position.z);
        //     globalGuidanceDir += (1 - delta) * cells[item].globalDir;
        // }
        // velocity.Linear.xz = math.normalizesafe(globalGuidanceDir / res.Length) * movementData.stdVel;
        velocity.Linear.xz = cells.GetPedestrainGlobalDir(localTransform.Position, settingData) * movementData.stdVel;
        // res.Dispose();
    }
}