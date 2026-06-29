using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DifficultyLevel", menuName = "Endless Runner/Difficulty Level")]
public class DifficultySystem : ScriptableObject
{

    [Header("Gameplay")]
    [Range(0f, 1f)] public float spawnRate = 0.35f;
    [Min(0f)] public float movementSpeed = 7f;
    [Min(0)] public int maxObstacles = 3;
    public List<GameObject> allowedObstacleTypes = new List<GameObject>();

    [Header("Power Ups")]
    [Min(1)] public int powerUpSpawnScoreInterval = 500;
    [FormerlySerializedAs("allowedCollectables")]
    public List<PowerUp> allowedPowerUps = new List<PowerUp>();

    [Header("Platform Look")]
    public Color platformColor = Color.white;
    public Color wallColor = Color.white;
}
