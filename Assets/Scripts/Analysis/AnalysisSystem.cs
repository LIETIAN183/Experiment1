using Unity.Entities;
using Unity.Mathematics;
using BansheeGz.BGDatabase;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
// 分析系统
// [DisableAutoCreation]
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ComsMotionSystem))]
[UpdateAfter(typeof(SubShakeSystem))]
// [UpdateAfter(typeof(AgentStateSystem))]
public class AnalysisSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        var data = GetSingleton<AccTimerData>();
        var setting = GetSingleton<AnalysisTypeData>();

        // 已知只有 NativeContainer 能从 Foreach 中读取数据，临时变量，引用变量均不行
        NativeArray<int> bridge = new NativeArray<int>(2, Allocator.TempJob);

        bridge[0] = 0;
        Entities.WithAll<ComsData>().ForEach((in Translation translation, in ComsData data) =>
        {
            if (translation.Value.y < 0.25f) bridge[0]++;
        }).Schedule();

        this.CompleteDependency();

        bridge[1] = 0;
        Entities.WithAll<AgentMovementData>().ForEach((in AgentMovementData data) =>
        {
            if (data.state == AgentState.Escaped) bridge[1]++;
        }).Schedule();

        // 读数据要等所有数据写完才能读
        this.CompleteDependency();

        var direction = data.acc.z > 0 ? 1 : -1;
        // Add row to Detial Table EachStep
        DB_Detail detail = DB_Detail.NewEntity();
        detail.F_eqIndex = setting.eqCount;
        detail.F_time = data.elapsedTime;
        detail.F_xAcc = data.acc.x;
        detail.F_zAcc = data.acc.z;
        detail.F_yAcc = data.acc.y;
        detail.F_horiAcc = direction * math.length(data.acc.xz);
        detail.F_Acc = direction * math.length(data.acc);
        detail.F_dropCount = bridge[0];
        detail.F_escaped = bridge[1];


        // 人数到达标准，结束仿真
        var count = GetSingleton<SpawnerData>().desireCount;
        if (bridge[1].Equals(count))
        {
            var analysisSetting = GetSingleton<AnalysisTypeData>();
            analysisSetting.task = AnalysisTasks.Reload;
            SetSingleton<AnalysisTypeData>(analysisSetting);
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<AccTimerSystem>().Enabled = false;
            this.Enabled = false;

            var cofficient = GetSingleton<AnalysisTypeData>().cofficient / 100f;

            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + SetupBlobSystem.gmBlobRefs[setting.index].Value.gmName.ToString() + "_" + cofficient.ToString() + ".png");
        }

        // 需要手动释放空间，不然会内存泄漏
        bridge.Dispose();
    }

    // 分析 导出数据
    protected override void OnStopRunning()
    {
        var data = GetSingleton<AccTimerData>();
        var setting = GetSingleton<AnalysisTypeData>();

        // World.DefaultGameObjectInjectionWorld.GetExistingSystem<AgentStateSystem>().Enabled = false;

        NativeArray<float> bridge = new NativeArray<float>(14, Allocator.TempJob);
        bridge[0] = bridge[1] = 0;
        Entities.WithAll<ComsData>().ForEach((in Translation translation, in ComsData data) =>
        {
            bridge[0]++;
            if (translation.Value.y < 0.25f) bridge[1]++;
        }).Run();

        this.CompleteDependency();

        // bridge[2] = bridge[3] = bridge[4] = 0;
        // bridge[5] = bridge[6] = bridge[7] = 0;
        // bridge[8] = bridge[9] = bridge[10] = 0;
        // bridge[11] = bridge[12] = bridge[13] = 0;

        bridge[2] = bridge[5] = bridge[8] = bridge[11] = 0;
        bridge[3] = bridge[6] = bridge[9] = bridge[12] = float.MaxValue;
        bridge[4] = bridge[7] = bridge[10] = bridge[13] = float.MinValue;

        Entities.WithAll<AgentMovementData>().ForEach((in AgentMovementData data) =>
        {
            bridge[2] += data.reactionTime;
            bridge[3] = math.min(bridge[3], data.reactionTime);
            bridge[4] = math.max(bridge[4], data.reactionTime);

            bridge[5] += data.escapeTime;
            bridge[6] = math.min(bridge[6], data.escapeTime);
            bridge[7] = math.max(bridge[7], data.escapeTime);

            bridge[8] += data.pathLength;
            bridge[9] = math.min(bridge[9], data.pathLength);
            bridge[10] = math.max(bridge[10], data.pathLength);

            bridge[11] += data.pathLength / (data.escapeTime - data.reactionTime);
            bridge[12] = math.min(bridge[12], data.pathLength / (data.escapeTime - data.reactionTime));
            bridge[13] = math.max(bridge[13], data.pathLength / (data.escapeTime - data.reactionTime));
        }).Schedule();

        // 读数据要等所有数据写完才能读
        this.CompleteDependency();

        var count = GetSingleton<SpawnerData>().desireCount;

        DB_Summary summary = DB_Summary.NewEntity();
        summary.F_eqIndex = setting.eqCount;
        summary.F_PGA = data.pga;

        summary.F_itemCount = ((int)bridge[0]);
        summary.F_finalDrop = ((int)bridge[1]);
        summary.F_simulationTime = data.elapsedTime;
        summary.F_reactionTIme_ave = bridge[2] / count;
        summary.F_reactionTIme_min = bridge[3];
        summary.F_reactionTIme_max = bridge[4];

        summary.F_escapeTIme_ave = bridge[5] / count;
        summary.F_escapeTIme_min = bridge[6];
        summary.F_escapeTIme_max = bridge[7];

        summary.F_escapeLength_ave = bridge[8] / count;
        summary.F_escapeLength_min = bridge[9];
        summary.F_escapeLength_max = bridge[10];

        summary.F_vel_ave = bridge[11] / count;
        summary.F_vel_min = bridge[12];
        summary.F_vel_max = bridge[13];

        bridge.Dispose();

        // 每一次地震导出一次
        BGExcelImportGo.Instance.Export();

        // 重置数据库
        DB_Detail.MetaDefault.ClearEntities();
        DB_Eq.MetaDefault.ClearEntities();
        DB_Summary.MetaDefault.ClearEntities();

        // World.DefaultGameObjectInjectionWorld.GetExistingSystem<AgentStateSystem>().Enabled = true;
    }
}
