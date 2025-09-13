using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialTrigger[] tutorialTriggers;
    [SerializeField] private bool resetTriggersOnNewGame = true;
    
    // 게임 시작 또는 재시작 시 호출
    public void ResetAllTriggers()
    {
        if (resetTriggersOnNewGame)
        {
            foreach (var trigger in tutorialTriggers)
            {
                // tutorialTriggered 변수를 리셋하는 메소드 필요
                // 현재 클래스 구조에서는 private이므로 리팩토링 필요
            }
        }
    }
}