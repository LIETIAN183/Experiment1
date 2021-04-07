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

    // TODO: ADD 重新载入目录的选项
    public IEnumerable<string> EarthquakeFolders()
    {
        return EqDataReader.EarthquakeFolders(Application.dataPath + "/Data/");
    }

    //--------------------------------------------Control Earthquake-----------------------------------------------------------------
    [TitleGroup("Control Earthquake")]
    // [PropertyOrder(4)]
    [ButtonGroup("Control Earthquake/Buttons")]
    // [ButtonGroup("Start Earthquake")]
    public void StartEq()
    {
        // 读取数据
        if (!GetData())
        {
            return;//读取数据失败，不开始地震仿真
            // TODO: 报错提示
        }
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

    bool GetData()
    {
        //判断是否已经选择某个地震数据
        if (string.IsNullOrEmpty(folders))
        {
            Debug.Log("Select Earthquake First!!!");
            return false;
        }

        // 读取数据
        acceleration = EqDataReader.ReadFile(new DirectoryInfo(Application.dataPath + "/Data/" + folders + "/"), skipLine, out timeLength);
        // 判断读取数据是否正常
        if (acceleration == null)
        {
            Debug.Log("Read Acceleration Failed!!!");
            return false;
        }

        // 转换单位 从 g 转换为 m/s2 乘以重力加速度大小
        acceleration = acceleration.Select(a => a * gravityValue).ToList();
        return true;
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

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's
    /// context menu or when adding the component the first time.
    /// </summary>
    void Reset()
    {
        skipLine = 3;
        gravityValue = 9.81f;
        folders = null;
        timeLength = 0;
        acceleration = null;

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