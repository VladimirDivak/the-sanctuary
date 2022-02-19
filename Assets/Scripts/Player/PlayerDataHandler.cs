using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using TheSanctuary;
using TheSanctuary.Network;
using TheSanctuary.Interfaces;

using UnityEngine;

public static class PlayerDataHandler
{
    public static PlayerData playerData { get; private set; } = new PlayerData();
    private static List<float> _accuracyData = new List<float>();

    private static readonly BinaryFormatter _formatter = new BinaryFormatter();
    private static readonly string _saveDirectoryPath = Application.persistentDataPath + "/plrd.sv";

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

    public static NetworkGame GetNetworkGameData(string name)
    {
        if(playerData.multiplayerGamesData != null && playerData.multiplayerGamesData.Count(x => x.name == name) != 0)
            return playerData.multiplayerGamesData.Find(x => x.name == name);
        else
        {
            NetworkGame newGameData = new NetworkGame();
            newGameData.name = name;
            playerData.multiplayerGamesData.Add(newGameData);

            return newGameData;
        }
    }
    public static void UpdateNetworkGameData(NetworkGame data, string game)
    {
        NetworkGame dataForUpdate = playerData.multiplayerGamesData.Find(x => x.name == game);
        int dataIndex = playerData.multiplayerGamesData.IndexOf(dataForUpdate);

        playerData.multiplayerGamesData[dataIndex] = data;
    }
}