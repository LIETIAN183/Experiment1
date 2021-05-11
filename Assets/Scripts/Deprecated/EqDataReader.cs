using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Project.Deprecated
{
    // TODO: 修改为 Resource 加载数据，优化资源存储方式 修改为 Resource Manger
    public static class EqDataReader
    {

        // 读取可选的仿真地震选项
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
        public static List<Vector3> ReadFile(DirectoryInfo folderPath, int skipLine, out int timeLength)
        {
            timeLength = int.MaxValue;      // 设置时间长度为最大，从而找出数据中时间的最短值

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
                return null;
            }

            List<Vector3> acceleration = new List<Vector3>();// 动态数组

            // 辅助变量
            string line;                    // 存储每一行的字符串
            string[] linedata;              // 存储分割空格后的字符串形式的数据数组
            Vector3 angle;                  // 存储加速度数据的角度 Vector
                                            // 读取数据
            foreach (var file in files)
            {
                // 读取txt标题中标注的角度
                // 用于与加速度相乘，得到加速度矢量
                angle = Vector3.zero;
                // 存储获得的字符串形式的角度
                string degreeStr = file.Name.Substring(file.Name.Length - 7, 3);
                // Debug.Log(degreeStr);
                if (degreeStr.Contains("UP"))
                {
                    angle = Vector3.up;
                }
                else
                {
                    // Quaternion * Vector3 work, Vector3 * Quaternion not work
                    angle = Quaternion.AngleAxis(int.Parse(degreeStr), Vector3.up) * Vector3.forward;
                    // Debug.Log(angle);
                }

                // 读取文件
                using (StreamReader reader = file.OpenText())
                {
                    // 跳过读取前skipLine行
                    int count = skipLine;
                    do
                    {
                        line = reader.ReadLine();
                        // Debug.Log(count);
                    } while (count-- > 0);
                    // Debug.Log(count);

                    // 读取点的个数，并更新最小值
                    Regex r = new Regex(@"(\d{4})");
                    Match m = r.Match(line);
                    if (string.IsNullOrEmpty(m.Groups[0].Value))
                    {
                        Debug.Log("Please check the skipLine!!!");
                        timeLength = 0;
                        return null;
                    }
                    int number = int.Parse(m.Groups[0].Value);
                    // Debug.Log(number);
                    timeLength = number < timeLength ? number : timeLength;

                    // TODO: use another way to save data, notice the first time List is lack of capacity
                    // Debug.Log(count);
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

                                acceleration.Add(angle * Convert.ToSingle(str));
                            }
                            else
                            {
                                acceleration[count] += angle * Convert.ToSingle(str);
                            }
                            ++count;
                        }
                        line = reader.ReadLine();
                    }
                    reader.Close();
                }
            }
            return acceleration;
        }
    }
}