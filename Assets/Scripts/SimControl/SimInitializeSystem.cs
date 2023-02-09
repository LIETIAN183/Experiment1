using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Scenes;
using UnityEngine;

using Hash128 = Unity.Entities.Hash128;

// [DisableAutoCreation]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class SimInitializeSystem : SystemBase
{
    // public readonly string subScenePath = "Assets/Scenes/SubScene/EnvironmentWithFluid.unity";
    // 必须先将该 subscene 放置在 main scene 内，可以不 AutoLoad，同时还必需加入 Build 的 SceneList
    public readonly string subScenesFolderPath = "Assets/Scenes/SubScene/ForTest.unity";
    Hash128 guid;

    private Entity sceneEntity;

    public NativeHashMap<FixedString32Bytes, Hash128> string2GUID;

    protected override void OnCreate()
    {
        // 初始化时设置时间间隔0.04f，防止太卡
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FixedStepSimulationSystemGroup>().Timestep = 0.04f;
        Application.targetFrameRate = -1;
    }

    protected override void OnUpdate()
    {
        UpdateGUID();
        sceneEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, guid, new SceneSystem.LoadParameters { AutoLoad = true });
        this.Enabled = false;

    }

    public void ReloadSubScene()
    {
        SceneSystem.UnloadScene(World.Unmanaged, guid);
        sceneEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, guid, new SceneSystem.LoadParameters { AutoLoad = true });
    }

    public void UpdateGUID()
    {
        guid = SceneSystem.GetSceneGUID(ref World.Unmanaged.GetExistingSystemState<SceneSystem>(), subScenesFolderPath);
    }

    public bool SceneLoadState()
    {
        return SceneSystem.IsSceneLoaded(World.Unmanaged, sceneEntity);
    }
}
