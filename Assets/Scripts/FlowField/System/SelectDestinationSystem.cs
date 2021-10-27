using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;

public class SelectDestinationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 通过鼠标设置目标点
        if (Input.GetMouseButtonDown(0))
        {
            var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var RaycastInput = new RaycastInput
            {
                Start = screenRay.origin,
                End = screenRay.GetPoint(100),
                Filter = CollisionFilter.Default
            };
            World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CastRay(RaycastInput, out RaycastHit hit);
            float3 worldMousePos = hit.Position;

            //判断鼠标点击的点是否在网格内
            FlowFieldSettingData data = GetSingleton<FlowFieldSettingData>();
            if ((worldMousePos.x > data.originPoint.x && worldMousePos.x < data.originPoint.x + data.gridSize.x * 2 * data.cellRadius.x) &&
            (worldMousePos.z > data.originPoint.z && worldMousePos.z < data.originPoint.z + data.gridSize.y * 2 * data.cellRadius.z))
            {
                data.destination = worldMousePos;
                SetSingleton<FlowFieldSettingData>(data);
            }
        }
    }
}
