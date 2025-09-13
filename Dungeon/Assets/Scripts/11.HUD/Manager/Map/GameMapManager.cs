using UnityEngine;

public class GameMapManager : MonoBehaviour
{
    [Header("맵 카메라들")]
    [SerializeField] private Camera overWorldMapCamera;
    [SerializeField] private Camera mapComponentCamera;
    private MapGUIManager _mapGUI;
    
    void Start()
    {
        _mapGUI = FindFirstObjectByType<MapGUIManager>();
    }
    
}