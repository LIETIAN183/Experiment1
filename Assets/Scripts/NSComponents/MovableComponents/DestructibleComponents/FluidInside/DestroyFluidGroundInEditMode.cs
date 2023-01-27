using UnityEngine;

[ExecuteInEditMode]
public class DestroyFluidGroundInEditMode : MonoBehaviour
{
    // 在 EditMode 下执行，删除 SubScene Baking 阶段生成的 GameObject，因为该 GameObject 在退出 PlayMode 后不自动删除
    void Update()
    {
        var go = GameObject.Find("GroundInGo(Work in Play Mode)");
        while (go != null)
        {
            Object.DestroyImmediate(go);
            go = GameObject.Find("GroundInGo(Work in Play Mode)");
        }
    }
}
