using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;

// https://www.youtube.com/watch?v=8bpYHCKdZno
public static class SQLiteDB
{
    private static string dbName = "URI=file:" + Application.streamingAssetsPath + "/RecordData/" + "ExperimentRecords.db";

    public static void CreateDB()
    {
        // Debug.Log(new Detail().ToString());
        // using (var connection = new SqliteConnection(dbName))
        // {
        //     connection.Open();
        //     using (var command = connection.CreateCommand())
        //     {
        //         command.CommandText = "CREATE TABLE IF NOT EXISTS";
        //         command.ExecuteNonQuery();
        //     }
        //     connection.Close();
        // }
    }

    public static void AddRecord()
    {
        using (var connection = new SqliteConnection(dbName))
        {

            // connection.Open();
            // using (var command = connection.CreateCommand())
            // {

            //     command.CommandText = "INSERT INTO";
            //     command.ExecuteNonQuery();
            // }
            // connection.Close();
        }
    }

    // public static void GetTableData()
    // {
    //     using (var connection = new SqliteConnection(dbName))
    //     {
    //         connection.Open();
    //         using (var command = connection.CreateCommand())
    //         {
    //             command.CommandText = "SELECT * FROM ;";
    //             using (IDataReader reader = command.ExecuteReader())
    //             {
    //                 while (reader.Read())
    //                 {

    //                 }
    //                 reader.Close();
    //             }

    //         }
    //         connection.Close();
    //     }
    // }

    // public static void Structure2TableSQL(Type type)
    // {
    //     var temp = new type();
    // }
}
