using UnityEngine;
using System.Collections.Generic;

public class TestInstancingHandler : MonoBehaviour
{
    public GameObject[] gameObjects;

    private InstancedIndirectGridReplicator VisualWrap;

    void OnEnable()
    {
        VisualWrap = new InstancedIndirectGridReplicator(new Vector3Int(4, 4, 4), new Vector3(5, 12, 1));
        foreach (var go in gameObjects)
        {
            VisualWrap.AddGameObject(go);
        }
    }
    void OnDisable()
    {
        // destroy VisualWrap class
        VisualWrap.cleanupForDeletion();
        VisualWrap = null;
    }

    void Update()
    {
        VisualWrap.RenderFrame();
    }
}