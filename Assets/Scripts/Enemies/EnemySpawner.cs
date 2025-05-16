/**********************************************************
 * Script Name: EnemySpawner
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 일정 시간마다 적을 스폰하는 스포너
 *********************************************************/

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] float _spawnInterval = 2f;
    [SerializeField] int _maxEnemies = 10;
    [SerializeField] float _spawnRadius = 10f;

    float _timer;
    int _curentEnemyCount;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _spawnInterval && _curentEnemyCount < _maxEnemies)
        {
            SpawnEnemy();
            _timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Vector2 randomPos = Random.insideUnitCircle * _spawnRadius;
        Instantiate(_enemyPrefab, new Vector3(randomPos.x, randomPos.y, 0), Quaternion.identity);
        _curentEnemyCount++;
    }

    public void OnEnemyDestroyed()
    {
        _curentEnemyCount--;
    }
}
