using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using Obi;
using Unity.Mathematics;

// TODO: 获得所有 Fluid 位置
public class FluidGOManager : MonoBehaviour
{
    public GameObject fluidPrefab;

    private static readonly float3 offset = new float3(0, 0.1f, 0);

    private EntityManager entityManager;
    private EntityQuery fluidQuery;

    private List<ObiFluidRenderer> renderers;
    private List<ObiParticleRenderer> particleList;
    // Start is called before the first frame update
    void Start()
    {
        particleList = new List<ObiParticleRenderer>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        fluidQuery = entityManager.CreateEntityQuery(typeof(FluidInfoBuffer));
        // clearQuery = entityManager.CreateEntityQuery(typeof(ClearFluidEvent));
        renderers = new List<ObiFluidRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fluidQuery.IsEmpty)
        {
            fluidQuery = entityManager.CreateEntityQuery(typeof(FluidInfoBuffer));
        }

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


        assingToFluidRender();

        ClearEventCheck();
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

    // 在相机中的 ObiFluidRenderer 组件中配置生成的新流体
    public void assingToFluidRender()
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
}