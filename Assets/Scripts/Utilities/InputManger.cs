using Unity.Entities;
using UnityEngine;
using Unity.Scenes;
using System.Threading.Tasks;
using Hash128 = Unity.Entities.Hash128;

public partial class InputManger : SystemBase
{
    public World simulation;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        // 按 H 键隐藏UI界面
        if (Input.GetKeyUp(KeyCode.H))
        {
            simulation.GetExistingSystemManaged<UISystem>().displayUI = !simulation.GetExistingSystemManaged<UISystem>().displayUI;
        }

        // 暂停
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UnityEngine.Time.timeScale = UnityEngine.Time.timeScale == 0 ? 1 : 0;
        }

        // 启动单次仿真
        if (Input.GetKeyUp(KeyCode.N))
        {
            simulation.GetExistingSystemManaged<AccTimerSystem>().StartSingleSimulation(0, 0);
        }

        // 启动多次仿真
        if (Input.GetKeyUp(KeyCode.M))
        {
            simulation.GetExistingSystemManaged<MultiRoundStatisticsSystem>().StartMultiRoundStatistics(0, 0);
        }

        // 截图快捷键
        if (Input.GetKeyUp(KeyCode.L))
        {
            var debugtype = simulation.GetExistingSystemManaged<FlowFieldVisulizeSystem>()._curDisplayType;
            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + debugtype.ToString() + "_" + UnityEngine.Time.time + ".png");
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            // var sceneSystem = World.GetExistingSystemManaged<SceneSystem>();
            // var guid = sceneSystem.GetSceneGUID("Assets/Scenes/SubScene/EnvironmentWithFluid.unity");
            // sceneSystem.UnloadScene(guid);
            // sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });
            simulation.GetExistingSystemManaged<InitialSystem>().ReloadSubScene();
        }

        // if (Input.GetKeyUp(KeyCode.T))
        // {
        //     var sceneSystem = World.GetExistingSystemManaged<SceneSystem>();
        //     var guid = sceneSystem.GetSceneGUID("Assets/Scenes/SubScene/EnvironmentWithFluid.unity");

        //     sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });// Flags = SceneLoadFlags.NewInstance,
        // }
    }
}
