using Unity.Entities;
using Unity.Mathematics;
using BansheeGz.BGDatabase;
using Unity.Transforms;
using Unity.Collections;
// 分析系统
[DisableAutoCreation]
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ComsMotionSystem))]
[UpdateAfter(typeof(SubShakeSystem))]
public class AnalysisSystem : SystemBase
{
    // Summary when simulation End
    public float pga;

    private int addDataLineCount;

    protected override void OnCreate()
    {
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        var data = GetSingleton<AccTimerData>();

        // 0 存储角度 1,2,3 存储 掉落数量
        // 已知只有 NativeContainer 能从 Foreach 中读取数据，临时变量，引用变量均不行
        NativeArray<int> bridge = new NativeArray<int>(1, Allocator.TempJob);

        bridge[0] = 0;
        Entities.WithAll<ComsTag>().ForEach((in Translation translation, in ComsTag data) =>
        {
            if (translation.Value.y < 0.25f)
            {
                bridge[0]++;
            }
        }).Schedule();

        // 读数据要等所有数据写完才能读
        this.CompleteDependency();
        var direction = data.acc.z > 0 ? 1 : -1;

        // Add row to Detial Table EachStep
        var temp = math.length(data.acc);
        DB_Detail detail = DB_Detail.NewEntity();
        detail.F_eqIndex = data.gmIndex;
        detail.F_time = data.elapsedTime;
        detail.F_xAcc = data.acc.x;
        detail.F_zAcc = data.acc.z;
        detail.F_yAcc = data.acc.y;
        detail.F_horiAcc = direction * math.length(data.acc.xz);
        detail.F_Acc = direction * temp;
        detail.F_dropCount = bridge[0];

        pga = temp > pga ? temp : pga;

        // 需要手动释放空间，不然会内存泄漏
        bridge.Dispose();
        // 记录插入数据的行数
        ++addDataLineCount;
    }

    protected override void OnStartRunning()
    {
        pga = float.MinValue;
        addDataLineCount = 0;
    }

    // 分析 导出数据
    protected override void OnStopRunning()
    {
        var data = GetSingleton<AccTimerData>();

        NativeArray<int> bridge = new NativeArray<int>(1, Allocator.TempJob);
        // bridge[0]:itemCount
        bridge[0] = 0;
        // BGExcelImportGo.Instance.Export();
        Entities.WithAll<ComsTag>().ForEach((in Translation translation, in ComsTag data) =>
        {
            bridge[0]++;
        }).Schedule();

        // 读数据要等所有数据写完才能读
        this.CompleteDependency();

        DB_Summary summary = DB_Summary.NewEntity();
        summary.F_eqIndex = data.gmIndex;
        summary.F_PGA = pga;

        var lastRow = DB_Detail.GetEntity(addDataLineCount - 1);

        summary.F_itemCount = bridge[0];
        summary.F_EqLength = lastRow.F_time;

        bridge.Dispose();

        pga = 0;
    }
}
