using UnityEngine;

// TODO: 创建一个移动 父物体 pivot 到子物体几何中心的脚本，父物体只是一个用来包括子物体的空物体
// TODO: 提升物体运动效果
// [ExecuteInEditMode]
namespace Project.Deprecated
{
    public class ComponentMove : MonoBehaviour
    {
        public GroundMove ground;
        public Rigidbody[] rbs;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            // 获取质心
            rbs = GetComponentsInChildren<Rigidbody>();
            Physics.SyncTransforms();
            foreach (var rb in rbs)
            {
                rb.ResetCenterOfMass();
            }
        }

        // Start is called before the first frame update
        void Start()
        {

            // 注册监听事件
            EqManger.Instance.startEarthquake.AddListener(() => this.enabled = true);
            EqManger.Instance.endEarthquake.AddListener(() => this.enabled = false);
            this.enabled = false;
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            // 地震对物体施加力
            foreach (var rb in rbs)
            {
                rb.AddForceAtPosition(ground.currentAcceleration * rb.mass, rb.centerOfMass, ForceMode.Force);
                // rb.AddForce(ground.currentAcceleration * rb.mass, ForceMode.Force);
            }
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's
        /// context menu or when adding the component the first time.
        /// </summary>
        void Reset()
        {
            ground = GameObject.Find("Ground").GetComponent<GroundMove>();
        }
    }
}