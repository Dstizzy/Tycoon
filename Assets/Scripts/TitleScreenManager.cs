using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 이동할 때 꼭 필요합니다!

public class MainMenuManager : MonoBehaviour
{
    // "START" 버튼을 눌렀을 때 실행될 함수
    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("MainScene");
    }

    // "INFO" 버튼을 눌렀을 때 실행될 함수
    public void OnInfoButtonClick()
    {
        Debug.Log("정보 버튼 클릭!");
        // TODO: 정보(Info) 팝업 창을 띄우는 코드
    }

    // "SETTINGS" 버튼을 눌렀을 때 실행될 함수
    public void OnSettingsButtonClick()
    {
        Debug.Log("설정 버튼 클릭!");
        // TODO: 설정(Settings) 팝업 창을 띄우는 코드
    }
}