using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SetupBlobSystem : SystemBase
{
    public static List<BlobAssetReference<GroundMotionBlobAsset>> gmBlobRefs;
    public string groundMotionPath = Application.streamingAssetsPath + "/Data/";

    public int skipLine = 3;
    public float gravity = 9.81f;

    private List<float3> acc;

    protected override void OnStartRunning()
    {
        // 读取数据并存储在 BlobAsset 中
        gmBlobRefs = new List<BlobAssetReference<GroundMotionBlobAsset>>();
        // 获得所有可选 GroundMotion 的名字
        var gms = GmDataReader.GroundMotionFolders(groundMotionPath);

        // 遍历每个文件夹内的文件
        foreach (var item in gms)
        {
            if (!GetData(item))
            {
                break;
            }

            // 创建 BlobBuilder，赋值 gmArray 和 gmName
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);

            // Blob 声明资产类型
            ref GroundMotionBlobAsset groundMotionBlobAsset = ref blobBuilder.ConstructRoot<GroundMotionBlobAsset>();
            BlobBuilderArray<GroundMotion> gmArray = blobBuilder.Allocate(ref groundMotionBlobAsset.gmArray, acc.Count);

            // 存储加速度数据为 BlobArray
            for (int i = 0; i < acc.Count; ++i)
            {
                gmArray[i] = new GroundMotion { acceleration = acc[i] };
            }

            // 存储相应地震名字为 BlobString
            blobBuilder.AllocateString(ref groundMotionBlobAsset.gmName, item);

            // 声明 Blob 资产引用
            BlobAssetReference<GroundMotionBlobAsset> groundMotionBlobAssetReference =
             blobBuilder.CreateBlobAssetReference<GroundMotionBlobAsset>(Allocator.Persistent);

            gmBlobRefs.Add(groundMotionBlobAssetReference);
            // 释放 BlobBuilder
            blobBuilder.Dispose();
        }

        // ECSUIController.Instance.Setup();
    }

    protected override void OnUpdate()
    {
    }

    bool GetData(string folderName)
    {
        // 读取数据
        acc = GmDataReader.ReadFile(Application.streamingAssetsPath + "/Data/" + folderName + "/", skipLine, gravity);
        // 判断读取数据是否正常
        if (acc == null)
        {
            Debug.Log("Read Acceleration Failed!!!");
            return false;
        }

        return true;
    }
}
