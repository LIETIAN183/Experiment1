using UnityEngine;
using Unity.Entities;

// tutorial:https://www.youtube.com/watch?v=JFv49-0vy_8&t=379s
[AddComponentMenu("Custom Authoring/ECS Sync Setting")]
public class ECSSyncSetting : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject dataInGO;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        GOSyncSetting setting = dataInGO.GetComponent<GOSyncSetting>();

        if (setting == null)
        {
            setting = dataInGO.AddComponent<GOSyncSetting>();
        }

        setting.dataInEntity = entity;
    }
}
