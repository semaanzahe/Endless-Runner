using UnityEngine;

public abstract class Collectables : ScriptableObject
{
    public string collectableName;
    [TextArea] public string collectableDescription;
    public float collectableDuration; 
    
    public abstract void  ApplyPowerUP(GameObject targe);
    
}
