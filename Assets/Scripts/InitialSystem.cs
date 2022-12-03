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
    public readonly string subScenePath = "Assets/Scenes/SubScene/ForTest.unity";
    protected override void OnUpdate()
    {
        // var sceneSystem = World.GetExistingSystem<SceneSystem>();
        // var guid = sceneSystem.GetSceneGUID(subScenePath);
        // sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });
        // this.Enabled = false;
    }

    public void ReloadSubScene()
    {
        var sceneSystem = World.GetExistingSystem<SceneSystem>();
        var guid = sceneSystem.GetSceneGUID(subScenePath);
        sceneSystem.UnloadScene(guid);
        sceneSystem.LoadSceneAsync(guid, new SceneSystem.LoadParameters() { AutoLoad = true });
    }
}
