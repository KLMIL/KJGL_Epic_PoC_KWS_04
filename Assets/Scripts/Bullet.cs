/**********************************************************
 * Script Name: Bullet
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 플레이어가 발사할 총알 프리펩
 *********************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    List<Vector3> _path;
    int _currentPointIndex;
    bool _hasPath = false;

    Vector3 _targetPosition; // 마우스 커서 위치 -> DEP cause 궤적으로 변환
    bool _hasTarget = false; // -> DEP cause 궤적으로 변환

    //PlayerController에서 호출해서 목표 위치 설정
    public void SetTarget(Vector3 target) // -> DEP cause 궤적으로 변환
    {
        _targetPosition = target;
        _targetPosition.z = 0; // 2D라서 0
        _hasTarget = true;
    }

    public void SetPath(List<Vector3> newPath, float newSpeed)
    {
        _path = newPath;
        _speed = newSpeed;
        _hasPath = _path != null && _path.Count > 0;
        if (_hasPath)
        {
            transform.position = _path[0];
        }
    }

    private void Update()
    {
        //if (!_hasTarget) return;
        if (!_hasPath || _currentPointIndex >= _path.Count) return;

        // 목표 지점으로 이동, 불릿타임 보정 -> DEP cause 궤적으로 변환
        //float step = _speed * Time.deltaTime / Time.timeScale;
        //transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);

        //if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        //{
        //    Destroy(gameObject);
        //}


        // 현재 목표 포인트로 이동
        Vector3 target = _path[_currentPointIndex];
        float step = _speed * Time.deltaTime / Time.timeScale; // 불릿타임 보정
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            _currentPointIndex++;
            if (_currentPointIndex >= _path.Count)
            {
                Destroy(gameObject); // 경로 끝 도달시 파괴
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        /* 적과 충돌 처리 추가 */
        //Destroy(gameObject);
    }
}
