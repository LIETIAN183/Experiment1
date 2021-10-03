using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;

public class SetupBlobSystem : SystemBase
{
    public static List<BlobAssetReference<GroundMotionBlobAsset>> gmBlobRefs;
    public string groundMotionPath = Application.streamingAssetsPath + "/Data/";

    public int skipLine = 3;
    public float gravity = 9.81f;

    private float deltaTime;

    private List<float3> acc;

    protected override void OnStartRunning()
    {
        // 清理数据
        CleanDirectory();

        // 分类数据
        ClassifyFile();


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
            groundMotionBlobAsset.deltaTime = deltaTime;

            // 声明 Blob 资产引用
            BlobAssetReference<GroundMotionBlobAsset> groundMotionBlobAssetReference =
             blobBuilder.CreateBlobAssetReference<GroundMotionBlobAsset>(Allocator.Persistent);

            gmBlobRefs.Add(groundMotionBlobAssetReference);
            // 释放 BlobBuilder
            blobBuilder.Dispose();
        }

        ECSUIController.Instance.Setup();
    }

    protected override void OnUpdate()
    {
    }

    bool GetData(string folderName)
    {
        // 读取数据
        acc = GmDataReader.ReadFile(Application.streamingAssetsPath + "/Data/" + folderName + "/", skipLine, gravity, out deltaTime);
        // 判断读取数据是否正常
        if (acc == null)
        {
            Debug.Log("Read Acceleration Failed!!!");
            return false;
        }

        return true;
    }

    // 清理 dt2, vt2 数据及其相应 meta 数据
    void CleanDirectory()
    {
        DirectoryInfo path = new DirectoryInfo(groundMotionPath);
        FileInfo[] files = new string[] { "*.DT2", "*.DT2.meta", "*.VT2", "*.VT2.meta" }.SelectMany(i => path.GetFiles(i)).ToArray();

        foreach (var f in files)
        {
            f.Delete();
        }
    }

    // AT2 数据文件分类到对应的文件夹
    // TODO: build 后无法正确分类
    void ClassifyFile()
    {
        DirectoryInfo path = new DirectoryInfo(groundMotionPath);
        FileInfo[] files = path.GetFiles("*.AT2");

        foreach (var f in files)
        {
            string[] temp = f.Name.Split('_');
            var desDirectory = Directory.CreateDirectory(groundMotionPath + temp[0] + "_" + temp[1]);
            // 移动 meta 文件
            // File.Move(f.Directory.FullName + "/" + f.Name + ".meta", desDirectory.FullName + "/" + temp[temp.Length - 1] + ".meta");
            // 移动原文件
            f.MoveTo(desDirectory.FullName + "/" + temp[temp.Length - 1]);
        }
    }
}
