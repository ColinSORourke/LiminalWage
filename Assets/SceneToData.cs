using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This file provides an editor functionality for our game. Right clicking inside of assets and selecting Create -> My Game -> Save Scene calls this code.
// The purpose of this is to allow us to block out each street in it's own scene, and save it to as a ScriptableObject asset to then be rendered in the core game scene.

[ExecuteInEditMode]
public static class SceneToData
{
    [MenuItem("Assets/Create/My Game/SaveScene")]
    public static void SaveSceneData()
    {
        // Create the empty Scriptable Object
        ScriptObjStreet thisStreet = ScriptableObject.CreateInstance<ScriptObjStreet>();

        // First look for an Object in the Scene named "Ground"
        // We save the X and Z scale of the object, as well as it's color material.
        // When rendered, these are used to create a plane with that scale and color as the 'ground.'
        GameObject ground = GameObject.Find("Ground");
        var groundTrans = ground.GetComponent<Transform>();
        thisStreet.Length = groundTrans.localScale.x ;
        thisStreet.Width = groundTrans.localScale.z;
    
        var groundMesh = ground.GetComponent<MeshRenderer>();
        thisStreet.Color = groundMesh.sharedMaterial;

        // After loading the ground, we search for a game object named "ObjectParent". All actual game objects should be prefabs that are children of this parent.
        GameObject objPar = GameObject.Find("ObjectParent");
        thisStreet.objects = new streetObj[objPar.transform.childCount];
       
        // Iterate over each child
        for (int i = 0; i < objPar.transform.childCount; i++){
            streetObj obj = new streetObj();
            // Store the prefab used to instantiate it & it's position
            // It is assumed the positions are relative to a street center at 0,0,0.
            obj.myPrefab = PrefabUtility.GetCorrespondingObjectFromSource(objPar.transform.GetChild(i).gameObject);
            obj.streetPos = objPar.transform.GetChild(i).GetComponent<Transform>().localPosition;
            thisStreet.objects[i] = obj;
        }

        // Delete the previously saved version of this asset and overwrite it.
        // I don't yet have a way to dynamically name these assets? So to save multiple scenes, just rename something other than Street5
        AssetDatabase.DeleteAsset("Assets/Street5.asset");
        AssetDatabase.CreateAsset(thisStreet, "Assets/Street5.asset");
        AssetDatabase.SaveAssets();

    }
}
