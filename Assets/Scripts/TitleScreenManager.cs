using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 이동할 때 꼭 필요합니다!

public class MainMenuManager : MonoBehaviour
{
    public Animator fadePanelAnimator; // FadePanel의 Animator
    public float fadeDuration = 0.5f;  // Fade_Out 애니메이션의 길이(초)

    // "START" 버튼을 눌렀을 때 실행될 함수
    public void OnStartButtonClick()
    {
        StartCoroutine(FadeAndLoadScene("GameScene"));
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

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // 1. 애니메이션 트리거 실행
        fadePanelAnimator.SetTrigger("StartFadeOut");

        // 2. 애니메이션이 끝날 때까지 기다림
        yield return new WaitForSeconds(fadeDuration);

        // 3. 씬 로드
        SceneManager.LoadScene(sceneName);
    }
}