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
    private static readonly string saveDirectoryPath = Application.dataPath + "/plrd.tsntry";

    public static void Save()
    {
        float accSum = 0;
        foreach(float accuracy in playerData.throwAccuracyData)
        {
            accSum += accuracy;
        }
        accSum /= playerData.throwAccuracyData.Count;
        playerData.avgAccuracy += accSum;
        playerData.avgAccuracy /= 2;
        playerData.avgAccuracy = MathF.Round(playerData.avgAccuracy, 1);

        playerData.throwAccuracyData = new List<float>();

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

    public static void ChangeAvgAccuracy(float lastValue) => playerData.throwAccuracyData.Add(lastValue);
}