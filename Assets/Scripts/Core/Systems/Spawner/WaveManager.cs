using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";

    [Tooltip("List of enemy prefabs to spawn in this wave.")]
    public List<GameObject> enemiesToSpawn;

    [Tooltip("Waitting time between enemy in this wave")]
    public float spawnDelay = 2f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Stage Settings")]
    [Tooltip("Danh sách các Đợt quái (Waves) của Màn chơi này")]
    public List<Wave> mapWaves;

    [Tooltip("Thời gian nghỉ giữa các đợt")]
    public float timeBetweenWaves = 5f;

    [Tooltip("Vị trí đẻ quái")]
    public Transform spawnPoint;

    private int currentWaveIndex = 0;

    private void Start()
    {
        if(spawnPoint == null)
        {
            spawnPoint = transform;
        }
        if (mapWaves == null || mapWaves.Count == 0)
        {
            Debug.Log("No waves defined for WaveManager");
            return;
        }
        StartCoroutine(SpawnWaveRoutine());
    }
    private IEnumerator SpawnWaveRoutine()
    {
        while (currentWaveIndex < mapWaves.Count)
        {
            Wave currentWave = mapWaves[currentWaveIndex];
            Debug.Log($"Bắt đầu: { currentWave.waveName}");

            foreach (GameObject enemyPrefab in currentWave.enemiesToSpawn)
            {
                if(enemyPrefab != null)
                {
                    Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                }    
                else
                {
                    Debug.Log("enemyPrefab null");
                }
                //Chờ sinh quái tiếp theo
                yield return new WaitForSeconds(currentWave.spawnDelay);
            }
            Debug.Log($"End: {currentWave.waveName}");

            // Advance to the next wave
            currentWaveIndex++;

            // If there are more waves left, wait between waves
            if (currentWaveIndex < mapWaves.Count)
            {
                Debug.Log($"Waitting {timeBetweenWaves} seconds");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
        Debug.Log("Finish all wave");
    }
}
