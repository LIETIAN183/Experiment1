using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Scenes;

// [DisableAutoCreation]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class InitialSystem : SystemBase
{
    // public readonly string subScenePath = "Assets/Scenes/SubScene/EnvironmentWithFluid.unity";
    public readonly string subScenePath = "Assets/Scenes/SubScene/ForTest.unity";
    protected override void OnUpdate()
    {
        UnityEngine.Debug.Log("Test");
        // var sceneSystem = World.GetExistingSystemManaged<SceneSystem>();
        var guid = SceneSystem.GetSceneGUID(ref World.Unmanaged.GetExistingSystemState<SceneSystem>(), subScenePath);
        var entity = SceneSystem.LoadSceneAsync(World.Unmanaged, guid, new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.NewInstance, AutoLoad = true });
        UnityEngine.Debug.Log(SceneSystem.IsSceneLoaded(World.Unmanaged, entity));
        this.Enabled = false;
    }

    public void ReloadSubScene()
    {
        // var sceneSystem = World.GetExistingSystemManaged<SceneSystem>();
        var guid = SceneSystem.GetSceneGUID(ref World.Unmanaged.GetExistingSystemState<SceneSystem>(), subScenePath);
        SceneSystem.UnloadScene(World.Unmanaged, guid);
        SceneSystem.LoadSceneAsync(World.Unmanaged, guid, new SceneSystem.LoadParameters() { AutoLoad = true });
    }
}
