#region 程序集 Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Unity.Entities.dll
#endregion

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Entities
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FlowFieldSimulationSystemGroup))]
    public class AgentSimulationSystemGroup : ComponentSystemGroup
    {
        [Preserve]
        public AgentSimulationSystemGroup()
        {
            RateManager = new RateUtils.FixedRateCatchUpManager(0.02f);
        }
    }
}