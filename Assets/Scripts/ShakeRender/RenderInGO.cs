using UnityEngine;
using Unity.Entities;

// tutorial:https://www.youtube.com/watch?v=JFv49-0vy_8&t=379s
[AddComponentMenu("Custom Authoring/Render In GO")]
public class RenderInGO : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject renderObject;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // 注意两个系统的物体必须重合首先
        // TODO: 计算货架弯曲程度
        RenderConfig config = renderObject.GetComponent<RenderConfig>();

        if (config == null)
        {
            config = renderObject.AddComponent<RenderConfig>();
        }

        config.positionOffset = renderObject.transform.position - transform.position;
        Debug.Log(transform.position);
        config.renderEntity = entity;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
