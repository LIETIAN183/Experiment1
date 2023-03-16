using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using Obi;
using Unity.Mathematics;
using Drawing;

// 经测试，0.5mx0.5mx0.5m= 0.125m^3 需要 obi 粒子15w个 即1m^3 需要120w个 液体视作水，则120w粒子质量为1000kg，则1200个粒子1kg，体积1L，则单个粒子0.83g
// 由于传输粒子坐标10取1，则单个传输坐标包含10个粒子，即8.3g每个坐标=0.0083kg
[RequireComponent(typeof(ObiSolver))]
public class FluidGOManager : MonoBehaviour
{
    // 待生成流体的预制体
    public GameObject fluidPrefab;

    // 生成流体位置偏移
    private static readonly float3 offset = new float3(0, 0.1f, 0);
    // 辅助变量
    private EntityManager entityManager;
    private EntityQuery fluidQuery;
    // 渲染配置变量
    private List<ObiFluidRenderer> renderers;
    private List<ObiParticleRenderer> particleList;
    // 获取全局流体粒子坐标
    private ObiSolver solver;

    private List<float2> fluidPosList;
    // Start is called before the first frame update
    void Start()
    {
        particleList = new List<ObiParticleRenderer>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        fluidQuery = entityManager.CreateEntityQuery(typeof(FluidInfoBuffer));
        solver = GetComponent<ObiSolver>();
        renderers = new List<ObiFluidRenderer>();
        fluidPosList = new List<float2>();
        //  设置 0.5f 启动一次，因为路径算法也是0.5f启动一次
        InvokeRepeating("GetAllFluidPostion", 0, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (fluidQuery.IsEmpty)
        {
            fluidQuery = entityManager.CreateEntityQuery(typeof(FluidInfoBuffer));
        }

        GenerateFluid();

        // using (Draw.ingame.WithColor(Color.green))
        // {
        //     foreach (var item in fluidPosList)
        //     {
        //         Draw.ingame.WireSphere(new float3(item.x, 0, item.y), 0.1f);
        //     }
        // }

        AssingToFluidRender();

        ClearEventCheck();
    }

    public void GenerateFluid()
    {
        var list = fluidQuery.GetSingletonBuffer<FluidInfoBuffer>().Reinterpret<fluidInfo>();
        while (list.Length > 0)
        {
            // 获得最后一个元素并在原列表中删除
            var lastInfo = list[list.Length - 1];
            list.RemoveAt(list.Length - 1);
            // 生成流体并设置父物体以及位置和旋转角度
            var fluid = Instantiate(fluidPrefab);
            fluid.transform.parent = this.transform;
            fluid.transform.position = lastInfo.position + offset;
            fluid.transform.rotation = lastInfo.rotation;
            particleList.Add(fluid.GetComponent<ObiParticleRenderer>());
        }
    }

    // 在相机中的 ObiFluidRenderer 组件中配置生成的新流体
    public void AssingToFluidRender()
    {
        if (renderers.Count == 0)
        {
            var cameraQuery = entityManager.CreateEntityQuery(typeof(CameraRefData));
            var cameraRef = cameraQuery.GetSingleton<CameraRefData>();
            renderers.Add(cameraRef.mainCamera.GetComponent<ObiFluidRenderer>());
            renderers.Add(cameraRef.overHeadCamera.GetComponent<ObiFluidRenderer>());
        }

        if (particleList.Count > 0)
        {
            var component = renderers[0];
            // 合并原有数据
            particleList.AddRange(component.particleRenderers.ToList());
            // 重新赋值回原组件
            component.particleRenderers = particleList.ToArray();

            renderers[1].particleRenderers = particleList.ToArray();
            // 清空本地数据
            particleList.Clear();
        }
    }

    public void ClearEventCheck()
    {
        var clearEntity = fluidQuery.GetSingletonEntity();
        var clearEvent = entityManager.GetComponentData<ClearFluidEvent>(clearEntity);
        if (clearEvent.isActivate)
        {
            fluidQuery.GetSingletonBuffer<FluidInfoBuffer>().Reinterpret<fluidInfo>().Clear();
            particleList.Clear();
            RemoveAllFluidInGo();
            clearEvent.isActivate = false;
            entityManager.SetComponentData<ClearFluidEvent>(clearEntity, clearEvent);
        }
    }

    public void RemoveAllFluidInGo()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var render in renderers)
        {
            render.particleRenderers = new ObiParticleRenderer[0];
        }
    }

    public void GetAllFluidPostion()
    {
        var fluidEntity = fluidQuery.GetSingletonEntity();
        var pos2DBuffer = entityManager.GetBuffer<Pos2DBuffer>(fluidEntity);

        pos2DBuffer.Clear();
        for (int i = 0; i < solver.positions.count; i += 10)
        {
            var pos = transform.TransformPoint(solver.positions.GetVector3(i));
            // 存在 (0,0.44f,0)相对 Obisolver 世界坐标的错误点位置，因此需要剔除
            // if (pos.x == transform.position.x && pos.z == transform.position.z && math.abs(pos.y - transform.position.y - 0.44f) < 0.01f)
            // {
            //     continue;
            // }
            // 简化版本
            if (pos.y > 0.4f) continue;
            var temp = new float2(pos.x, pos.z);
            foreach (var item in fluidPosList)
            {
                if (math.lengthsq(temp - item) < 0.01f) break;
            }
            pos2DBuffer.Add(temp);
        }
    }
}