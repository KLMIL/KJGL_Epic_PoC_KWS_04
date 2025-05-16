/**********************************************************
 * Script Name: PlayerController
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 캐릭터의 움직임 및 상호작용 기능
 *********************************************************/

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

    [Header("BulletLine")]
    LineRenderer _lineRenderer;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
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
        if (Input.GetMouseButtonDown(0) && !_isBulletTime)
        {
            Shoot(mousePos);
        }

        if (Input.GetMouseButtonDown(1))
        {
            _isBulletTime = true;
            Time.timeScale = _bulletTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 업데이트 보정
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isBulletTime = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveInput * _moveSpeed;
    }

    private void Shoot(Vector3 targetPos)
    {
        if (!_bulletPrefab || !_firePoint)
        {
            Debug.LogError("prefab or firepoint is not assigned");
            return;
        }
        GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetTarget(targetPos);
    }

    private void UpdateTrajectory(Vector3 mousePos)
    {
        // bullettime 중에만 궤적 활성화
        if (_firePoint && _isBulletTime)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, _firePoint.position);
            _lineRenderer.SetPosition(1, mousePos);
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
}
