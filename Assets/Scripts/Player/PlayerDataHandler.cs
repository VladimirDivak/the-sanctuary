using System;
using System.IO;
using TheSanctuary;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

public static class PlayerDataHandler
{
    public static PlayerData playerData { get; private set; } = new PlayerData();
    private static readonly BinaryFormatter _formatter = new BinaryFormatter();
    private static readonly string saveDirectoryPath = Application.dataPath + "/plrd.ts";

    public static void Save()
    {
        using (FileStream fs = new FileStream(saveDirectoryPath, FileMode.OpenOrCreate))
        {
            _formatter.Serialize(fs, playerData);
        }
    }

    public static bool Load()
    {
        try
        {
            using (FileStream fs = new FileStream(saveDirectoryPath, FileMode.Open))
            {
                playerData = (PlayerData)_formatter.Deserialize(fs);
            }

            return true;
        }
        catch
        {
            playerData.username = "Vladimir Divak";
            playerData.throwAccuracyData = new List<float>();
            Save();

            return false;
        }
    }

    public static void ChangeAvgAccuracy(float lastValue)
    {
        float itemsSum = 0;
        playerData.throwAccuracyData.Add(lastValue);
        
        foreach(float item in playerData.throwAccuracyData)
        {
            itemsSum += item;
        }

        playerData.avgAccuracy = itemsSum / playerData.throwAccuracyData.Count;
    }
}