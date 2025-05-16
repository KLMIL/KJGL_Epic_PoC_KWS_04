/**********************************************************
 * Script Name: Target
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 튜토리얼을 위한, 공격하지 않고 고정된 적 개체
 *********************************************************/

using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] float _health = 1f;
    SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0f)
        {
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner) spawner.OnEnemyDestroyed();
            Destroy(gameObject);
        }
    }
}
