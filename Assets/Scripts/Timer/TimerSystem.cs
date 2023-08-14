using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct TimerSystem : ISystem
{
    private EntityQuery escapingQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // 关联数据和系统
        // 这里不用 System Entity, 而使用新建的 Entity 是因为 System Entity 不支持 IJobEntity
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.SetName(entity, "TimerDataEntity");
        state.EntityManager.AddComponentData<TimerData>(entity, new TimerData { simDeltaTime = 0.04f });

        escapingQuery = state.GetEntityQuery(ComponentType.ReadOnly<Escaping>());

        state.Enabled = false;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 获取时间数据
        var data = SystemAPI.GetSingleton<TimerData>();

        // 获得地震事件对应的加速度数据
        ref var accList = ref SystemAPI.GetSingletonBuffer<BlobRefBuffer>(true)[data.seismicEventIndex].Value.Value.eventAccArray;

        // 根据当前时刻更新地震加速度、地震速度以及当前峰值地面加速度
        data.elapsedTime = data.accListIndex * data.eventDeltaTime;
        if (data.elapsedTime < data.eventDuration)
        {
            data.curAcc = accList[data.accListIndex] * data.adjustmentPGAFactor;
            data.curVel += data.curAcc * data.simDeltaTime;

            data.curPGA = math.max(data.curPGA, math.length(data.curAcc) / Constants.gravity);
        }
        else { data.curAcc = float3.zero; }
        // 更新时刻
        data.accListIndex += data.accListIndexIncrement;
        SystemAPI.SetSingleton(data);

        // 判断结束条件
        var simulationSetting = SystemAPI.GetSingleton<SimConfigData>();

        // 有行人时所有行人逃出后结束仿真
        if (!simulationSetting.performStatistics && !simulationSetting.simAgent && data.elapsedTime >= data.eventDuration + 2)
        // else if (!simulationSetting.performStatistics && data.elapsedTime >= 3)
        {
            SystemAPI.SetSingleton(new EndSeismicEvent { isActivate = true });
        }
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}


/// <summary>
/// 初始化时间数据 TimerData Component
/// </summary>
// SystemAPI 不可用, System Entity 不支持 IJobEntity
// [BurstCompile]
// ToString() 不支持 Burst Compile, 但是 BlobString 又必须用 ToString
[WithAll(typeof(TimerData))]
partial struct TimerInitJob : IJobEntity
{
    [ReadOnly] public int eventIndex;
    [ReadOnly] public float simPGA;

    void Execute(ref TimerData data, in DynamicBuffer<BlobRefBuffer> refBuffer)
    {
        ref var dataResource = ref refBuffer[eventIndex].Value.Value;
        data.seismicEventIndex = eventIndex;
        data.simPGA = simPGA;
        data.simDeltaTime = data.simDeltaTime.inRange(0.01f, 0.06f, 0.04f);
        // 更新数据组件
        data.seismicEventName = dataResource.eventName.ToString();
        data.eventDeltaTime = dataResource.eventDeltaTime;
        data.accListIndex = 0;
        data.accListIndexIncrement = (int)(data.simDeltaTime / data.eventDeltaTime);
        data.curAcc = float3.zero;
        data.curVel = float3.zero;
        data.elapsedTime = 0;
        data.eventDuration = dataResource.eventAccArray.Length * dataResource.eventDeltaTime;
        data.curPGA = 0;
        data.eventPGA = dataResource.eventAccArray.maxPGA();
        data.adjustmentPGAFactor = math.select(data.simPGA / data.eventPGA, 1, data.simPGA.Equals(0));
        data.envEnhanceFactor = math.select(data.envEnhanceFactor, 1, data.envEnhanceFactor.Equals(0));
    }
}