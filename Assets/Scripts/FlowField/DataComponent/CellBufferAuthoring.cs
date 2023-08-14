using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

// 实现 CellBuffer 通过 Inspector 挂载的功能
public class CellBufferAuthoring : MonoBehaviour
{
    class Baker : Baker<CellBufferAuthoring>
    {
        public override void Bake(CellBufferAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AddBuffer<CellBuffer>(entity);
        }
    }
}

// 单个网格的数据
public struct CellData
{
    // 网格中心点的三维世界坐标
    public float3 worldPos;
    // 网格在网格集合中的二维坐标
    public int2 gridIndex;
    // 网格的总代价
    public float localCost;
    // 网格的集成代价
    public float integrationCost;
    // 网格全局指导方向
    public float2 globalDir;
    // 网格局部指导方向
    public float2 localDir;

    // 用于计算 localCost 的辅助变量
    // 网格内障碍物的质量乘以相应危险系数
    public float massVariable;
    // 网格内障碍物的最高堆积高度
    public float maxHeight;
    // 网格内流体的数量
    public int fluidElementCount;
    // 判断该网格能否看见出口
    public bool seeExit;
}

// 存储 CellData 数据的动态数组
[InternalBufferCapacity(250)]
public struct CellBuffer : IBufferElementData
{
    public CellData cell;

    public static implicit operator CellData(CellBuffer cellBufferElement) => cellBufferElement.cell;

    public static implicit operator CellBuffer(CellData e) => new CellBuffer { cell = e };
}