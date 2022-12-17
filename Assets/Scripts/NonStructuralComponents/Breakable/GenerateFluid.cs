using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using Obi;
using Unity.Mathematics;

public class GenerateFluid : MonoBehaviour
{
    public GameObject fluidPrefab;

    private List<ObiParticleRenderer> tempList;

    private static float3 offset = new float3(0, 0.1f, 0);
    // Start is called before the first frame update
    void Start()
    {
        tempList = new List<ObiParticleRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (World world in World.All)
        {
            ReplaceSystem replaceSystem = world.GetExistingSystemManaged<ReplaceSystem>();
            if (replaceSystem != null && replaceSystem.fluidSolver == null)
            {
                replaceSystem.fluidSolver = this.gameObject;
            }
            while (replaceSystem != null && replaceSystem.fluidGeneratePositions.Count > 0)
            {
                // 获得最后一个元素并在原列表中删除
                var fluidInformation = replaceSystem.fluidGeneratePositions.Last();
                replaceSystem.fluidGeneratePositions.RemoveAt(replaceSystem.fluidGeneratePositions.Count - 1);
                // 生成流体并设置父物体以及位置和旋转角度
                var fluid = Instantiate(fluidPrefab);
                fluid.transform.parent = this.transform;
                fluid.transform.position = fluidInformation.position + offset;
                fluid.transform.rotation = fluidInformation.rotation;
                tempList.Add(fluid.GetComponent<ObiParticleRenderer>());
            }

            // 在相机中的 ObiFluidRenderer 组件中配置生成的新流体
            if (tempList.Count > 0)
            {
                var component = Camera.main.transform.GetComponent<ObiFluidRenderer>();
                // 合并原有数据
                tempList.AddRange(component.particleRenderers.ToList());
                // 重新赋值回原组件
                component.particleRenderers = tempList.ToArray();
                // 清空本地数据
                tempList.Clear();
            }
        }
    }

    public void RemoveAllFluidInGo()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        var component = Camera.main.transform.GetComponent<ObiFluidRenderer>();
        component.particleRenderers = new ObiParticleRenderer[0];
    }
}
