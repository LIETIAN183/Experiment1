using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;

public class SelectDestinationSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        // 通过鼠标设置目标点
        float3 worldMousePos = float3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            // worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var RaycastInput = new RaycastInput
            {
                Start = screenRay.origin,
                End = screenRay.GetPoint(100),
                Filter = CollisionFilter.Default
            };
            buildPhysicsWorld.PhysicsWorld.CastRay(RaycastInput, out RaycastHit hit);
            worldMousePos = hit.Position;
        }

        if (!worldMousePos.Equals(float3.zero))
        {
            FlowFieldSettingData data = GetSingleton<FlowFieldSettingData>();
            //判断鼠标点击的点是否在网格内
            if ((worldMousePos.x > data.originPoint.x && worldMousePos.x < data.originPoint.x + data.gridSize.x * 2 * data.cellRadius.x) &&
            (worldMousePos.z > data.originPoint.z && worldMousePos.z < data.originPoint.z + data.gridSize.y * 2 * data.cellRadius.z))
            {
                data.destination = worldMousePos;
                SetSingleton<FlowFieldSettingData>(data);
            }
        }
    }
}
