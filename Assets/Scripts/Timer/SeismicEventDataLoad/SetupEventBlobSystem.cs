using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System.IO;
using System.Linq;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class SetupEventBlobSystem : SystemBase
{
    // 数据文件夹路径
    private static readonly string seismicDataPath = Application.streamingAssetsPath + "/SeismicData/";

    protected override void OnCreate()
    {
        // 在系统中挂在表示数据读取成功状态的组件
        EntityManager.AddComponentData<DataLoadStateData>(this.SystemHandle, new DataLoadStateData { isLoadSuccessed = false });
    }

    // 读取文件内所有地震事件的数据
    protected override void OnUpdate()
    {
        if (SystemAPI.TryGetSingletonEntity<TimerData>(out var entity))
        {
            if (!SystemAPI.HasBuffer<BlobRefBuffer>(entity))
            {
                EntityManager.AddBuffer<BlobRefBuffer>(entity);
            }
        }

        if (SystemAPI.TryGetSingletonBuffer<BlobRefBuffer>(out var blobRefBuffer))
        {
            // 判断目标路径的文件夹是否存在
            if (!Directory.Exists(seismicDataPath))
            {
                var message = SystemAPI.GetSingleton<MessageEvent>();
                message.isActivate = true;
                message.message = "SeismicData Folder don't exist in StreamingAsset Folder";
                message.displayForever = true;
                SystemAPI.SetSingleton(message);
                return;
            }
            // 清理数据
            CleanDirectory();

            // 分类 AT2 数据
            ClassifyFile();

            // 获得地震事件名称列表
            var events = DataReaderUtil.SeismicEventFolders(seismicDataPath);
            // SeismicData文件夹内没有地震事件子文件夹
            if (events.ToArray().Length == 0)
            {
                var message = SystemAPI.GetSingleton<MessageEvent>();
                message.isActivate = true;
                message.message = "No Seismic Event in SeismicData Folder";
                message.displayForever = true;
                SystemAPI.SetSingleton(message);
                return;
            }

            // 创建 BlobAsset 资源引用
            // 遍历每个文件夹内的AT2文件并读取数据
            foreach (var item in events)
            {
                float tempDeltaTime;
                var accData = DataReaderUtil.ReadFile(seismicDataPath + item + "/", Constants.gravity, out tempDeltaTime);
                if (accData.Equals(null))
                {
                    var message = SystemAPI.GetSingleton<MessageEvent>();
                    message.isActivate = true;
                    message.message = "Read AT2 File Error";
                    message.displayForever = true;
                    SystemAPI.SetSingleton(message);
                    return;
                }

                // 创建 BlobBuilder，赋值 seismicAccArray 和 seismicName
                BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);

                // Blob 声明资产类型
                ref SeismicEventBlobAsset seismicBlobAsset = ref blobBuilder.ConstructRoot<SeismicEventBlobAsset>();
                BlobBuilderArray<float3> seismicAccArray = blobBuilder.Allocate(ref seismicBlobAsset.eventAccArray, accData.Count);

                // 存储加速度数据为 BlobArray
                for (int i = 0; i < accData.Count; ++i) { seismicAccArray[i] = accData[i]; }

                // 存储相应地震名字为 BlobString
                blobBuilder.AllocateString(ref seismicBlobAsset.eventName, item);
                seismicBlobAsset.eventDeltaTime = tempDeltaTime;

                // 声明 Blob 资产引用
                BlobAssetReference<SeismicEventBlobAsset> seismicBlobAssetReference =
                 blobBuilder.CreateBlobAssetReference<SeismicEventBlobAsset>(Allocator.Persistent);

                blobRefBuffer.Add(seismicBlobAssetReference);
                // 释放 BlobBuilder
                blobBuilder.Dispose();
            }
            SystemAPI.SetComponent<DataLoadStateData>(this.SystemHandle, new DataLoadStateData { isLoadSuccessed = true });
            this.Enabled = false;
        }
    }

    // 清理 dt2, vt2 数据及其相应 meta 数据
    private void CleanDirectory()
    {
        // 获得目标文件夹内的所有DT2、VT2和相关的meta文件
        FileInfo[] files = new string[] { "*.DT2", "*.DT2.meta", "*.VT2", "*.VT2.meta" }.SelectMany(i => new DirectoryInfo(seismicDataPath).GetFiles(i)).ToArray();
        // 删除文件
        foreach (var f in files) f.Delete();
    }

    // AT2 数据文件分类到对应的文件夹
    private void ClassifyFile()
    {
        FileInfo[] files = new DirectoryInfo(seismicDataPath).GetFiles("*.AT2");

        foreach (var f in files)
        {
            string[] temp = f.Name.Split('_');
            var desDirectory = Directory.CreateDirectory(seismicDataPath + temp[0] + "_" + temp[1]);

#if UNITY_EDITOR
            // 移动 meta 文件,Build 后没有 meta 文件，只在Editor模式下删除
            File.Move(f.Directory.FullName + "/" + f.Name + ".meta", desDirectory.FullName + "/" + temp[temp.Length - 1] + ".meta");
#endif
            // 移动原文件
            f.MoveTo(desDirectory.FullName + "/" + temp[temp.Length - 1]);
        }
    }
}