/**********************************************************
 * Script Name: PlayerController
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 캐릭터의 움직임 및 상호작용 기능
 *********************************************************/

using System.Collections.Generic;
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
    Vector3 _bulletTimeStartPos; // 곡선 중간 지점
    Vector3 _bulletTimeEndPos; // 곡선 최종 지점

    [Header("BulletLine")]
    LineRenderer _lineRenderer;
    [SerializeField] int _curvePoints = 20; // 곡선 해상도
    [SerializeField] float _curveIntensity = 1f; // 곡률 강도
    [SerializeField] float _maxTargetMoveDistance = 5f; // 불릿타임 중 최대 이동 거리
    [SerializeField] float _maxBendAngle = 60f; // 최대 꺾임 각도

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


        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 플레이어 회전 -> 마우스 방향, 불릿타임 중 회전 중지
        if (!_isBulletTime)
        {
            Vector3 direction = (mousePos - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 불릿타임 중 회전 중지
        if (_isBulletTime)
        {
            // _bulletTimeEndPos 업데이트, 각도 제한
            Vector3 newEndPos = mousePos;
            Vector3 offset = newEndPos - _bulletTimeStartPos;
            newEndPos = _bulletTimeStartPos + Vector3.ClampMagnitude(offset, _maxTargetMoveDistance);

            // 각도 제한: _firePoint -> _bulletTimeStartPos -> _bulletTimeEndPos
            Vector3 dir1 = (_bulletTimeStartPos - _firePoint.position).normalized;
            Vector3 dir2 = (newEndPos - _bulletTimeStartPos).normalized;
            float signedAngle = Vector3.SignedAngle(dir1, dir2, Vector3.forward);
            float absAngle = Mathf.Abs(signedAngle);
            if (absAngle > _maxBendAngle)
            {
                // 최대 각도(+90도 또는 -90도)로 고정
                float targetAngle = _maxBendAngle * Mathf.Sign(signedAngle);
                Vector3 perp = Vector3.Cross(dir1, Vector3.forward).normalized;
                Vector3 clampedDir = Quaternion.AngleAxis(targetAngle, Vector3.forward) * dir1;
                float dist = Vector3.Distance(_bulletTimeStartPos, newEndPos);
                newEndPos = _bulletTimeStartPos + clampedDir * dist;
                // 거리 제한 재적용
                newEndPos = _bulletTimeStartPos + Vector3.ClampMagnitude(newEndPos - _bulletTimeStartPos, _maxTargetMoveDistance);
                Debug.Log($"SignedAngle: {signedAngle}, ClampedAngle: {targetAngle}, newEndPos: {newEndPos}");
            }

            _bulletTimeEndPos = newEndPos;
        }

        // 궤적 업데이트
        UpdateTrajectory(mousePos);

        // 좌클릭으로 사격
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(mousePos);
        }

        if (Input.GetMouseButtonDown(1))
        {
            _bulletTimeStartPos = mousePos; // 불릿타임 시작 시 초기 위치 저장
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

        Vector3 startPos = _firePoint.position;
        Vector3 targetPos = _isBulletTime ? _bulletTimeEndPos : currentMousePos;
        List<Vector3> path = GenerateCurvePath(startPos, targetPos, _isBulletTime ? _bulletTimeStartPos : targetPos);

        GameObject bullet = Instantiate(_bulletPrefab, startPos, _firePoint.rotation);
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
        _bulletTimeStartPos = Vector3.zero;
        _bulletTimeEndPos = Vector3.zero;
    }

    private void UpdateTrajectory(Vector3 currentMousePos)
    {
        // bullettime 중에만 궤적 활성화
        if (_firePoint && _isBulletTime)
        {
            _lineRenderer.enabled = true;
            List<Vector3> path = GenerateCurvePath(_firePoint.position, _bulletTimeEndPos, _bulletTimeStartPos);
            _lineRenderer.positionCount = path.Count;
            _lineRenderer.SetPositions(path.ToArray());
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }

    List<Vector3> GenerateCurvePath(Vector3 startPos, Vector3 endPos, Vector3 midPos)
    {
        List<Vector3> path = new List<Vector3>();

        // 불릿 타임의 경우
        if (_isBulletTime)
        {
            Vector3 p0 = startPos; // _firePoint
            Vector3 p3 = endPos; // _bulletTimeEndPos
            Vector3 m = midPos; // _bulletTimeStartPos

            float dist1 = Vector3.Distance(p0, m);
            float dist2 = Vector3.Distance(m, p3);
            Vector3 dir1 = (m - p0).normalized;
            Vector3 dir2 = (p3 - m).normalized;

            // 곡률 방향
            Vector3 relativeEnd = p3 - m;
            float curveDirection = relativeEnd.x >= 0 ? -1f : 1f;
            Vector3 perp = Vector3.Cross(dir1, Vector3.forward).normalized * curveDirection;

            // 곡률 강도: 각도 제한 반영
            float signedAngle = Vector3.SignedAngle(dir1, dir2, Vector3.forward);
            float absAngle = Mathf.Abs(signedAngle);
            float curveFactor = Mathf.Clamp01(Vector3.Distance(m, p3) / _maxTargetMoveDistance) * _curveIntensity * Mathf.Clamp01(_maxBendAngle / Mathf.Max(absAngle, 1f));

            Vector3 p1 = p0 + dir1 * dist1 * 0.5f + perp * dist1 * curveFactor * 0.5f;
            Vector3 p2 = p3 - dir2 * dist2 * 0.5f + perp * dist2 * curveFactor * 0.5f;

            if (Vector3.Distance(m, p3) < 0.1f)
            {
                p1 = Vector3.Lerp(p0, m, 0.5f);
                p2 = p1;
            }
            else
            {
                // t=0.5에서 midPos 경유 보정
                Vector3 midPoint = 0.125f * p0 + 0.375f * p1 + 0.375f * p2 + 0.125f * p3;
                Vector3 offset = m - midPoint;
                p1 += offset * 4f / 3f;
                p2 += offset * 4f / 3f;
            }

            for (int i = 0; i < _curvePoints; i++)
            {
                float t = i / (float)(_curvePoints - 1);
                Vector3 point = Mathf.Pow(1 - t, 3) * p0 +
                               3 * Mathf.Pow(1 - t, 2) * t * p1 +
                               3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                               Mathf.Pow(t, 3) * p3;
                path.Add(point);
            }
        }
        else
        { 
            // 직선 경로
            for (int i = 0; i < _curvePoints; i++)
            {
                float t = i / (float)(_curvePoints - 1);
                Vector3 point = Vector3.Lerp(startPos, endPos, t);
                path.Add(point);
            }
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
