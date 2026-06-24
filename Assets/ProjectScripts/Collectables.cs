using UnityEngine;

public class Collectables : ScriptableObject
{
    public string collectableName;
    [TextArea] public string collectableDescription;
    public GameObject collectablePrefab;
    public float collectableDuration; // how duration of the collectable when collecting it
    
    
}
