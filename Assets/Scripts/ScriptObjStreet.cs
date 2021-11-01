using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This creates a menu button that allows you to create an empty Street data object.
[CreateAssetMenu(fileName = "StreetData", menuName = "My Game/Street Data")]

// The magical Scriptable Object. These are the current fields used to represent each street. More fields could easily be added
    // To add more fields, code would need to be adjusted in SceneToData.cs to allow us to save something to these fields from a scene
    // And code would need to be adjusted in StreetManager.cs - to actually use the data in those fields.
public class ScriptObjStreet : ScriptableObject
{
    public float Length;
    public float Width;
    public Material Color;

    public Intersection[] intersections;

    public streetObj[] objects;
}

// Intersection sub class for ease of use. (Currently I have no way of 'building' an intersection in a scene, so these have to be manually filled out.)
[System.Serializable] 
public class Intersection
{
    public float position;
    public float otherPosition;
    public ScriptObjStreet other;
}

// streetObj sub class. Prefab + position allows us to instantiate objects exactly where we want them.
[System.Serializable] 
public class streetObj
{
    public GameObject myPrefab;
    public Vector3 streetPos;
}