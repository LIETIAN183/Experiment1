using Unity.Entities;
using Unity.Mathematics;
using BansheeGz.BGDatabase;
using Unity.Transforms;
using Unity.Collections;
// 分析系统
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ComsMotionSystem))]
[UpdateAfter(typeof(SubShakeSystem))]
public class AnalysisSystem : SystemBase
{
    // Summary when simulation End
    public float pga;
    public float averageDis, minDis, maxDis;
    public float maxDegree;
    public int finalDrop, itemCount;

    protected override void OnCreate()
    {
        pga = float.MinValue;
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        var data = GetSingleton<AccTimerData>();

        // 0 存储角度 1,2,3 存储 掉落数量
        // 已知只有 NativeContainer 能从 Foreach 中读取数据，临时变量，引用变量均不行
        NativeArray<float> bridge = new NativeArray<float>(5, Allocator.TempJob);
        // bridge[0]:degree;bridge[1]:dropCount1;brighe[2]:dropCount2;bridge[3]:dropCount3;bridge[4]:AccOsc
        Entities.WithAll<AnalysisTag>().ForEach((in ShakeData data) =>
        {
            bridge[0] = math.atan(data.endMovement / data.length);
            bridge[4] = data.strength;
        }).Run();

        // 写数据前所有前一次操作要全部完成
        this.CompleteDependency();

        bridge[1] = bridge[2] = bridge[3] = 0;
        Entities.WithAll<ComsTag>().ForEach((in Translation translation, in ComsTag data) =>
        {
            if (translation.Value.y < 0.25f)
            {
                switch (data.groupID)
                {
                    case 1:
                        bridge[1]++;
                        break;
                    case 2:
                        bridge[2]++;
                        break;
                    case 3:
                        bridge[3]++;
                        break;
                    default:
                        break;
                }
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
        detail.F_AccOsc = bridge[4];
        detail.F_degree = bridge[0];
        detail.F_dropCount1 = (int)bridge[1];
        detail.F_dropCount2 = (int)bridge[2];
        detail.F_dropCount3 = (int)bridge[3];

        // 需要手动释放空间，不然会内存泄漏
        bridge.Dispose();

        pga = temp > pga ? temp : pga;
    }


    // 分析 导出数据
    protected override void OnStopRunning()
    {
        var data = GetSingleton<AccTimerData>();

        NativeArray<float> bridge = new NativeArray<float>(4, Allocator.TempJob);
        // bridge[0]:averageDis;bridge[1]:minDIs;brighe[2]:maxDis;bridge[3]:itemCount
        bridge[0] = bridge[1] = bridge[2] = bridge[3] = 0;
        // BGExcelImportGo.Instance.Export();
        Entities.WithAll<ComsTag>().ForEach((in Translation translation, in ComsTag data) =>
        {

        }).Schedule();

        DB_Summary summary = DB_Summary.NewEntity();
        summary.F_eqIndex = data.gmIndex;
        summary.F_PGA = pga;

        var lastRow = DB_Detail.GetEntity(data.timeCount - 1);
        summary.F_finalDrop1 = lastRow.F_dropCount1;
        summary.F_finalDrop2 = lastRow.F_dropCount2;
        summary.F_finaldrop3 = lastRow.F_dropCount3;

        bridge.Dispose();
    }
}
