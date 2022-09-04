using UnityEngine;
using Unity.Entities;

public class GameObjectCameraInject : MonoBehaviour
{
    void Update()
    {
        // 关联当前相机Transform 到 CameraSyncSystem
        foreach (World world in World.All)
        {
            CameraSyncSystem cameraSyncSystem = world.GetExistingSystem<CameraSyncSystem>();
            if (cameraSyncSystem != null)
            {
                cameraSyncSystem.CameraTransformInGO = this.transform;
                cameraSyncSystem.Enabled = true;
                Destroy(this);
            }
        }
    }
}
