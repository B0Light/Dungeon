using System.Collections;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        ResetCharacter();
        WorldSaveGameManager.Instance.SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중이면 중지
#else
            Application.Quit(); // 빌드된 게임에서는 종료
#endif
    }

    public void BackToTitle()
    {
        ResetCharacter();
        WorldSaveGameManager.Instance.SaveGame();
        GUIController.Instance.HandleEscape();
        WorldSceneChangeManager.Instance.LoadSceneAsync(1);
    }

    private void ResetCharacter()
    {
        if (WorldSceneChangeManager.Instance.IsExtractionMap())
        {
            GameManager.Instance.GetPlayer().playerVariableManager.OnPlayerDeath(true);
            StartCoroutine(ResetCharacterCoroutine());
        }
    }

    IEnumerator ResetCharacterCoroutine()
    {
        yield return new WaitForEndOfFrame();
        
        GUIController.Instance.playerUIPopUpManager.CloseYouDiedPopUp();

    }
}