/**********************************************************
 * Script Name: MenuButton
 * Author: 김우성
 * Date Created: 2025-05-16
 * Last Modified: 2025-05-16
 * Description: 
 * - 메인 메뉴에서, 레벨을 선택해서 들어가는 버튼
 *********************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
