using UnityEngine;

public class DungeonFloorManager : MonoBehaviour
{
    [SerializeField] private Material onMouseMat;
    [SerializeField] private Material onClickMat;

    private GameObject currentGameObject = null;
    private Material originMaterial = null;

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        if (Mouse3D.GetRaycastHit(mousePos, out RaycastHit hit))
        {
            if (currentGameObject != hit.collider.gameObject)
            {
                if (currentGameObject != null && originMaterial != null)
                {
                    currentGameObject.GetComponent<MeshRenderer>().material = originMaterial;
                }

                currentGameObject = hit.collider.gameObject;
                originMaterial = currentGameObject.GetComponent<MeshRenderer>().material;
                currentGameObject.GetComponent<MeshRenderer>().material = onMouseMat;
            }

            if (Input.GetMouseButtonDown(0))
            {
                currentGameObject.GetComponent<MeshRenderer>().material = onClickMat;
            }
        }
        else
        {
            if (currentGameObject != null && originMaterial != null)
            {
                currentGameObject.GetComponent<MeshRenderer>().material = originMaterial;
                currentGameObject = null;
                originMaterial = null;
            }
        }
    }
}
