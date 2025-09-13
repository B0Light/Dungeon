using UnityEngine;

public class ModelInstantiateSlot : MonoBehaviour
{
    public void UnloadModel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadModel(GameObject itemModel)
    {
        itemModel.transform.parent = transform;

        itemModel.transform.localPosition = Vector3.zero;
        itemModel.transform.localRotation = Quaternion.identity;
        itemModel.transform.localScale = Vector3.one;
    }
}
