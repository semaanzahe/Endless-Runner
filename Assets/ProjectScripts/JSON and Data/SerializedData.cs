
using System;
using UnityEngine;
[Serializable]
public class SerializedData
{
    public int profileNumber;
    public string profileName = "";
    public int totalCoins = 0;
    public int highestScore = 0;
    public int lastClaimedDay;            // Progress index (0 to 6)
    public string lastClaimTimeStamp;     // Exact real-world date/time of last claim

}
