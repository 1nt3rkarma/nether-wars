using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    const string defaultFileName = "/terrain.tdf";

    public static void SaveTerrain(TerrainSystem terrain)
    {
        string path = Application.persistentDataPath + defaultFileName;
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

        TerrainData data = new TerrainData(TerrainSystem.local);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static TerrainData LoadTerrain()
    {
        string path = Application.persistentDataPath + defaultFileName;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            TerrainData data = formatter.Deserialize(stream) as TerrainData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Не удалось найти файл");
            return null;
        }
    }
}
