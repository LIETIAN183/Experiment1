using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
// Read Earthquake Data
// use OdinInspector

public class EqManger : MonoBehaviour
{
    // 地震事件
    public UnityEvent StartEarthquake { get; set; } = new UnityEvent();
    public UnityEvent EndEarthquake { get; set; } = new UnityEvent();
    // 单例模式
    public static EqManger Instance
    {
        get; private set;
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        init();
    }

    // -------------------------------------------------Set ReadData Parameter---------------------------------------------------------
    [TitleGroup("Read Data Settings")]
    public int skipLine = 3;
    public float gravityValue = 9.81f;




    //--------------------------------------------Select Specific Earthquake Data--------------------------------------------------------------
    // 显示可选择的不同地震
    // [PropertyOrder(3)]
    [ValueDropdown("EarthquakeData"), Required]
    public string earthquakes = null;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
#pragma warning disable // And these members are in fact being used, though the compiler cannot tell. Let's not have bothersome warnings.
    private IEnumerable<string> EarthquakeData()
    {
        return EqDataReader.EarthquakeFolders(Application.dataPath + "/Data/");
    }
#endif

    //--------------------------------------------Control Earthquake-----------------------------------------------------------------
    [TitleGroup("Control Earthquake")]
    // [PropertyOrder(4)]
    [ButtonGroup("Control Earthquake/Buttons")]
    // [ButtonGroup("Start Earthquake")]
    public void startEq()
    {
        //判断是否已经选择某个地震数据
        if (string.IsNullOrEmpty(earthquakes))
        {
            Debug.Log("Select Earthquake First!!!");
            return;
        }

        // 读取数据
        acceleration = EqDataReader.ReadData(new DirectoryInfo(Application.dataPath + "/Data/" + earthquakes + "/"), skipLine, out timeLength);
        // 判断读取数据是否正常
        if (acceleration == null)
        {
            Debug.Log("Read Acceleration Failed!!!");
            return;
        }

        // 转换单位 从 g 转换为 m/s2 乘以重力加速度大小
        acceleration = acceleration.Select(a => a * gravityValue).ToList();

        // 开始计时
        Counter.Instance.enabled = true;
        //开始地震模拟，激活Ground的Earthquake脚本
        StartEarthquake.Invoke();
    }

    // [PropertyOrder(5)]
    // [Button("Stop Earthquake")]
    [ButtonGroup("Control Earthquake/Buttons")]
    public void stopEq()
    {
        EndEarthquake.Invoke();
        Counter.Instance.enabled = false;// UI 界面暂停时停止 Counter
    }

    // 重置场景
    [Button("Restart")]
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    //------------------------------------------------Show Data Parameter Read From File-------------------------------------------------------------
    [TitleGroup("Earthquake Data")]
    [ShowInInspector, ReadOnly]
    private int timeLength;
    [ShowInInspector]
    public List<Vector3> acceleration { get; private set; }



    //---------------------------------------------------------Method-------------------------------------------------------------------------------
    // TODO: 考虑加速度数据传输后可能被 Earthquake 脚本修改
    public Vector3 getAcc(int index)// Earthquake 脚本从中获得数据
    {
        return acceleration[index];
    }

    public int getTime()
    {
        return this.timeLength;
    }
    // 获取所有的地震脚本并设置为未激活状态
    private GroundMove[] eqScripts;
    void init()
    {
        // StartEarthquake = new UnityEvent();
        // EndEarthquake = new UnityEvent();
        Counter.Instance.StopEarthquake.AddListener(stopEq);
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    // void Start()
    // {
    //     // earthquakes = EqDataReader.EarthquakeFolders(Application.dataPath + "/Data/")[0];
    //     earthquakes = "RSN1063_NORTHR";
    //     startEq();
    // }
}