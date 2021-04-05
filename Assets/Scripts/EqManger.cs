using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

// Read Earthquake Data
// use OdinInspector
// TODO: change Debug to Visulizable tips
public class EqManger : MonoBehaviour
{
    // 地震事件
    public UnityEvent startEarthquake { get; set; } = new UnityEvent();
    public UnityEvent endEarthquake { get; set; } = new UnityEvent();
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
        // 单例模式判断
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------Set GetData Parameter---------------------------------------------------------
    [TitleGroup("Read Data Settings")]
    public int skipLine = 3;
    [TitleGroup("Read Data Settings")]
    public float gravityValue = 9.81f;
    // 显示可选择的不同地震
    [ValueDropdown("EarthquakeFolders"), Required, TitleGroup("Read Data Settings")]
    public string folders = null;
#if UNITY_EDITOR // Editor-related code must be excluded from builds
#pragma warning disable // And these members are in fact being used, though the compiler cannot tell. Let's not have bothersome warnings.
    public IEnumerable<string> EarthquakeFolders()
    {
        return EqDataReader.EarthquakeFolders(Application.dataPath + "/Data/");
    }
#endif

    //--------------------------------------------Control Earthquake-----------------------------------------------------------------
    [TitleGroup("Control Earthquake")]
    // [PropertyOrder(4)]
    [ButtonGroup("Control Earthquake/Buttons")]
    // [ButtonGroup("Start Earthquake")]
    public void StartEq()
    {
        // 读取数据
        GetData();
        //开始地震模拟，激活Ground的Earthquake脚本
        startEarthquake.Invoke();
    }

    // [PropertyOrder(5)]
    // [Button("Stop Earthquake")]
    [ButtonGroup("Control Earthquake/Buttons")]
    public void EndEq()
    {
        endEarthquake.Invoke();
    }

    // 重置场景
    [Button("Restart")]
    public void ReLoad()
    {
        endEarthquake.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    //------------------------------------------------Show Data Parameter Read From File-------------------------------------------------------------
    [TitleGroup("Earthquake Data")]
    [ShowInInspector, ReadOnly]
    private int timeLength;
    [ShowInInspector, TitleGroup("Earthquake Data")]
    public List<Vector3> acceleration { get; private set; }

    void GetData()
    {
        //判断是否已经选择某个地震数据
        if (string.IsNullOrEmpty(folders))
        {
            Debug.Log("Select Earthquake First!!!");
            return;
        }

        // 读取数据
        acceleration = EqDataReader.ReadFile(new DirectoryInfo(Application.dataPath + "/Data/" + folders + "/"), skipLine, out timeLength);
        // 判断读取数据是否正常
        if (acceleration == null)
        {
            Debug.Log("Read Acceleration Failed!!!");
            return;
        }

        // 转换单位 从 g 转换为 m/s2 乘以重力加速度大小
        acceleration = acceleration.Select(a => a * gravityValue).ToList();
    }

    //---------------------------------------------------------Method-------------------------------------------------------------------------------
    public Vector3 GetAcc(int index)// Earthquake 脚本从中获得数据
    {
        return acceleration[index];
    }

    public int GetTime()
    {
        return this.timeLength;
    }

    // For Test
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