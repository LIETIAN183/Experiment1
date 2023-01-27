using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;

public partial class SelectDestinationSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        // 通过鼠标设置目标点
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main.Equals(null)) return;
            var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var RaycastInput = new RaycastInput
            {
                Start = screenRay.origin,
                End = screenRay.GetPoint(100),
                Filter = CollisionFilter.Default
            };
            SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.CastRay(RaycastInput, out RaycastHit hit);
            float3 worldMousePos = hit.Position;

            //判断鼠标点击的点是否在网格内
            FlowFieldSettingData data = SystemAPI.GetSingleton<FlowFieldSettingData>();
            if ((worldMousePos.x > data.originPoint.x && worldMousePos.x < data.originPoint.x + data.gridSetSize.x * 2 * data.cellRadius.x) &&
            (worldMousePos.z > data.originPoint.z && worldMousePos.z < data.originPoint.z + data.gridSetSize.y * 2 * data.cellRadius.z))
            {
                data.destination = worldMousePos;
                SystemAPI.SetSingleton<FlowFieldSettingData>(data);
            }
        }
    }
}
