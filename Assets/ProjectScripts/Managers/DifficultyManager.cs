using UnityEngine;

[DisallowMultipleComponent]

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager instance;

    [Header("References")]
    public Hud hud;
    public PlayerMovement playerMovement;

    [Header("Difficulties by score")]
    public DifficultySystem easyDifficulty;
    public DifficultySystem mediumDifficulty;
    public DifficultySystem hardDifficulty;
    [Min(0)] public int mediumScore = 150;
    [Min(0)] public int hardScore = 350;

    [Header("Power Ups")]
    public PowerUpsGenerator powerUpsGenerator;

    public DifficultySystem CurrentDifficulty { get; private set; }

    private int nextPowerUpSpawnScore;
    private int pendingPowerUpSpawns;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (hud == null)
        {
            hud = FindObjectOfType<Hud>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (powerUpsGenerator == null)
        {
            powerUpsGenerator = FindObjectOfType<PowerUpsGenerator>();
        }

        if (powerUpsGenerator == null)
        {
            powerUpsGenerator = GetComponent<PowerUpsGenerator>();
        }

        if (powerUpsGenerator == null)
        {
            powerUpsGenerator = gameObject.AddComponent<PowerUpsGenerator>();
        }

        UpdateDifficultyFromScore();
        InitializePowerUpSpawnThreshold();
    }

    private void Update()
    {
        UpdateDifficultyFromScore();
        QueuePowerUpsByScore();
    }

    private void UpdateDifficultyFromScore()
    {
        int score = hud != null ? hud.CurrentScore : 0;
        int difficultyIndex = 0;

        if (score >= hardScore)
        {
            difficultyIndex = 2;
        }
        else if (score >= mediumScore)
        {
            difficultyIndex = 1;
        }

        DifficultySystem targetDifficulty = null;
        switch (difficultyIndex)
        {
            case 2:
                targetDifficulty = hardDifficulty != null ? hardDifficulty : mediumDifficulty;
                break;
            case 1:
                targetDifficulty = mediumDifficulty != null ? mediumDifficulty : easyDifficulty;
                break;
            default:
                targetDifficulty = easyDifficulty;
                break;
        }

        if (targetDifficulty == null || targetDifficulty == CurrentDifficulty) return;

        SetCurrentDifficulty(targetDifficulty);
    }

    private void SetCurrentDifficulty(DifficultySystem difficulty)
    {
        CurrentDifficulty = difficulty;
        ApplyPlayerDifficulty(difficulty);
        ApplyObstaclesDifficulty(difficulty);
        if (difficulty != easyDifficulty)
        {
            ApplyPlatformColors(difficulty);
        }
    }

    private void InitializePowerUpSpawnThreshold()
    {
        nextPowerUpSpawnScore = GetCurrentPowerUpInterval();
        pendingPowerUpSpawns = 0;
    }

    private void QueuePowerUpsByScore()
    {
        if (hud == null || CurrentDifficulty == null) return;

        int score = hud.CurrentScore;
        if (nextPowerUpSpawnScore <= 0)
        {
            nextPowerUpSpawnScore = GetCurrentPowerUpInterval();
        }

        while (score >= nextPowerUpSpawnScore)
        {
            pendingPowerUpSpawns++;

            nextPowerUpSpawnScore += GetCurrentPowerUpInterval();
        }
    }

    public void TrySpawnQueuedPowerUpOnCoinPlatform(CoinsGenerator coinsGenerator)
    {
        if (pendingPowerUpSpawns <= 0) return;
        if (CurrentDifficulty == null || powerUpsGenerator == null || coinsGenerator == null) return;
        if (CurrentDifficulty.allowedPowerUps == null || CurrentDifficulty.allowedPowerUps.Count == 0) return;
        if (!coinsGenerator.HasCoinsOnPlatform()) return;

        if (!coinsGenerator.TryGetEmptyLaneSpawnPoint(out Transform spawnPoint)) return;

        bool spawned = powerUpsGenerator.SpawnPowerUpOnSpawnPoint(CurrentDifficulty.allowedPowerUps, spawnPoint);
        if (!spawned) return;

        pendingPowerUpSpawns--;
    }

    private int GetCurrentPowerUpInterval()
    {
        if (CurrentDifficulty == null) return 500;
        return Mathf.Max(1, CurrentDifficulty.powerUpSpawnScoreInterval);
    }

    private void ApplyPlayerDifficulty(DifficultySystem difficulty)
    {
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (playerMovement == null) return;

        playerMovement.speed = difficulty.movementSpeed;
    }

    private void ApplyObstaclesDifficulty(DifficultySystem difficulty)
    {
        if (PlatformPoolingSystem.instance == null) return;

        Transform platformsRoot = PlatformPoolingSystem.instance.transform;
        for (int i = 0; i < platformsRoot.childCount; i++)
        {
            Transform platformTransform = platformsRoot.GetChild(i);
            ObstaclesGenerator obstaclesGenerator = platformTransform.GetComponent<ObstaclesGenerator>();
            if (obstaclesGenerator != null)
            {
                obstaclesGenerator.ApplyDifficulty(difficulty);
            }
        }
    }

    private void ApplyPlatformColors(DifficultySystem difficulty)
    {
        if (PlatformPoolingSystem.instance == null) return;

        Transform platformsRoot = PlatformPoolingSystem.instance.transform;
        for (int i = 0; i < platformsRoot.childCount; i++)
        {
            Transform platformTransform = platformsRoot.GetChild(i);
            Renderer[] renderers = platformTransform.GetComponentsInChildren<Renderer>(true);

            for (int j = 0; j < renderers.Length; j++)
            {
                Renderer renderer = renderers[j];
                if (renderer == null) continue;

                string rendererName = renderer.gameObject.name.ToLowerInvariant();
                if (rendererName.Contains("wall") || rendererName.Contains("side"))
                {
                    SetRendererColor(renderer, difficulty.wallColor);
                }
                else if (rendererName.Contains("platform"))
                {
                    SetRendererColor(renderer, difficulty.platformColor);
                }
            }
        }
    }

    private void SetRendererColor(Renderer renderer, Color color)
    {
        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material == null) continue;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }
    }
}
