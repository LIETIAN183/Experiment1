using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
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
            // 分析系统
            ECSUIController.Instance.ShowNotification("Simulation End");
            // 废弃 ECSSystemManager 系统，因为分离会导致运行时间存在差异，导致 Analysis NativeContainer 内存泄露，所以只能由 AccTimerSystem 直接控制其他系统的停止
            this.Enabled = false;
        }



        // 更新时间进度条
        ECSUIController.Instance.progress.currentValue = accTimer.timeCount;// 这里还不需要减一
        // 计算当前已经过去的时间
        accTimer.elapsedTime = accTimer.timeCount * 0.01f;
        // 更新加速度后，更新时间计量
        accTimer.acc = gmArray[accTimer.timeCount++].acceleration;

        accTimer.accMagnitude = math.length(accTimer.acc);
        // 更新单例数据
        SetSingleton(accTimer);


        // Debug Acc
        // var temp = GetSingleton<AccTimerData>().acc;
        // Debug.Log(new Vector3(temp.x, temp.y, temp.z).magnitude);
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
        SetSingleton(accTimer);
        this.Enabled = true;

        ControlSystem(true);
    }

    protected override void OnStartRunning()
    {
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.acc = 0;
        SetSingleton(accTimer);
    }

    public void ControlSystem(bool status)
    {
        simulation.GetExistingSystem<GlobalGravitySystem>().Enabled = status;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = status;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = status;

        // 全局仿真时 SyncSystem、SubShakeSystem 可以选择不启用
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = status;
        simulation.GetExistingSystem<SyncSystem>().Enabled = status;

        // 分析
        // simulation.GetExistingSystem<AnalysisSystem>().Enabled = status;
    }
}
