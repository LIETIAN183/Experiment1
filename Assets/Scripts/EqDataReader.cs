using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public static class EqDataReader
{

    public static IEnumerable<string> EarthquakeFolders(string directoryPath)
    {
        try
        {
            // 读取Data文件夹下的每个地震数据文件夹
            DirectoryInfo dataDir = new DirectoryInfo(directoryPath);
            DirectoryInfo[] dirs = dataDir.GetDirectories();
            return dirs.Select(dir => dir.Name).ToArray();// 提取DirectoryInfo中的文件夹名字property，创建新数组
        }
        catch (System.Exception e)
        {
            Debug.Log($"{e}");
            throw;
        }
    }

    // Read Earthquake Data from Specific File
    public static void ReadData(DirectoryInfo folderPath, int skipLine, out int timeLength, out List<Vector3> acceleration)
    {
        // Init variable
        acceleration = new List<Vector3>();
        timeLength = int.MaxValue;      // 设置时间长度为最大，从而找出数据中时间的最短值
        string line;                    // 存储每一行的字符串
        string temp;                    // 临时变量，存储判断的角度值
        string[] linedata;              // 存储分割空格后的字符串形式的数据数组
        int count;                      // 辅助计算跳过开头的行数
        Vector3 angle;                  // 存储加速度数据的角度 Vector
        int horizontalAngel;            // 存储水平平面上绕y轴旋转的度数

        // 获取目录下的所有 txt 文件
        FileInfo[] files;
        try
        {
            files = folderPath.GetFiles("*.txt");
        }
        catch (System.Exception e)
        {
            Debug.Log($"{e}");
            throw;
        }
        // 若文件下内无 txt 文件，返回 NULL
        if (files.Count().Equals(0))
        {
            Debug.Log("No TXT File In Current Directory!!!");
            timeLength = 0;
            return;
        }

        // 读取数据
        foreach (var file in files)
        {
            // 读取txt标题中标注的角度
            // 用于与加速度相乘，得到加速度矢量
            angle = Vector3.zero;
            temp = file.Name.Substring(file.Name.Length - 7, 3);
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
                count = skipLine;
                do
                {
                    line = reader.ReadLine();
                } while (count-- > 0);

                // 读取点的个数，并更新最小值
                Regex r = new Regex(@"(\d{4})");
                Match m = r.Match(line);
                if (string.IsNullOrEmpty(m.Groups[0].Value))
                {
                    Debug.Log("Please check the skipLine!!!");
                    timeLength = 0;
                    return;
                }
                int number = int.Parse(m.Groups[0].Value);
                timeLength = number < timeLength ? number : timeLength;

                // 读取加速度值，第一次List缺少空间，用try catch捕获异常，从而实现数据的添加
                // 可能该方法并不好，尝试未来使用其他方式
                // TODO: use another to save data, notice the first time List is lack of capacity
                line = reader.ReadLine();
                while (line != null)
                {
                    //分割字符串为字符串数据数组
                    linedata = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var str in linedata)
                    {
                        try
                        {
                            acceleration[count++] += angle * Convert.ToSingle(str);
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log($"{e.GetType()}");
                            continue;
                        }
                        finally
                        {
                            acceleration.Add(angle * Convert.ToSingle(str));
                        }
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
            }
        }
        // return acceleration;
    }
}