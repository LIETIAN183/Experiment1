using Unity.Entities;

public partial class FPSSystem : SystemBase
{
    const double fpsMeasurePeriod = 0.5f;
    private double m_FpsNextPeriod = 0;
    private int m_FpsAccumulator = 0;

    public int curFPS { get; private set; }

    protected override void OnStartRunning() => m_FpsNextPeriod = Time.ElapsedTime + fpsMeasurePeriod;

    protected override void OnUpdate()
    {
        // measure average frames per second
        m_FpsAccumulator++;
        if (Time.ElapsedTime > m_FpsNextPeriod)
        {
            curFPS = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
        }
    }
}
