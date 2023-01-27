using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct FPSSystem : ISystem
{
    private const double fpsMeasurePeriod = 0.5f;
    private double m_FpsNextPeriod;
    private int m_FpsAccumulator;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.AddComponent<FPSData>(state.SystemHandle);
        m_FpsNextPeriod = SystemAPI.Time.ElapsedTime + fpsMeasurePeriod;
        m_FpsAccumulator = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = SystemAPI.GetSingleton<FPSData>();
        // measure average frames per second
        m_FpsAccumulator++;
        if (SystemAPI.Time.ElapsedTime > m_FpsNextPeriod)
        {
            data.curFPS = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
            SystemAPI.SetSingleton(data);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}
