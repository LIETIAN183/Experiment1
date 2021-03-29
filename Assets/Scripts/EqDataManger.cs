using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.SceneManagement;

// Read Earthquake Data
// use OdinInspector
[ExecuteInEditMode]
public class EqDataManger : MonoBehaviour
{

    // Set ReadData Parameter
    [PropertyOrder(1)]
    public int skipLine = 3;
    [PropertyOrder(2)]
    public float gravityValue = 9.81f;

    //--------------------------------------------Swith Earthquake Data--------------------------------------------------------------
    // 读取可选择的不同地震
    [PropertyOrder(3)]
    [ValueDropdown("EarthquakeData"), ShowInInspector]
    public string earthquakes = null;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
#pragma warning disable // And these members are in fact being used, though the compiler cannot tell. Let's not have bothersome warnings.
    private IEnumerable<string> EarthquakeData()
    {
        return EqDataReader.EarthquakeFolders(Application.dataPath + "/Data/");
    }
#endif
    //---------------------------------------------------End-----------------------------------------------------------------------


    //--------------------------------------------Control Earthquake-----------------------------------------------------------------
    [PropertyOrder(4)]
    [Button("Start Earthquake")]
    private void startEq()
    {
        //判断是否已经选择某个地震数据
        if (string.IsNullOrEmpty(earthquakes))
        {
            Debug.Log("Select Earthquake First!!!");
            return;
        }

        EqDataReader.ReadData(new DirectoryInfo(Application.dataPath + "/Data/" + earthquakes + "/"), skipLine, out timeLength, out acceleration);
        // 判断读取数据是否正常
        if (acceleration.Capacity == 0)
        {
            Debug.Log("Read Acceleration Failed!!!");
            return;
        }
        // 转换单位 从 g 转换为 m/s2 乘以重力加速度大小
        acceleration = acceleration.Select(a => a * gravityValue).ToList();

        //开始地震模拟，激活Ground的Earthquake脚本
        foreach (var eq in eqScripts)
        {
            eq.DataManager = this.transform;
            eq.enabled = true;
        }
    }

    [PropertyOrder(5)]
    [Button("Stop Earthquake")]
    public void stopEq()
    {
        foreach (var eq in eqScripts)
        {
            eq.GetComponent<Earthquake>().enabled = false;
        }
    }
    [Button("Restart")]
    private void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    //------------------------------------------------End----------------------------------------------------------------------------

    // Show Data Parameter Read From File
    [PropertyOrder(6)]
    public int timeLength;
    [PropertyOrder(7)]
    public List<Vector3> acceleration;

    public void getData(out List<Vector3> acceleration, out int timeLength)
    {
        acceleration = this.acceleration;
        timeLength = this.timeLength;
    }

    // 初始化这时 Earthquake 脚本
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        init();
    }
    private Earthquake[] eqScripts;
    void init()
    {
        eqScripts = GameObject.FindGameObjectsWithTag("Ground").Select(g => g.GetComponent<Earthquake>()).ToArray();
        foreach (var eq in eqScripts)
        {
            eq.enabled = false;
        }
    }
}
