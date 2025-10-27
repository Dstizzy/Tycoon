using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    // 1. 싱글톤 패턴을 위한 코드
    // 'Instance'라는 static 변수에 자기 자신을 저장하여
    // 다른 모든 스크립트가 'TurnManager.Instance'로 이 스크립트에 접근할 수 있게 합니다.
    public static TurnManager Instance { get; private set; }

    void Awake()
    {
        // 씬에 이미 TurnManager 인스턴스가 있는지 확인
        if (Instance != null && Instance != this)
        {
            // 이미 있다면 이 오브젝트는 파괴
            Destroy(gameObject);
        }
        else
        {
            // 없다면 이 인스턴스를 static 변수에 할당
            Instance = this;
            // (선택 사항) 씬이 바뀌어도 파괴되지 않게 하려면
            // DontDestroyOnLoad(gameObject); 
        }
    }
    // --- 싱글톤 코드 끝 ---


    // --- 기존 GameManager의 로직 ---
    [Header("Crystal Setting")]
    public int currentResource = 0;
    public int resourcePerTurn = 50; // 턴당 얻는 재화
    public Text resourceText; // 또는 public TextMeshProUGUI resourceText;

    [Header("Turn Setting")]
    public int currentTurn = 1;
    public int maxTurns = 20;
    public Text turnText;     // 턴 텍스트 UI

    [Header("UI/Game Status")]
    public Button endTurnButton; // 게임 종료 시 비활성화할 버튼
    private bool isGameActive = true;

    void Start()
    {
        UpdateResourceUI();
        UpdateTurnUI();
    }

    // '턴 종료' 버튼이 호출할 공용(public) 함수
    public void EndTurn()
    {
        if (!isGameActive) return; // 게임 종료 시 아무것도 안 함

        // 1. 재화 증가
        currentResource += resourcePerTurn;
        UpdateResourceUI();

        // 2. 턴 증가
        currentTurn++;

        // 3. 턴 확인 및 UI 업데이트
        if (currentTurn > maxTurns)
        {
            EndGame();
        }
        else
        {
            UpdateTurnUI();
            Debug.Log("Turn" + currentTurn + "Start");

            // 여기에 다음 턴이 시작될 때 필요한 로직을 추가
            // (예: 적 턴 시작, 유닛 행동력 초기화 등)
        }
    }

    void UpdateResourceUI()
    {
        if (resourceText != null)
        {
            resourceText.text = "Crystal: " + currentResource.ToString();
        }
    }

    void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = "Turn: " + currentTurn.ToString() + " / " + maxTurns.ToString();
        }
    }

    void EndGame()
    {
        isGameActive = false;
        Debug.Log("Game over! Reached max turn(" + maxTurns + ").");

        if (turnText != null)
        {
            turnText.text = "Game over!";
        }

        if (endTurnButton != null)
        {
            endTurnButton.interactable = false;
        }
    }
}