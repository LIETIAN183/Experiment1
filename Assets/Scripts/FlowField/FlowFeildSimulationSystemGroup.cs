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
    public class FlowFieldSimulationSystemGroup : ComponentSystemGroup
    {
        [Preserve]
        public FlowFieldSimulationSystemGroup()
        {
            FixedRateManager = new FixedRateUtils.FixedRateCatchUpManager(0.5f);
        }
    }
}