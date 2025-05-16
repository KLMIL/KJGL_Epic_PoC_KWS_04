/**********************************************************
 * Script Name: LevelManager
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 레벨 선택과 완료 조건 통제 스크립트
 *********************************************************/

using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    static LevelManager _instance;
    public static LevelManager Instance => _instance;

    [SerializeField] string _nextLevel;
    [SerializeField] float _timeLimit = 180f; // 시간

    [SerializeField] Text _timerText; // UI 텍스트
    [SerializeField] Text _enemyCountText; // UI 텍스트
    
    float _timer;
    GameObject[] _enemies;
    bool _isInTrigger;


    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _timer = _timeLimit;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            Debug.Log("Game Over");
            GameOver();
        }

        _timerText.text = $"Time: {Mathf.CeilToInt(_timer)}";
        _enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _enemyCountText.text = $"Enemies: {_enemies.Length}";

        if (_enemies.Length == 0)
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(_nextLevel))
        {
            SceneManager.LoadScene(_nextLevel);
        }
        else
        {
            Debug.Log("Game Complete!");
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene("MainScene"); // 실패 시 메인 메뉴로
    }
}
