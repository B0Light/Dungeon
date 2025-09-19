using UnityEngine;

public class HoveringRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0f, 50f, 0f); // 초당 회전 속도 (도 단위)

    [Header("Hover Settings")]
    public float hoverAmplitude = 0.5f; // 위아래 움직이는 범위
    public float hoverFrequency = 1f;   // 초당 진동 횟수

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        // 회전
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // 호버링 (위아래 진동)
        float newY = _startPosition.y + Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}