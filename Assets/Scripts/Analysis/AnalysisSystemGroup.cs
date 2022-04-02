#region 程序集 Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Unity.Entities.dll
#endregion

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Entities
{
    [ExecuteAlways]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    // [UpdateAfter(typeof(FlowFieldSimulationSystemGroup))]
    public class AnalysisSystemGroup : ComponentSystemGroup
    {
        [Preserve]
        public AnalysisSystemGroup()
        {
            FixedRateManager = new FixedRateUtils.FixedRateCatchUpManager(1f);
        }
    }
}