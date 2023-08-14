using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Drawing;

// 本文提出的地震人群疏散模型
[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct OurModelJob : IJobEntity
{
    [ReadOnly] public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<int> dests;
    [ReadOnly] public FlowFieldSettingData settingData;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public TimerData accData;
    [ReadOnly] public float standardVel;
    [ReadOnly] public ComponentLookup<AgentMovementData> agentDataList;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformList;
    public EntityCommandBuffer.ParallelWriter parallelECB;

    [ReadOnly] public uint randomInitSeed;


    // public CommandBuilder builder;
    void Execute(Entity entity, [EntityIndexInQuery] int index, ref PhysicsVelocity velocity, in PhysicsMass mass)
    {
        // velocity.Linear = new float3(0, 0, 3);
        // return;
        var random = Random.CreateFromIndex(randomInitSeed + (uint)index);
        // builder.PushLineWidth(3f);
        var curEntityLocalTransform = localTransformList[entity];
        var curEntityMovementData = agentDataList[entity];

        // 行人站立后需要一定的恢复时间
        if (curEntityMovementData.recoverTimer > 0f)
        {
            curEntityMovementData.recoverTimer -= deltaTime;
            curEntityMovementData.fallTimer = 2f;
            parallelECB.SetComponent<AgentMovementData>(index + 300, entity, curEntityMovementData);
            return;
        }

        // 若行人摔倒，fallTimer 小于 0 后执行站立行为
        if (curEntityMovementData.isFall)
        {
            curEntityMovementData.fallTimer -= deltaTime;
            velocity.Linear.xz = 0;
            if (curEntityMovementData.fallTimer < 0)
            {
                curEntityMovementData.isFall = false;
                curEntityMovementData.recoverTimer = 2f;
            }
            parallelECB.SetComponent<AgentMovementData>(index + 300, entity, curEntityMovementData);
            return;
        }

        float2 globalGuideDir = cells.GetPedestrainGlobalDir(curEntityLocalTransform.Position, settingData);

        if (curEntityMovementData.SeeExit == false)
        {
            var localIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(curEntityLocalTransform.Position.xz, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
            if (cells[localIndex].seeExit)
            {
                curEntityMovementData.SeeExit = true;
            }
        }

        float2 interactionForce = 0;
        // 计算和其他智能体的交互力
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapSphere(curEntityLocalTransform.Position, 1, ref outHits, Constants.agentOnlyFilter);
        foreach (var hit in outHits)
        {
            // 和行人之间的交互
            if ((hit.Material.CustomTags & 0b_1000_0000) != 0)
            {
                if (hit.Entity.Equals(entity)) continue;
                var direction = math.normalizesafe(curEntityLocalTransform.Position.xz - hit.Position.xz);
                interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f - accData.curPGA * Constants.gravity) * direction;
            }
        }

        // 计算期望方向
        float2 desireDir = float2.zero;
        if (!curEntityMovementData.SeeExit && curEntityMovementData.familiarity < 1)
        {
            // 看不见出口，则需要计算局部跟随方向
            var LocalGuideDir = cells.GetPedestrainLocalDir(curEntityLocalTransform.Position, settingData);
            var followNeighborDir = float2.zero;
            // 计算从周围行人获得的跟随方向
            foreach (var hit in outHits)
            {
                if (hit.Entity.Equals(entity)) continue;
                var neighborFamiliarity = agentDataList[hit.Entity].familiarity;
                if (neighborFamiliarity > curEntityMovementData.familiarity)
                {
                    // followNeighborDir += math.normalizesafe(localTransformList[hit.Entity].Position.xz - curEntityLocalTransform.Position.xz) * neighborFamiliarity;
                    followNeighborDir += math.normalizesafe(localTransformList[hit.Entity].Forward().xz) * neighborFamiliarity;
                }
            }
            if (!followNeighborDir.Equals(float2.zero))
            {
                desireDir = math.normalizesafe(curEntityMovementData.familiarity * globalGuideDir + (1 - curEntityMovementData.familiarity) * (math.normalizesafe(followNeighborDir) + Constants.c_local * LocalGuideDir));
            }
            else
            {
                float2 selfDir = float2.zero;
                // 看不见出口，且周围无行人可以跟随,则需要计算自身随机方向
                RaycastInput ray = new RaycastInput
                {
                    Start = curEntityLocalTransform.Position,
                    Filter = Constants.WallOnlyFilter
                };

                // if (curEntityMovementData.lastSelfDir.Equals(float2.zero))
                // {
                //     curEntityMovementData.lastSelfDir = new float2(1, 0);
                // }
                // ray.End = ray.Start + curEntityMovementData.lastSelfDir.ToFloat3() * 5f;
                // var flag = physicsWorld.CastRay(ray, out var closestHit);
                // if (!curEntityMovementData.lastSelfDir.Equals(float2.zero) && (!flag || closestHit.Fraction > 0.5f))
                // // if ((!flag || closestHit.Fraction > 0.5f))
                // {
                //     selfDir = curEntityMovementData.lastSelfDir;
                //     UnityEngine.Debug.Log("1");
                // }
                // else
                // {
                // dirList 中存储没有发生碰撞的方向
                float maxDistance = 0;
                float3 corrDir = float3.zero;
                float rayLength = 1f;
                // for (int i = -60; i <= 60; i += 10)
                for (int j = 0; j < 13; j++)
                {
                    int i = Constants.dirIterOrder[j];
                    // 相同夹角随机检测
                    var dir = math.normalizesafe(math.mul(quaternion.AxisAngle(math.up(), math.radians(i)), curEntityLocalTransform.Forward()));
                    ray.End = ray.Start + dir * rayLength;
                    var flag = physicsWorld.CastRay(ray, out var closestHit);
                    if (!flag)
                    {
                        maxDistance = rayLength;
                        corrDir = dir;
                        break;
                    }
                    var dis = closestHit.Fraction * rayLength;
                    if (dis > 0f)
                    {
                        maxDistance = dis;
                        corrDir = dir;
                        break;
                    }
                    else if (dis > maxDistance)
                    {
                        maxDistance = dis;
                        corrDir = dir;
                    }
                    // }
                    // else
                    // {
                    //     if (5 > maxDistance)
                    //     {
                    //         maxDistance = 5f;
                    //         corrDir = dir;
                    //     }
                    // }
                }
                if (maxDistance <= 0)
                {
                    // UnityEngine.Debug.Log(maxDistance);
                    selfDir = math.normalizesafe(math.mul(quaternion.AxisAngle(math.up(), math.radians(30)), curEntityLocalTransform.Forward()).xz);
                    // curEntityMovementData.lastSelfDir = selfDir;
                    // UnityEngine.Debug.Log("2");
                    // builder.Arrow(curEntityLocalTransform.Position, curEntityLocalTransform.Position + selfDir.ToFloat3(), UnityEngine.Color.blue);
                }
                else
                {// 否则找到最大可视区域，进行前进
                    selfDir = corrDir.xz;
                    // curEntityMovementData.lastSelfDir = selfDir;
                    // UnityEngine.Debug.Log("3");
                    // builder.Arrow(curEntityLocalTransform.Position, curEntityLocalTransform.Position + corrDir * maxDistance, UnityEngine.Color.black);
                }
                // }

                // builder.Arrow(curEntityLocalTransform.Position, curEntityLocalTransform.Position + math.normalizesafe(math.mul(quaternion.AxisAngle(math.up(), math.radians(-60)), curEntityLocalTransform.Forward())), UnityEngine.Color.yellow);
                // builder.Arrow(curEntityLocalTransform.Position, curEntityLocalTransform.Position + math.normalizesafe(math.mul(quaternion.AxisAngle(math.up(), math.radians(60)), curEntityLocalTransform.Forward())), UnityEngine.Color.yellow);
                // UnityEngine.Debug.Log(dirList.IsEmpty);
                // builder.Arrow(curEntityLocalTransform.Position, curEntityLocalTransform.Position + curEntityLocalTransform.Forward(), UnityEngine.Color.green);

                desireDir = math.normalizesafe(curEntityMovementData.familiarity * globalGuideDir + (1 - curEntityMovementData.familiarity) * (math.normalizesafe(selfDir) + Constants.c_local * LocalGuideDir));
                // builder.Arrow(curEntityLocalTransform.Position, curEntityLocalTransform.Position + desireDir.ToFloat3(), UnityEngine.Color.red);
            }
        }
        else
        {
            // 看见出口，直接跟随全局指导
            desireDir = globalGuideDir;
        }
        outHits.Dispose();

        // 设置人物方向
        var targetRotation = quaternion.LookRotationSafe(new float3(desireDir.x, 0, desireDir.y), math.up());
        curEntityLocalTransform.Rotation = UnityEngine.Quaternion.Slerp(curEntityLocalTransform.Rotation, targetRotation, 5f * deltaTime);
        // curEntityLocalTransform.Rotation = targetRotation;
        parallelECB.SetComponent<LocalTransform>(index, entity, curEntityLocalTransform);

        // 计算期望速度
        curEntityMovementData.desireSpeed = math.exp(-curEntityMovementData.deltaHeight - math.length(accData.curAcc)) * standardVel;
        curEntityMovementData.curSpeed = math.length(velocity.Linear.xz);

        // 计算加速度
        float2 curAcc = ((desireDir * curEntityMovementData.desireSpeed - velocity.Linear.xz) / 0.5f - accData.curAcc.xz + interactionForce * mass.InverseMass);

        // 计算脚部受力
        curEntityMovementData.forceForFootInteraction = new float3(-curAcc.x, -Constants.gravity - accData.curAcc.y, -curAcc.y) / mass.InverseMass;
        // TODO: 300更换为 Agent 数量
        parallelECB.SetComponent<AgentMovementData>(index + 300, entity, curEntityMovementData);

        // 更新速度
        velocity.Linear.xz += curAcc * deltaTime;
        // builder.PopLineWidth();
        // velocity.Linear.xz = math.normalizesafe(desireDir) * 0;
    }
}


// [BurstCompile]
// [WithAll(typeof(AgentMovementData), typeof(Escaping))]
// partial struct OurModelJob : IJobEntity
// {
//     [ReadOnly] public NativeArray<CellData> cells;
//     [ReadOnly] public NativeArray<int> dests;
//     [ReadOnly] public FlowFieldSettingData settingData;
//     [ReadOnly] public PhysicsWorld physicsWorld;
//     [ReadOnly] public float deltaTime;
//     [ReadOnly] public TimerData accData;
//     [ReadOnly] public float standardVel;
//     [ReadOnly] public ComponentLookup<AgentMovementData> agentDataList;
//     [ReadOnly] public ComponentLookup<LocalTransform> localTransformList;
//     public EntityCommandBuffer.ParallelWriter parallelECB;
//     void Execute(Entity entity, [EntityIndexInQuery] int index, ref PhysicsVelocity velocity, in PhysicsMass mass)
//     {
//         var curEntityLocalTransform = localTransformList[entity];
//         var curEntityMovementData = agentDataList[entity];

//         float2 globalGuideDir = cells.GetPedestrainGlobalDir(curEntityLocalTransform.Position, settingData);

//         if (curEntityMovementData.SeeExit == false)
//         {
//             var localIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(curEntityLocalTransform.Position.xz, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
//             if (cells[localIndex].seeExit)
//             {
//                 curEntityMovementData.SeeExit = true;
//             }
//         }

//         float2 interactionForce = 0;
//         // 计算和其他智能体的交互力
//         NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
//         physicsWorld.OverlapSphere(curEntityLocalTransform.Position, 1, ref outHits, Constants.agentOnlyFilter);
//         foreach (var hit in outHits)
//         {
//             // 和行人之间的交互
//             if ((hit.Material.CustomTags & 0b_1000_0000) != 0)
//             {
//                 if (hit.Entity.Equals(entity)) continue;
//                 var direction = math.normalizesafe(curEntityLocalTransform.Position.xz - hit.Position.xz);
//                 interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f - accData.curPGA * Constants.gravity) * direction;
//             }
//         }

//         // 计算期望方向
//         float2 desireDir;
//         if (!curEntityMovementData.SeeExit)
//         {
//             // 看不见出口，则需要计算局部跟随方向
//             var LocalGuideDir = cells.GetPedestrainLocalDir(curEntityLocalTransform.Position, settingData);
//             var followNeighborDir = float2.zero;
//             // 计算从周围行人获得的跟随方向
//             foreach (var hit in outHits)
//             {
//                 if (hit.Entity.Equals(entity)) continue;
//                 var neighborFamiliarity = agentDataList[hit.Entity].familiarity;
//                 if (neighborFamiliarity > curEntityMovementData.familiarity)
//                 {
//                     followNeighborDir += math.normalizesafe(localTransformList[hit.Entity].Position.xz - curEntityLocalTransform.Position.xz) * neighborFamiliarity;
//                 }
//             }
//             if (followNeighborDir.Equals(float2.zero))
//             {
//                 RaycastInput ray = new RaycastInput
//                 {
//                     Start = curEntityLocalTransform.Position,
//                     Filter = Constants.agentWallOnlyFilter
//                 };
//                 // dirList 中存储没有发生碰撞的方向
//                 NativeList<int> dirList = new NativeList<int>(Allocator.Temp);
//                 NativeList<RaycastHit> allHits = new NativeList<RaycastHit>(Allocator.Temp);
//                 for (int i = -60; i <= 60; i += 10)
//                 {
//                     var dir = math.mul(quaternion.AxisAngle(math.up(), i), curEntityLocalTransform.Forward());
//                     ray.End = ray.Start + math.normalizesafe(dir);
//                     physicsWorld.CastRay(ray, ref allHits);
//                     if (allHits.Length <= 1)
//                     {
//                         dirList.Add(i);
//                     }
//                 }
//                 allHits.Dispose();
//                 // 若前方 120° 均发生碰撞，则跟随期望方向为当前方向旋转30°
//                 // UnityEngine.Debug.Log(dirList.IsEmpty);
//                 if (dirList.IsEmpty)
//                 {
//                     followNeighborDir = math.normalizesafe(math.mul(quaternion.AxisAngle(math.up(), 60), curEntityLocalTransform.Forward()).xz);
//                 }
//                 else
//                 {// 否则找到最大可视区域，进行前进
//                     NativeList<ViewArea> areas = new NativeList<ViewArea>(Allocator.Temp);
//                     int startIndex = 0, endIndex = 0, maxAreaSize = 0;

//                     // UnityEngine.Debug.Log(dirList[0]);
//                     for (int i = 1; i < dirList.Length; ++i)
//                     {
//                         // UnityEngine.Debug.Log(dirList[i]);
//                         if (dirList[i] - dirList[i - 1] != 10)
//                         {
//                             var temp = dirList[endIndex] - dirList[startIndex];
//                             maxAreaSize = math.max(maxAreaSize, temp);

//                             areas.Add(new ViewArea
//                             {
//                                 begin = dirList[startIndex],
//                                 end = dirList[endIndex],
//                                 size = temp
//                             });
//                             startIndex = i;
//                             endIndex = i;
//                         }
//                         else if (i == dirList.Length - 1 && (dirList[i] - dirList[i - 1] == 10))
//                         {
//                             var temp = dirList[i] - dirList[startIndex];
//                             maxAreaSize = math.max(maxAreaSize, temp);

//                             areas.Add(new ViewArea
//                             {
//                                 begin = dirList[startIndex],
//                                 end = dirList[i],
//                                 size = temp
//                             });
//                         }
//                         else
//                         {
//                             endIndex = i;
//                         }
//                     }
//                     // UnityEngine.Debug.Log(dirList.ToString());
//                     // UnityEngine.Debug.Log(areas.ToString());
//                     foreach (var item in areas)
//                     {
//                         // UnityEngine.Debug.Log(item.begin + ";" + item.end + ";" + item.size);
//                         if (item.size == maxAreaSize)
//                         {
//                             var rotateAngle = (item.begin + item.end) / 2;
//                             curEntityMovementData.debugAngle = rotateAngle;
//                             // UnityEngine.Debug.Log(rotateAngle);
//                             followNeighborDir = math.normalizesafe(math.mul(quaternion.AxisAngle(math.up(), rotateAngle), curEntityLocalTransform.Forward()).xz);
//                             break;
//                         }
//                     }
//                     // UnityEngine.Debug.Log("------------------------");
//                     areas.Dispose();
//                 }
//                 dirList.Dispose();
//             }
//             desireDir = math.normalizesafe(curEntityMovementData.familiarity * globalGuideDir + (1 - curEntityMovementData.familiarity) * (math.normalizesafe(followNeighborDir) + 0.75f * LocalGuideDir));
//         }
//         else
//         {
//             // 看见出口，直接跟随全局指导
//             desireDir = globalGuideDir;
//         }
//         outHits.Dispose();

//         // 设置人物方向
//         var targetRotation = quaternion.LookRotationSafe(new float3(desireDir.x, 0, desireDir.y), math.up());
//         curEntityLocalTransform.Rotation = UnityEngine.Quaternion.Slerp(curEntityLocalTransform.Rotation, targetRotation, 2.5f * deltaTime);
//         parallelECB.SetComponent<LocalTransform>(index, entity, curEntityLocalTransform);

//         // 计算期望速度
//         curEntityMovementData.desireSpeed = math.exp(-curEntityMovementData.deltaHeight - math.length(accData.curAcc)) * standardVel;
//         curEntityMovementData.curSpeed = math.length(velocity.Linear.xz);

//         // 计算加速度
//         float2 curAcc = ((desireDir * curEntityMovementData.desireSpeed - velocity.Linear.xz) / 0.5f - accData.curAcc.xz + interactionForce * mass.InverseMass);

//         // 计算脚部受力
//         curEntityMovementData.forceForFootInteraction = new float3(-curAcc.x, -Constants.gravity - accData.curAcc.y, -curAcc.y) / mass.InverseMass;
//         // TODO: 300更换为 Agent 数量
//         parallelECB.SetComponent<AgentMovementData>(index + 300, entity, curEntityMovementData);

//         // 更新速度
//         velocity.Linear.xz += curAcc * deltaTime;
//     }
// }