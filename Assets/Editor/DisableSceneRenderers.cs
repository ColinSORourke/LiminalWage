#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

// This file provides an editor functionality for our game. Right clicking inside of assets and selecting Create -> My Game -> Save Scene calls this code.
// The purpose of this is to allow us to block out each street in it's own scene, and save it to as a ScriptableObject asset to then be rendered in the core game scene.

[ExecuteInEditMode]
public static class DisableSceneRenderers
{
    [MenuItem("Assets/Create/My Game/DisableWrappingRenderComponents")]
    public static void DisableWrappingRenderComponents()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            foreach (Renderer childR in go.GetComponentsInChildren<Renderer>())
            {
                Material instanceMaterial = childR.sharedMaterial;
                if (instanceMaterial.shader.name != "Instanced/InstancedIndirectSurfaceShader") continue;
                childR.enabled = false;
            }
        }
    }

    [MenuItem("Assets/Create/My Game/DisableWrappingRenderComponents")]
    public static void EnableWrappingRenderComponents()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            foreach (Renderer childR in go.GetComponentsInChildren<Renderer>())
            {
                Material instanceMaterial = childR.sharedMaterial;
                if (instanceMaterial.shader.name != "Instanced/InstancedIndirectSurfaceShader") continue;
                childR.enabled = true;
            }
        }
    }
}
#endif