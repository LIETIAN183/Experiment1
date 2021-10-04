using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using BansheeGz.BGDatabase;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AccTimerSystem : SystemBase
{
    World simulation;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;

        // 设置读取加速度的功能类为单例模式，方便数据同步和读取
        RequireSingletonForUpdate<AccTimerData>();
        var entity = EntityManager.CreateEntity(typeof(AccTimerData));
        // EntityManager.SetName(entity, "AccTimer");
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
        ref BlobArray<GroundMotion> gmArray = ref SetupBlobSystem.gmBlobRefs[accTimer.gmIndex].Value.gmArray;

        // 超出时间范围后地震仿真结束
        // 放在函数末尾会导致时间还在最后一秒时由于 timeCount++ 直接超出时间范围直接结束仿真
        // 放在这里可以保留最后一秒的仿真数据
        if (accTimer.timeCount >= gmArray.Length)
        {
            // 关闭其他系统
            ControlSystem(false);

            // 分析系统 应该在下一帧结束
            this.Enabled = false;

            // 结束场景后自动重置，执行下一轮仿真
            var setting = GetSingleton<AnalysisTypeData>();
            setting.task = AnalysisTasks.Reload;
            SetSingleton<AnalysisTypeData>(setting);
        }
        else
        {
            var setting = GetSingleton<AnalysisTypeData>();
            // 更新时间进度条
            accTimer.elapsedTime = accTimer.timeCount * accTimer.dataDeltaTime;
            ECSUIController.Instance.progress.currentTime = accTimer.elapsedTime;
            // 更新加速度后，更新时间计量
            accTimer.acc = gmArray[accTimer.timeCount].acceleration * setting.cofficient;
            accTimer.timeCount += accTimer.increaseNumber;

            // 更新单例数据
            SetSingleton<AccTimerData>(accTimer);
        }

        // Debug Acc
        // var temp = GetSingleton<AccTimerData>().acc;
        // Debug.Log(new Vector3(temp.x, temp.y, temp.z).magnitude);
    }

    public void Active(int index)
    {
        //-----------------------------------数据分析 填写地震Index和地震名字-----------------------------------------------------
        // 不知道为什么，这里读取 analysisTypeData 存在误差


        // ------------------------------------- Analysis END -------------------------------------------------------------------

        // 初始化单例数据
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.gmIndex = index;
        accTimer.acc = 0;
        accTimer.timeCount = 0;
        accTimer.dataDeltaTime = SetupBlobSystem.gmBlobRefs[index].Value.deltaTime;
        accTimer.increaseNumber = (int)(0.01f / accTimer.dataDeltaTime);
        SetSingleton<AccTimerData>(accTimer);
        this.Enabled = true;

        ControlSystem(true);
    }

    protected override void OnStopRunning()
    {
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.acc = 0;
        SetSingleton<AccTimerData>(accTimer);
    }

    public void ControlSystem(bool status)
    {
        simulation.GetExistingSystem<GlobalGravitySystem>().Enabled = status;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = status;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = status;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = status;
        simulation.GetExistingSystem<AnalysisSystem>().Enabled = status;
    }
}
