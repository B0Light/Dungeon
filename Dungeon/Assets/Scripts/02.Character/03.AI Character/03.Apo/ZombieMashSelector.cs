using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieMashSelector : MonoBehaviour
{
    [SerializeField] private List<GameObject> meshList;

    private void Start()
    {
        if(meshList == null || meshList.Count == 0) return;

        foreach (var mesh in meshList)
        {
            mesh.SetActive(false);
        }
        
        meshList[Random.Range(0, meshList.Count)].SetActive(true);
    }
}
