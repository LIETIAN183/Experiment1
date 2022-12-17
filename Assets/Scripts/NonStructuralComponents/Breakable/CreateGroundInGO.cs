using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Obi;

// 挂载该脚本的 Entity 不能放在 SubScene 内部
public class CreateGroundInGO : MonoBehaviour { }

public class CreateGroundInGOBaker : Baker<CreateGroundInGO>
{
    public override void Bake(CreateGroundInGO authoring)
    {
        // 创建物体并设置尺寸
        GameObject go = new GameObject("GroundInGo");

        go.transform.position = authoring.transform.position;
        go.transform.localScale = authoring.transform.localScale;

        // 配置 Collider
        var colliderInGo = go.AddComponent<BoxCollider>();
        var colliderWaitToRemove = authoring.GetComponent<BoxCollider>();
        colliderInGo.center = colliderWaitToRemove.center;
        colliderInGo.size = colliderWaitToRemove.size;

        // 配置 ObiCollider
        var obiColliderInGO = go.AddComponent<ObiCollider>();
        obiColliderInGO.sourceCollider = colliderInGo;

        // 删除原物体的 Collider 和 ObiCollider
        Object.DestroyImmediate(authoring.GetComponent<ObiCollider>());
        Object.DestroyImmediate(colliderWaitToRemove);
    }
}