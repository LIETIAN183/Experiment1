using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Mathematics;

public static class DataReaderUtil
{
    // 读取可选的仿真地震选项
    public static IEnumerable<string> SeismicEventFolders(string directoryPath)
    {
        // 提取DirectoryInfo中的文件夹名字property，创建新数组
        DirectoryInfo[] dirs = new DirectoryInfo(directoryPath).GetDirectories();
        return dirs.Select(dir => dir.Name);
    }

    // Read Earthquake Data from Specific File
    public static List<float3> ReadFile(string eventFolderPath, float gravity, out float dataDeltaTime)
    {
        dataDeltaTime = 0;
        // 获取文件夹内的所有 AT2 文件
        FileInfo[] files = new DirectoryInfo(eventFolderPath).GetFiles("*.AT2");
        // 文件夹内无 AT2 文件 
        if (files.Length == 0) { return null; }

        List<float3> acceleration = new List<float3>();// 动态数组
        // 读取数据
        foreach (var file in files)
        {
            // 读取AT2标题中标注的角度
            // 用于与加速度相乘，得到加速度矢量
            float3 degree = float3.zero;
            // 存储获得的字符串形式的角度
            string degreeStr = file.Name.Substring(file.Name.Length - 7, 3);
            if (degreeStr.Contains("UP")) { degree = math.up(); }
            else if (degreeStr.Contains("DOWN")) { degree = math.down(); }
            else
            {
                degree = math.mul(quaternion.AxisAngle(math.up(), math.radians(int.Parse(degreeStr))), math.forward());
            }

            // 辅助变量
            string line;                    // 存储每一行的字符串
            string[] linedata;              // 存储分割空格后的字符串形式的数据数组

            // 读取文件
            using (StreamReader reader = file.OpenText())
            {
                do
                {
                    line = reader.ReadLine();
                } while (!line.Contains("NPTS"));

                // 读取数据点个数与数据点间的时间间隔
                Regex r = new Regex(@"[0-9.]+");
                MatchCollection ms = r.Matches(line);
                // ms0 存储点个数， ms1 存储时间间隔
                int number = int.Parse(ms[0].Groups[0].Value);
                dataDeltaTime = float.Parse(ms[1].Groups[0].Value);
                // 提前声明空间
                if (acceleration.Count < number) acceleration.AddRange(Enumerable.Repeat(float3.zero, number - acceleration.Count));

                int count = 0;
                line = reader.ReadLine();
                while (line != null)
                {
                    //分割字符串为字符串数据数组
                    linedata = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var str in linedata)
                    {
                        acceleration[count++] += degree * Convert.ToSingle(str);
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