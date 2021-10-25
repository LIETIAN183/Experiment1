using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AccTimerSystem : SystemBase
{
    World simulation;

    private float timeStep = 0.02f;
    private int increase;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;

        // 设置读取加速度的功能类为单例模式，方便数据同步和读取
        RequireSingletonForUpdate<AccTimerData>();
        var entity = EntityManager.CreateEntity(typeof(AccTimerData));
        // 设置仿真系统 Update 时间间隔
        var fixedSimulationGroup = simulation?.GetExistingSystem<FixedStepSimulationSystemGroup>();
        fixedSimulationGroup.Timestep = timeStep;

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
            // 分析系统
            this.Enabled = false;
        }
        else
        {
            accTimer.elapsedTime = accTimer.timeCount * accTimer.dataDeltaTime;
            // 更新时间进度条
            ECSUIController.Instance.progress.currentTime = accTimer.elapsedTime;
            accTimer.acc = gmArray[accTimer.timeCount].acceleration;
            accTimer.timeCount += accTimer.increaseNumber;

            // 更新单例数据
            SetSingleton(accTimer);
        }
    }

    public void Active(int index)
    {
        //-----------------------------------数据分析 填写地震Index和地震名字-----------------------------------------------------
        // DB_Eq newData = DB_Eq.NewEntity();
        // newData.F_eqIndex = index;
        // newData.F_eqName = SetupBlobSystem.gmBlobRefs[index].Value.gmName.ToString();
        // ------------------------------------- Analysis END -------------------------------------------------------------------

        // 初始化单例数据
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.gmIndex = index;
        accTimer.acc = 0;
        accTimer.timeCount = 0;
        accTimer.dataDeltaTime = SetupBlobSystem.gmBlobRefs[index].Value.deltaTime;
        accTimer.increaseNumber = (int)(timeStep / accTimer.dataDeltaTime);
        SetSingleton(accTimer);
        this.Enabled = true;

        ControlSystem(true);
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = true;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = true;
        simulation.GetExistingSystem<AgentInitSystem>().Enabled = true;
        simulation.GetExistingSystem<AgentMovementSystem>().Enabled = true;
        simulation.GetExistingSystem<PathDisplaySystem>().Enabled = true;
    }

    protected override void OnStopRunning()
    {
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.acc = 0;
        SetSingleton(accTimer);
    }

    public void ControlSystem(bool status)
    {
        simulation.GetExistingSystem<GlobalGravitySystem>().Enabled = status;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = status;


        // 分析
        // simulation.GetExistingSystem<AnalysisSystem>().Enabled = status;
    }
}
