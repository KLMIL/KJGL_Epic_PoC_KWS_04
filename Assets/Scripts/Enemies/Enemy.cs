/**********************************************************
 * Script Name: Enemy
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 일정 거리 내 플레이어를 검출해서 다가오고 사격하는 적
 *********************************************************/

using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 2f;
    [SerializeField] float _maxHealth = 3f;
    [SerializeField] float _flashDuration = 0.1f;
    [SerializeField] float _detectionRange = 5f;

    float _currentHealth;
    Transform _player;
    Rigidbody2D _rb;
    SpriteRenderer _spriteRenderer;
    Color _originalColor;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        _currentHealth = _maxHealth;
        _player = GameObject.FindGameObjectWithTag("Player").transform; // 플레이어 찾기
    }

    private void FixedUpdate()
    {
        if (_player && Vector2.Distance(transform.position, _player.position) <= _detectionRange)
        {
            Vector2 direction = (_player.position - transform.position).normalized;
            _rb.linearVelocity = direction * _moveSpeed;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        FlashSprite();
        if (_currentHealth <= 0)
        {
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner) spawner.OnEnemyDestroyed();
            Destroy(gameObject);
        }
    }

    private void FlashSprite()
    {
        _spriteRenderer.color = Color.white;
        Invoke(nameof(ResetColor), _flashDuration);
    }

    private void ResetColor()
    {
        _spriteRenderer.color = _originalColor;
    }
}
