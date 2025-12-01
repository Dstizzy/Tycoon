using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   // 이 슬롯들을 인스펙터에서 채워야 합니다.
   public Animator FadePanelAnimator;
   public float FadeDuration = 0.5f;

   // START 버튼이 호출할 함수
   public void OnStartButtonClick()
   {
      // "GameScene" 부분은 실제 메인 씬 이름으로 변경하세요.
      StartCoroutine(FadeAndLoadScene("MainScene"));
   }

   // 정보 버튼 (미구현)
   public void OnInfoButtonClick()
   {
      Debug.Log("Info button clicked!");
   }

   // 설정 버튼 (미구현)
   public void OnSettingsButtonClick()
   {
      Debug.Log("Settings button clicked!");
   }

   // 페이드 아웃을 실행하고 씬을 로드하는 코루틴
   private IEnumerator FadeAndLoadScene(string sceneName)
   {
      // 1. 애니메이터에게 "StartFadeOut" 신호를 보냅니다.
      FadePanelAnimator.SetTrigger("StartFadeOut");

      // 2. 애니메이션이 끝날 때까지 (FadeDuration 초) 기다립니다.
      yield return new WaitForSeconds(FadeDuration);

      // 3. 씬을 로드합니다.
      SceneManager.LoadScene(sceneName);
   }
}