using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawn")]
    [SerializeField] private GameObject enemyPrefab;

    public GameObject GetEnemyPrefab() => enemyPrefab;

    [Header("Time Spawn")]
    [SerializeField] private float spawnInterval = 5f;

    private float timer;

    private void Start()
    {
        timer = spawnInterval; // Khởi tạo timer bằng khoảng thời gian spawn
    }

    private void Update()
    {
        timer -= Time.deltaTime; // Giảm timer theo thời gian
        if (timer <= 0f)
        {
            SpawnEnemy(); // Spawn một enemy mới
            timer = spawnInterval; // Reset timer sau khi spawn
        }
    }
    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity); // Tạo một instance của enemyPrefab tại vị trí của EnemySpawner
        }
        else
        {
            Debug.LogWarning("Enemy Prefab chưa được gán trong EnemySpawner!");
        }

    }
}
