using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
// Read Earthquake Data
// [ExecuteInEditMode]
// use OdinInspector
public class EqDataReader : MonoBehaviour
{

    [PropertyOrder(0)]
    [Button("Start Earthquake")]
    private void startEq()
    {
        //判断是否已经选择某个地震数据
        if (string.IsNullOrEmpty(earthquakes))
        {
            Debug.Log("Select Earthquake First!!!");
            return;
        }

        ReadData(new DirectoryInfo(Application.dataPath + "/Data/" + earthquakes + "/"));

        //开始地震模拟，激活Ground的Earthquake脚本
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
        foreach (var g in grounds)
        {
            g.GetComponent<Earthquake>().enabled = true;
        }
    }

    [PropertyOrder(1)]
    [ValueDropdown("EarthquakeData"), ShowInInspector]
    public string earthquakes = null;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
#pragma warning disable // And these members are in fact being used, though the compiler cannot tell. Let's not have bothersome warnings.
    private IEnumerable<string> EarthquakeData()
    {
        try
        {
            // 读取Data文件夹下的每个地震数据文件夹
            DirectoryInfo dataDir = new DirectoryInfo(Application.dataPath + "/Data/");
            DirectoryInfo[] dirs = dataDir.GetDirectories();
            return dirs.Select(dir => dir.Name).ToArray();// 提取DirectoryInfo中的文件夹名字property，创建新数组
        }
        catch (System.Exception e)
        {
            Debug.Log($"{e}");
            throw;
        }

    }
#endif

    // [ShowInInspector]
    public List<Vector3> acceleration;
    public int skipLine = 3;
    public int timeLength;
    public float gravityValue = 9.81f;


    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        // x = ReadData(xFilePath);
        // y = ReadData(yFilePath);
        // z = ReadData(zFilePath);
    }
    void Start()
    {

    }

    //读取地震时程数据
    void ReadData(DirectoryInfo folderPath)
    {
        FileInfo[] files = folderPath.GetFiles("*.txt");

        timeLength = int.MaxValue;
        string line, temp;
        string[] linedata;
        int count;
        Vector3 angle;
        int horizontalAngel;

        foreach (var file in files)
        {
            // 读取txt标题中标注的角度
            // 用于与加速度相乘
            angle = Vector3.zero;
            count = skipLine;
            temp = file.Name.Substring(file.Name.Length - 7, 3);
            Debug.Log(temp);
            if (temp.Contains("UP"))
            {
                angle = new Vector3(0, 1, 0);
            }
            else
            {
                horizontalAngel = int.Parse(temp);
                // Quaternion * Vector3 work, Vector3 * Quaternion not work
                angle = Quaternion.AngleAxis(horizontalAngel, Vector3.up) * Vector3.forward;
            }

            // 读取文件
            using (StreamReader reader = file.OpenText())
            {
                // 跳过读取前skipLine行
                do
                {
                    line = reader.ReadLine();
                } while (count-- > 0);

                // 读取点的个数，并选择最小值
                Regex r = new Regex(@"(\d{4})");
                Match m = r.Match(line);
                int number = int.Parse(m.Groups[0].Value);
                timeLength = number < timeLength ? number : timeLength;
                line = reader.ReadLine();

                // 读取加速度值，第一次List缺少空间，用try catch捕获异常，从而实现数据的添加
                // 可能该方法并不好，尝试未来使用其他方式
                // TODO: use another to save data, notice the first time List is lack of capacity
                while (line != null)
                {
                    linedata = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var str in linedata)
                    {
                        try
                        {
                            acceleration[count++] += angle * Convert.ToSingle(str) * gravityValue;
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log($"{e.GetType()}");
                            continue;
                        }
                        finally
                        {
                            acceleration.Add(angle * Convert.ToSingle(str) * gravityValue);
                        }
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
            }
        }
        // List<double> datas = new List<double>();



    }
    // List<double> ReadData(string filePath)
    // {
    //     List<double> datas = new List<double>();

    //     string line;
    //     string[] linedata;
    //     int count = skipReadLine;

    //     using (StreamReader reader = new StreamReader(filePath))
    //     {
    //         line = reader.ReadLine();
    //         while (count-- > 0)
    //         {
    //             line = reader.ReadLine();
    //         }

    //         while (line != null)
    //         {
    //             linedata = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //             foreach (var str in linedata)
    //             {
    //                 datas.Add(Convert.ToDouble(str) * gravityValue);
    //             }
    //             line = reader.ReadLine();
    //         }
    //         reader.Close();
    //     }
    //     return datas;
    // }
}
