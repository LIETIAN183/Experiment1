using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public partial class InputManger : SystemBase
{
    public World simulation;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
    }

    protected override void OnUpdate()
    {
        // 按 Ecs 退出程序
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        // 按 H 键隐藏UI界面
        if (Input.GetKeyUp(KeyCode.H))
        {
            var displaySetting = SystemAPI.GetSingleton<UIDisplayStateData>();
            displaySetting.isDisplay = !displaySetting.isDisplay;
            SystemAPI.SetSingleton(displaySetting);
        }

        // 暂停
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UnityEngine.Time.timeScale = UnityEngine.Time.timeScale == 0 ? 1 : 0;
        }

        // 启动单次仿真
        if (Input.GetKeyUp(KeyCode.N))
        {
            var startEvent = SystemAPI.GetSingleton<StartSeismicEvent>();
            startEvent.isActivate = true;
            startEvent.targetPGA = startEvent.index = 0;
            SystemAPI.SetSingleton(startEvent);
        }

        // 启动多次仿真
        if (Input.GetKeyUp(KeyCode.M))
        {
            // SystemAPI.SetSingleton<ClearFluidEvent>(new ClearFluidEvent { isActivate = true });
        }

        // 截图快捷键
        if (Input.GetKeyUp(KeyCode.L))
        {
            var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
            var debugtype = SystemAPI.GetSingleton<FFVisTypeStateData>().ffVisType;
            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + debugtype.ToString() + "_" + setting.index + ".png");
        }

        // TODO：不适用于人群系统 
        if (Input.GetKeyUp(KeyCode.R))
        {
            // var sceneSystem = World.GetExistingSystemManaged<SceneSystem>();
            // var guid = sceneSystem.GetSceneGUID("Assets/Scenes/SubScene/EnvironmentWithFluid.unity");
            // sceneSystem.UnloadScene(guid);
            // sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });
            SystemAPI.SetSingleton<ClearFluidEvent>(new ClearFluidEvent { isActivate = true });
            simulation.GetExistingSystemManaged<SimInitializeSystem>().ReloadSubScene();
        }

        // 
        if (Input.GetKeyUp(KeyCode.K))
        {
            var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
            setting.index += 1;
            if (setting.index < 0 || setting.index > 3)
            {
                setting.index = 0;
            }
            SystemAPI.SetSingleton(setting);
        }

        if (Input.GetKeyUp(KeyCode.J))
        {
            var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
            setting.agentIndex += 1;
            if (setting.agentIndex < -1 || setting.agentIndex > 3)
            {
                setting.agentIndex = 0;
            }
            SystemAPI.SetSingleton(setting);
        }

    }
}
