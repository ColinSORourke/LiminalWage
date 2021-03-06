#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

// This file provides an editor functionality for our game. Right clicking inside of assets and selecting Create -> My Game -> Save Scene calls this code.
// The purpose of this is to allow us to block out each street in it's own scene, and save it to as a ScriptableObject asset to then be rendered in the core game scene.

[ExecuteInEditMode]
public static class SceneToData
{
    [MenuItem("Assets/Create/My Game/SaveScene")]
    public static void SaveSceneData()
    {
        // Create the empty Scriptable Object
        Scene scene = SceneManager.GetActiveScene();

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
            obj.rotation = objPar.transform.GetChild(i).GetComponent<Transform>().rotation;
            thisStreet.objects[i] = obj;
        }

        GameObject interPar = GameObject.Find("Intersections");
        thisStreet.intersections = new Intersection[interPar.transform.childCount];

        for (int i = 0; i < interPar.transform.childCount; i++){
            Intersection inter = new Intersection();
            var interTrans = (interPar.transform.GetChild(i)).GetComponent<Transform>();
            inter.position = interTrans.position.x;
            inter.otherPosition = interTrans.position.z * -1;

            thisStreet.intersections[i] = inter;
        }

        System.Array.Sort(thisStreet.intersections, new IntersectionComparer());

        // Delete the previously saved version of this asset and overwrite it.
        string path = "Assets/ScriptableObjects/StreetData/" + scene.name + ".asset";

        var previous = (ScriptObjStreet) AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        Debug.Log(previous);
        if (previous != null && thisStreet.intersections.Length == previous.intersections.Length){
            Debug.Log("In Here");
            for (int i = 0; i < thisStreet.intersections.Length; i++){
                var other = previous.intersections[i].other;
                for (int j = 0; j < other.intersections.Length; j++){
                    if (other.intersections[j].other = previous){
                        bool matching = other.intersections[j].position == thisStreet.intersections[i].otherPosition && other.intersections[j].otherPosition == thisStreet.intersections[i].position;
                        Debug.Log("Checking for Match");
                        Debug.Assert(matching);
                        if (!matching){
                            Debug.LogError("Intersection with" + other + "Is not properly matched");
                        }
                    }
                }
                thisStreet.intersections[i].other = other;
            }
        }

        AssetDatabase.CreateAsset(thisStreet, path);
        AssetDatabase.SaveAssets();
    }
}
#endif