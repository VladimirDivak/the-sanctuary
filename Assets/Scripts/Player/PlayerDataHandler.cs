using System;
using System.IO;
using TheSanctuary;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

public static class PlayerDataHandler
{
    public static PlayerData playerData { get; private set; } = new PlayerData();
    private static List<float> _accuracyData = new List<float>();

    private static readonly BinaryFormatter _formatter = new BinaryFormatter();
    private static readonly string _saveDirectoryPath = Application.persistentDataPath + "/plrd.dat";

    public static bool Load()
    {
        try
        {
            using (FileStream fs = new FileStream(_saveDirectoryPath, FileMode.Open))
            {
                playerData = (PlayerData)_formatter.Deserialize(fs);
            }

            return true;
        }
        catch
        {
            playerData.username = "Vladimir Divak";
            Task.Run(SaveAsync);

            return false;
        }
    }

    public static async Task SaveAsync()
    {
        await Task.Run(()=>
        {
            if(_accuracyData.Count != 0)
            {
                float accSum = 0;
                foreach(float accuracy in _accuracyData)
                {
                    accSum += accuracy;
                }
                accSum /= _accuracyData.Count;
                if(playerData.avgAccuracy != 0)
                {
                    playerData.avgAccuracy += accSum;
                    playerData.avgAccuracy /= 2;
                }
                else playerData.avgAccuracy = accSum;
                playerData.avgAccuracy = MathF.Round(playerData.avgAccuracy, 1);
            }
            else playerData.avgAccuracy = 0;

            using (FileStream fs = new FileStream(_saveDirectoryPath, FileMode.OpenOrCreate))
            {
                _formatter.Serialize(fs, playerData);
            }
            Debug.Log($"Данные успешно сохранены. {_accuracyData.Count} бросков, средняя точность - {playerData.avgAccuracy}");

            _accuracyData = null;
            _accuracyData = new List<float>();
        });
    }

    public static void AddAvgAccuracy(float lastValue) => _accuracyData.Add(lastValue);
}