/**********************************************************
 * Script Name: PlayerController
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 캐릭터의 움직임 및 상호작용 기능
 *********************************************************/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float _moveSpeed = 5f;
    Rigidbody2D _rb;
    Vector2 _moveInput;

    [Header("Shoot")]
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] float _bulletSpeed = 10f;
    [SerializeField] Transform _firePoint;

    [Header("BulletTime")]
    [SerializeField] float _bulletTimeScale = 0.2f;
    bool _isBulletTime = false;
    Vector3 _bulletTimeEndPos; // 불릿타임 시작 시 마우스 위치(고정 끝점)

    [Header("BulletLine")]
    LineRenderer _lineRenderer;
    [SerializeField] int _curvePoints = 20; // 곡선 해상도
    [SerializeField] float _curveIntensity = 1f; // 곡률 강도

    [Header("Player Status")]
    [SerializeField] float _maxHealth = 5f;
    float _currentHealth;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
        // 이동 입력
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // 플레이어 회전 -> 마우스 방향
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 궤적 업데이트
        UpdateTrajectory(mousePos);

        // 좌클릭으로 사격
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(mousePos);
        }

        if (Input.GetMouseButtonDown(1))
        {
            _bulletTimeEndPos = mousePos;
            EnableBulletTime();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            DisableBulletTime();
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveInput * _moveSpeed;
    }

    private void Shoot(Vector3 currentMousePos)
    {
        if (!_bulletPrefab || !_firePoint)
        {
            Debug.LogError("prefab or firepoint is not assigned");
            return;
        }
        Vector3 targetPos = _isBulletTime ? _bulletTimeEndPos : currentMousePos;
        List<Vector3> path = GenerateCurvePath(_firePoint.position, targetPos, currentMousePos);

        GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetPath(path, _bulletSpeed);

        // 사격 후 불릿타임 해제
        if (_isBulletTime)
        {
            DisableBulletTime();
        }
    }

    private void EnableBulletTime()
    {
        _isBulletTime = true;
        Time.timeScale = _bulletTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 업데이트 보정
    }

    private void DisableBulletTime()
    {
        _isBulletTime = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void UpdateTrajectory(Vector3 currentMousePos)
    {
        // bullettime 중에만 궤적 활성화
        if (_firePoint && _isBulletTime)
        {
            _lineRenderer.enabled = true;
            List<Vector3> path = GenerateCurvePath(_firePoint.position, _bulletTimeEndPos, currentMousePos);
            _lineRenderer.positionCount = path.Count;
            _lineRenderer.SetPositions(path.ToArray());
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }

    List<Vector3> GenerateCurvePath(Vector3 startPos, Vector3 endPos, Vector3 currentMousePos)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 controlPos = startPos + (endPos - startPos) / 2; // 기본 중간점
        Vector3 mouseOffset = currentMousePos - endPos; // 현재 마우스와 고정 끝점의 차이
        controlPos += mouseOffset * _curveIntensity; // 이동량으로 곡률 조정

        // 2차 베지어 곡선
        for (int i = 0; i < _curvePoints; i++)
        {
            float t = i / (float)(_curvePoints - 1);
            Vector3 point = Mathf.Pow(1 - t, 2) * startPos +
                            2 * (1 - t) * t * controlPos +
                            Mathf.Pow(t, 2) * endPos;
            path.Add(point);
        }
        return path;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            _currentHealth--;
            if (_currentHealth <= 0)
            {
                LevelManager.Instance.GameOver();
            }
        }
    }
}
