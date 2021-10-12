using UnityEngine;
using Unity.Entities;

public class GameObjectCameraInject : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        foreach (World world in World.All)
        {
            CameraSyncSystem cameraSyncSystem = world.GetExistingSystem<CameraSyncSystem>();
            if (cameraSyncSystem != null)
            {
                cameraSyncSystem.CameraGameObjectTransform = this.transform;
                cameraSyncSystem.Enabled = true;
                Destroy(this);
            }
        }
    }
}
