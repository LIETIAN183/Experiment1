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
    public float maxDegree;

    protected override void OnCreate()
    {
        maxDegree = float.MinValue;
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
                    // gruopID->1: 不加振荡组
                    // groupID->2: 加振荡
                    // groupID->3: 加震荡，加质量
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

        pga = temp > pga ? temp : pga;
        maxDegree = bridge[0] > maxDegree ? bridge[0] : maxDegree;

        // 需要手动释放空间，不然会内存泄漏
        bridge.Dispose();
    }


    // 分析 导出数据
    protected override void OnStopRunning()
    {
        var data = GetSingleton<AccTimerData>();

        NativeArray<float> bridge = new NativeArray<float>(10, Allocator.TempJob);
        // bridge[0]:averageDis1; bridge[1]:minDIs1; brighe[2]:maxDis1;
        // bridge[3]:averageDis2; bridge[4]:minDIs2; brighe[5]:maxDis2;
        // bridge[6]:averageDis3; bridge[7]:minDIs3; brighe[8]:maxDis3;
        // bridge[9]:itemCount
        bridge[0] = bridge[3] = bridge[6] = bridge[9] = 0;
        bridge[1] = bridge[4] = bridge[7] = float.MaxValue;
        bridge[2] = bridge[5] = bridge[8] = float.MinValue;
        // BGExcelImportGo.Instance.Export();
        Entities.WithAll<ComsTag>().ForEach((in Translation translation, in ComsTag data) =>
        {
            // 统计的是水平位移
            var dis = math.length(data.originPosition.xz - translation.Value.xz);
            switch (data.groupID)
            {
                case 1:
                    bridge[0] += dis;
                    bridge[1] = dis < bridge[1] ? dis : bridge[1];
                    bridge[2] = dis > bridge[2] ? dis : bridge[2];
                    bridge[9]++;
                    break;
                case 2:
                    bridge[3] += dis;
                    bridge[4] = dis < bridge[4] ? dis : bridge[4];
                    bridge[5] = dis > bridge[5] ? dis : bridge[5];
                    break;
                case 3:
                    bridge[6] += dis;
                    bridge[7] = dis < bridge[7] ? dis : bridge[7];
                    bridge[8] = dis > bridge[8] ? dis : bridge[8];
                    break;
                default:
                    break;
            }
        }).Schedule();

        // 读数据要等所有数据写完才能读
        this.CompleteDependency();

        DB_Summary summary = DB_Summary.NewEntity();
        summary.F_eqIndex = data.gmIndex;
        summary.F_PGA = pga;

        summary.F_averageDis1 = bridge[0] / bridge[9];
        summary.F_averageDis2 = bridge[3] / bridge[9];
        summary.F_averageDis3 = bridge[6] / bridge[9];

        summary.F_minDis1 = bridge[1];
        summary.F_minDis2 = bridge[4];
        summary.F_minDis3 = bridge[7];

        summary.F_maxDis1 = bridge[2];
        summary.F_maxDis2 = bridge[5];
        summary.F_maxDis3 = bridge[8];

        summary.F_maxDegree = maxDegree;

        var lastRow = DB_Detail.GetEntity(data.timeCount - 1);
        summary.F_finalDrop1 = lastRow.F_dropCount1;
        summary.F_finalDrop2 = lastRow.F_dropCount2;
        summary.F_finaldrop3 = lastRow.F_dropCount3;

        summary.F_itemCount = (int)bridge[9];

        bridge.Dispose();
    }
}
