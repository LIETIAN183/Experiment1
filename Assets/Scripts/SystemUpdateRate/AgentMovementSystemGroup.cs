#region 程序集 Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Unity.Entities.dll
#endregion

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Entities
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(AgentSimulationSystemGroup))]
    public partial class AgentMovementSystemGroup : ComponentSystemGroup
    {
    }
}