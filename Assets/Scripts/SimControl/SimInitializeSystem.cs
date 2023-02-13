using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Scenes;
using UnityEngine;
using System.Collections.Generic;
using Unity.Scenes;

using Hash128 = Unity.Entities.Hash128;

public struct sceneRef
{
    public string sceneName;
    public string scenePath;
    public Hash128 guid;
}

// https://forum.unity.com/threads/dots-subscene-works-well-in-editor-but-wont-load-in-standalone-build.1038679/#post-8618604
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class SimInitializeSystem : SystemBase
{
    // TODO: 手动添加 Scene 路径
    // 必须先将该 subscene 放置在 main scene 内，同时不可以激活，可以不 AutoLoad，同时还必需加入 Build 的 SceneList
    // public readonly string subScenesFolderPath = "Assets/Scenes/SubScene/ForTest.unity";
    private readonly string[] subScenesPath = { "Assets/Scenes/SubSceneForLoad/ForTest.unity", "Assets/Scenes/SubSceneForLoad/EnvironmentWithFluid.unity" };
    private List<sceneRef> sceneRefs;

    public int curSceneIndex;

    private Entity sceneEntity;

    private SceneSystem.LoadParameters loadParameters;

    protected override void OnCreate()
    {
        sceneRefs = new List<sceneRef>();
        foreach (var i in subScenesPath)
        {
            string[] tempArray = i.Split('/');
            var temp2 = tempArray[tempArray.Length - 1];
            var name = temp2.Split('.')[0];
            sceneRefs.Add(new sceneRef
            {
                sceneName = name,
                scenePath = i,
                guid = GetGUID(i)
            });
        }
        curSceneIndex = 0;
        loadParameters = new SceneSystem.LoadParameters { AutoLoad = true };
        // 初始化时设置时间间隔0.04f，防止太卡
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FixedStepSimulationSystemGroup>().Timestep = 0.04f;
        Application.targetFrameRate = -1;
    }

    protected override void OnUpdate()
    {
        sceneEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, sceneRefs[curSceneIndex].guid, loadParameters);
        this.Enabled = false;
    }

    public void ReloadSubScene()
    {
        SceneSystem.UnloadScene(World.Unmanaged, sceneRefs[curSceneIndex].guid);
        sceneEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, sceneRefs[curSceneIndex].guid, loadParameters);
    }

    public Hash128 GetGUID(string path)
    {
        return SceneSystem.GetSceneGUID(ref this.CheckedStateRef, path);
    }

    public bool SceneLoadState()
    {
        if (sceneEntity == null)
        {
            sceneEntity = SceneSystem.GetSceneEntity(World.Unmanaged, sceneRefs[curSceneIndex].guid);
        }
        return SceneSystem.IsSceneLoaded(World.Unmanaged, sceneEntity);
    }

    public string[] GetSceneNameArray()
    {
        string[] nameArray = new string[sceneRefs.Count];
        for (int i = 0; i < sceneRefs.Count; ++i)
        {
            nameArray[i] = sceneRefs[i].sceneName;
        }
        return nameArray;
    }

    public bool ChangeScene(int index)
    {
        if (index < 0 || index > sceneRefs.Count - 1 || SceneLoadState())
        {
            return false;
        }
        SceneSystem.UnloadScene(World.Unmanaged, sceneRefs[curSceneIndex].guid);
        curSceneIndex = index;
        sceneEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, sceneRefs[curSceneIndex].guid, loadParameters);
        return true;
    }
}
