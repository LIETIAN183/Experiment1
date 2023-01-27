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
            var debugtype = SystemAPI.GetSingleton<FFVisTypeStateData>().ffVisType;
            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + debugtype.ToString() + "_" + UnityEngine.Time.time + ".png");
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            // var sceneSystem = World.GetExistingSystemManaged<SceneSystem>();
            // var guid = sceneSystem.GetSceneGUID("Assets/Scenes/SubScene/EnvironmentWithFluid.unity");
            // sceneSystem.UnloadScene(guid);
            // sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });
            simulation.GetExistingSystemManaged<SimInitializeSystem>().ReloadSubScene();
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            // SimUtility.instance.InvokeSeismic(0, 0);
            var settingData = SystemAPI.GetSingleton<FlowFieldSettingData>();
            settingData.index = math.select(1, 2, settingData.index == 1);
            SystemAPI.SetSingleton(settingData);
        }


    }
}
