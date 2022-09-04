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
    [UpdateAfter(typeof(AgentSimulationSystemGroup))]
    public class TrajectoryRecordSystemGroup : ComponentSystemGroup
    {
        [Preserve]
        public TrajectoryRecordSystemGroup()
        {
            RateManager = new RateUtils.FixedRateCatchUpManager(0.1f);
        }
    }
}