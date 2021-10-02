using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Mathematics;

public static class GmDataReader
{
    public static readonly float3 forward = new float3(0, 0, 1);

    // 读取可选的仿真地震选项
    public static IEnumerable<string> GroundMotionFolders(string directoryPath)
    {
        try
        {
            // 读取Data文件夹下的每个地震数据文件夹
            DirectoryInfo dataDir = new DirectoryInfo(directoryPath);
            DirectoryInfo[] dirs = dataDir.GetDirectories();
            return dirs.Select(dir => dir.Name);// 提取DirectoryInfo中的文件夹名字property，创建新数组
        }
        catch (System.Exception e)
        {
            Debug.Log($"{e}");
            throw;
        }
    }

    // Read Earthquake Data from Specific File
    public static List<float3> ReadFile(string gmPath, int skipLine, float gravity)
    {
        DirectoryInfo folderPath = new DirectoryInfo(gmPath);
        // 获取目录下的所有 txt 文件
        FileInfo[] files;

        // 读取文件夹内的文件
        try
        {
            files = folderPath.GetFiles("*.txt");
        }
        catch (System.Exception e)
        {
            Debug.Log($"{e}");
            return null;
        }
        // 若文件下内无 txt 文件，返回 NULL
        if (files.Count().Equals(0))
        {
            Debug.Log("No TXT File In Current Directory!!!");
            return null;
        }

        List<float3> acceleration = new List<float3>();// 动态数组

        // 辅助变量
        string line;                    // 存储每一行的字符串
        string[] linedata;              // 存储分割空格后的字符串形式的数据数组
        float3 degree;                  // 存储加速度数据的角度 Vector
        // 读取数据
        foreach (var file in files)
        {
            // 读取txt标题中标注的角度
            // 用于与加速度相乘，得到加速度矢量
            degree = float3.zero;
            // 存储获得的字符串形式的角度
            string degreeStr = file.Name.Substring(file.Name.Length - 7, 3);
            // Debug.Log(degreeStr);
            if (degreeStr.Contains("UP"))
            {
                degree = math.up();
            }
            else
            {
                degree = math.mul(quaternion.AxisAngle(math.up(), math.radians(int.Parse(degreeStr))), math.forward());
            }

            // 读取文件
            using (StreamReader reader = file.OpenText())
            {
                // 跳过读取前skipLine行
                int count = skipLine;
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
                    return null;
                }
                int number = int.Parse(m.Groups[0].Value);

                count = 0;
                line = reader.ReadLine();
                while (line != null)
                {
                    //分割字符串为字符串数据数组
                    linedata = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var str in linedata)
                    {
                        // 初始时 List 没有空间，只能使用Add函数添加
                        if (count >= acceleration.Count)
                        {

                            acceleration.Add(degree * Convert.ToSingle(str));
                        }
                        else
                        {
                            acceleration[count] += degree * Convert.ToSingle(str);
                        }
                        ++count;
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
            }
        }

        // 转换单位 从 g 转换为 m/s2 乘以重力加速度大小
        return acceleration.Select(a => a * gravity).ToList();
    }
}