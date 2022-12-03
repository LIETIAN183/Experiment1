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
            simulation.GetExistingSystem<UISystem>().displayUI = !simulation.GetExistingSystem<UISystem>().displayUI;
        }

        // 暂停
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UnityEngine.Time.timeScale = UnityEngine.Time.timeScale == 0 ? 1 : 0;
        }

        // 启动单次仿真
        if (Input.GetKeyUp(KeyCode.N))
        {
            simulation.GetExistingSystem<AccTimerSystem>().StartSingleSimulation(0, 0);
        }

        // 启动多次仿真
        if (Input.GetKeyUp(KeyCode.M))
        {
            simulation.GetExistingSystem<MultiRoundStatisticsSystem>().StartMultiRoundStatistics(0, 0);
        }

        // 截图快捷键
        if (Input.GetKeyUp(KeyCode.L))
        {
            var debugtype = simulation.GetExistingSystem<FlowFieldVisulizeSystem>()._curDisplayType;
            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + debugtype.ToString() + "_" + UnityEngine.Time.time + ".png");
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            // var sceneSystem = World.GetExistingSystem<SceneSystem>();
            // var guid = sceneSystem.GetSceneGUID("Assets/Scenes/SubScene/EnvironmentWithFluid.unity");
            // sceneSystem.UnloadScene(guid);
            // sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });
            simulation.GetExistingSystem<InitialSystem>().ReloadSubScene();
        }

        // if (Input.GetKeyUp(KeyCode.T))
        // {
        //     var sceneSystem = World.GetExistingSystem<SceneSystem>();
        //     var guid = sceneSystem.GetSceneGUID("Assets/Scenes/SubScene/EnvironmentWithFluid.unity");

        //     sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });// Flags = SceneLoadFlags.NewInstance,
        // }
    }
}
