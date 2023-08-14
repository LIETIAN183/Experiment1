using Unity.Entities;

public struct DataLoadStateData : IComponentData
{
    // 标记数据是否读取成功
    public bool isLoadSuccessed;
}