/**********************************************************
 * Script Name: Bullet
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 플레이어가 발사할 총알 프리펩
 *********************************************************/

using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    Vector3 _targetPosition; // 마우스 커서 위치
    bool _hasTarget = false;

    // PlayerController에서 호출해서 목표 위치 설정
    public void SetTarget(Vector3 target)
    {
        _targetPosition = target;
        _targetPosition.z = 0; // 2D라서 0
        _hasTarget = true;
    }

    private void Update()
    {
        if (!_hasTarget) return;

        // 목표 지점으로 이동, 불릿타임 보정
        float step = _speed * Time.deltaTime / Time.timeScale;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);

        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        /* 적과 충돌 처리 추가 */
        //Destroy(gameObject);
    }
}
