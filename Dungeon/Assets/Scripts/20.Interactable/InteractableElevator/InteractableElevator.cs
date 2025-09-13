using System.Collections;
using UnityEngine;

public class InteractableElevator : Area
{
    [SerializeField] private float targetHeight;
    [SerializeField] private bool isMovingUp = false;
    [SerializeField] private float moveDuration = 3f;

    private Coroutine moveCoroutine;

    protected override void EnterArea(CharacterManager character)
    {
        base.EnterArea(character);
        
        if (moveCoroutine == null)
        {
            moveCoroutine = StartCoroutine(MoveElevator());
        }
    }

    private IEnumerator MoveElevator()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos;

        if (isMovingUp)
        {
            endPos = new Vector3(startPos.x, targetHeight, startPos.z);
        }
        else
        {
            endPos = new Vector3(startPos.x, targetHeight, startPos.z);
        }

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos; // 마지막 위치 보정
        moveCoroutine = null;
        isMovingUp = !isMovingUp;
    }
}
