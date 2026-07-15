using System.IO;
using UnityEngine;

public class Serializator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       SerializeData();
       DeserializeData();
    }

    void SerializeData()
    {
        var json = JsonUtility.ToJson(new SerializedData());
        
        
        File.WriteAllText(Application.persistentDataPath + "/data.json", json);
    }

    void DeserializeData()
    {
        var json =  File.ReadAllText(Application.persistentDataPath + "/data.json");
        Debug.Log(json);
    }
}
