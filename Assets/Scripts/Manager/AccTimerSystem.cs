using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AccTimerSystem : SystemBase
{

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<AccTimerData>();
        var entity = EntityManager.CreateEntity(typeof(AccTimerData));
        EntityManager.SetName(entity, "AccTimer");
        // 设置仿真系统 Update 时间间隔
        var fixedSimulationGroup = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FixedStepSimulationSystemGroup>();
        fixedSimulationGroup.Timestep = 0.01f;
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        // 获得单例数据
        var accTimer = GetSingleton<AccTimerData>();
        // 读取加速度序列
        ref BlobArray<GroundMotion> gmArray = ref GroundMotionBlobAssetsConstructor.gmBlobRefs[accTimer.gmIndex].Value.gmArray;
        // 更新时间进度条
        ECSUIController.Instance.progress.currentValue = accTimer.timeCount;
        // 更新加速度后，更新时间计量
        accTimer.acc = gmArray[accTimer.timeCount++].acceleration;
        accTimer.accMagnitude = math.length(accTimer.acc);
        // 更新单例数据
        SetSingleton(accTimer);

        // 超出范围后地震仿真结束
        if (accTimer.timeCount >= gmArray.Length)
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<ECSSystemManager>().Dective();
        }
        // Debug Acc
        // var temp = GetSingleton<AccTimerData>().acc;
        // Debug.Log(new Vector3(temp.x, temp.y, temp.z).magnitude);
    }

    public void Active(int index)
    {
        // 初始化单例数据
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.gmIndex = index;
        accTimer.acc = 0;
        accTimer.timeCount = 0;
        SetSingleton(accTimer);
        this.Enabled = true;
    }

    public void Dective()
    {

    }
}
