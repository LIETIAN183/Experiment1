using Unity.Entities;
using UnityEngine;

public partial class InputManger : SystemBase
{
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        // 按 H 键隐藏UI界面
        if (Input.GetKeyUp(KeyCode.H))
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<UISystem>().displayUI = !World.DefaultGameObjectInjectionWorld.GetExistingSystem<UISystem>().displayUI;
        }

        // 暂停
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UnityEngine.Time.timeScale = UnityEngine.Time.timeScale == 0 ? 1 : 0;
        }

        // 启动单次仿真
        if (Input.GetKeyUp(KeyCode.N))
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<AccTimerSystem>().StartSingleSimulation(0, 0);
        }

        // 启动多次仿真
        if (Input.GetKeyUp(KeyCode.M))
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<MultiRoundStatisticsSystem>().StartMultiRoundStatistics(0, 0);
        }

        // 截图快捷键
        if (Input.GetKeyUp(KeyCode.L))
        {
            var debugtype = World.DefaultGameObjectInjectionWorld.GetExistingSystem<FlowFieldVisulizeSystem>()._curDisplayType;
            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + debugtype.ToString() + "_" + UnityEngine.Time.time + ".png");
        }
    }
}
