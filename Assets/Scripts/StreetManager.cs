using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the big boy. It is doing a whole lot of work to create 'seamless' wraparound.
// This script is attached to the player, but is entirely separate from the player controls. 
// It takes no input, and does no modifying to the player body. It simply reads the players position to determine how to render the world around them.
// It's split into two parts. The StreetManager MonoBehaviour, and a 'RenderedStreet' Class.

// StreetManager. This tracks what street the player is currently on, and what other streets need to be rendered to preserve the wraparound.
public class StreetManager : MonoBehaviour
{
    public ScriptObjStreet myStreet;
    public RenderedStreet onStreet;
    public float renderDistance = 200;

    private Transform playerTrans;

    public void Construct(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            throw new ArgumentNullException(nameof(playerTransform));
        }

        playerTrans = playerTransform;
    }

    // Start is called before the first frame update
    void Start()
    {
        // We start by instantiating a renderedStreet from a Script Object attached to the player.
        onStreet = new RenderedStreet(myStreet, new Vector3(0, 0, 0), true);
    }

    // Update is called once per frame
    void Update()
    {
        // Keeps some object names up to date for easier debugging.
        onStreet.parent.name = "onStreet";

        // Calculate the players LOCAL POSITION. Their position relative to the center of the street they are on.
        var relPos = playerTrans.position - onStreet.truePos;

        onStreet.wraparound(renderDistance, playerTrans.position);
        var maybeTurned = onStreet.onIntersection(renderDistance, playerTrans.position);
        if (maybeTurned != onStreet){
            myStreet = maybeTurned.streetInfo;
            var pos = maybeTurned.truePos;
            var middle = maybeTurned.middle;
            var edge = maybeTurned.edge;
            var orient = maybeTurned.xOriented;
            onStreet.destroyStreet();
            onStreet = new RenderedStreet(myStreet, pos, orient, middle, edge);
        }

    }
}

[System.Serializable] 
public class RenderedStreet
{
    public ScriptObjStreet streetInfo;
    public Vector3 truePos;
    public GameObject parent;
    public bool xOriented;
    public float edge;
    public float middle;

    [System.NonSerialized]
    public RenderedStreet[] myIntersections;

    public GameObject objectParent;
    public GameObject wallsParent;
    public GameObject[] myObjects;

    GameObject ground;
    int ignoreIndex;
    RenderedStreet ignoreStreet;

    // The constructer takes:
    // ScriptObjStreet, the scriptable object storing juicy information
    // Vector3 Pos, the true-game position of the street-center.
    // bool intersection - is this a full street we want to render, or the small intersection portion.
    // int interIndex - the index of intersection between this and another street. Helps avoid double-rendering some stuff, as well as correctly positioning the street.
    public RenderedStreet(ScriptObjStreet street, Vector3 pos, bool orient, float center = 0.0f, float range = -1.0f, RenderedStreet ignoredInter = null,int interIndex = -1){
        streetInfo = street;
        truePos = pos;
        xOriented = orient;
        // Create a Parent Game object at the street's true-game position. All other game objects will be childed to this parent, and thus their coordinates will be local to that position.
        parent = new GameObject();
        parent.name = "Street";
        var parentTrans = parent.GetComponent<Transform>();
        parentTrans.position = pos;

        if (range == -1.0f){
            edge = street.Length * 5;
        } else {
            edge = range;
        }

        ignoreStreet = ignoredInter;
        ignoreIndex = -1;


        Debug.Log(center);
        Debug.Log(street.Length);
        this.middle = center % (street.Length * 10);
        Debug.Log(this.middle);

        parent.layer = 8;

        // Create and color the ground.
        /* ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Middle: " + middle;
        ground.layer = 8;
        ground.transform.parent = parent.transform;
        var groundRenderer = ground.GetComponent<MeshRenderer>();
        groundRenderer.material = street.Color; */

        this.render(interIndex);
    }

    public void render(int i){
        /* var groundTransform = ground.GetComponent<Transform>();

        if (xOriented){
            groundTransform.localPosition = new Vector3(middle,0.0f,0.0f);
            groundTransform.localScale = new Vector3(edge/5, 1, streetInfo.Width);
        } else {
            groundTransform.localPosition = new Vector3(0.0f,0.0f,middle);
            groundTransform.localScale = new Vector3(streetInfo.Width, 1, edge/5);
        } */
        
        int copies = 1 + (int) Mathf.Floor(this.edge/ (streetInfo.Length * 5));
        
        int intersLength = streetInfo.intersections.Length;

        myIntersections = new RenderedStreet[copies * intersLength];
        for(int j = 0; j < intersLength; j++){
            var inter = streetInfo.intersections[j];
            var interRelPos = inter.position - this.middle;
            for (int currCopy = 0; currCopy < copies; currCopy += 1){
                var offset = currCopy - Mathf.Floor(copies/2);
                if (Mathf.Abs(interRelPos + (streetInfo.Length * offset * 10)) < this.edge){
                    Vector3 pos;
                    if (xOriented){
                        pos = truePos + new Vector3(inter.position + streetInfo.Length * offset * 10, 0.0f, 0.0f) - new Vector3(0.0f, 0.0f, inter.otherPosition);
                    } else {
                        pos = truePos + new Vector3(0.0f, 0.0f, inter.position + streetInfo.Length * offset * 10) - new Vector3(inter.otherPosition, 0.0f, 0.0f);
                    }
                    var otherStreet = inter.other;
                    int index = this.getOtherIntersectionId(otherStreet);
                    var interWidth = 25.0f * otherStreet.Width;
                    if (this.ignoreIndex != -1){
                        interWidth = 8.0f * otherStreet.Width;
                    }

                    if (j != i || offset != 0){
                        var renderedInter = new RenderedStreet(otherStreet, pos, !xOriented, inter.otherPosition, interWidth, this, index);
                        myIntersections[j + (intersLength * currCopy)] = renderedInter;
                    }
                    else {
                        // Don't! Render a small intersection that we are already standing on.
                        ignoreIndex = j + (intersLength * currCopy);
                        myIntersections[j + (intersLength * currCopy)] = ignoreStreet;
                    }
                }
                else {
                    // Don't! Render an intersection that is outside of our current range.
                    myIntersections[j + (intersLength * currCopy)] = null;
                }
            }
        }

        int objLength = streetInfo.objects.Length;
        myObjects = new GameObject[copies * objLength];

        var newAngle = Quaternion.Euler(new Vector3(0, 90, 0));

        objectParent = new GameObject();
        objectParent.transform.parent = parent.transform;
        objectParent.name = "Objects";
        objectParent.GetComponent<Transform>().localPosition = new Vector3 (0.0f, 0.0f, 0.0f);

        for(int j = 0; j < objLength; j++){
            
            var obj = streetInfo.objects[j];
            var objRelPos = obj.streetPos.x - middle;
            for (int currCopy = 0; currCopy < copies; currCopy += 1){
                var offset = currCopy - Mathf.Floor(copies/2);
                if (Mathf.Abs(objRelPos + streetInfo.Length * offset * 10) < this.edge){
                    myObjects[j + (objLength * currCopy)] = GameObject.Instantiate(obj.myPrefab);
                    myObjects[j + (objLength * currCopy)].transform.parent = objectParent.transform;

                    var objTransform = myObjects[j + (objLength * currCopy)].GetComponent<Transform>();
                    if (xOriented){
                        objTransform.localPosition = obj.streetPos + new Vector3(streetInfo.Length * offset * 10, 0.0f, 0.0f);
                        objTransform.localRotation = obj.rotation;

                    } else {
                        objTransform.localPosition = new Vector3 (obj.streetPos.z, obj.streetPos.y, obj.streetPos.x) + new Vector3(0.0f, 0.0f, streetInfo.Length * offset * 10);
                        objTransform.localRotation = Quaternion.Euler(obj.rotation.eulerAngles + new Vector3(0,90,0));
                    }
                }
                else {
                    // Don't! Render Extra objects
                    myObjects[j + (objLength * currCopy)] = null;
                }
            }
        }

        wallsParent = new GameObject();
        wallsParent.transform.parent = parent.transform;
        wallsParent.name = "Walls";
        wallsParent.GetComponent<Transform>().localPosition = new Vector3 (0.0f, 0.0f, 0.0f);

        int totalIntersections = myIntersections.Length;
        float previousEdge = middle - edge;
        for (int j = 0; j < totalIntersections; j++){
            var intersection = myIntersections[j];
            if (intersection != null){
                float interEdge;
                float newEdge;
                if (xOriented){
                    interEdge = (intersection.truePos.x - this.truePos.x) - intersection.streetInfo.Width * 5;
                    newEdge = (intersection.truePos.x - this.truePos.x) + intersection.streetInfo.Width * 5;
                } else {
                    interEdge = (intersection.truePos.z - this.truePos.z) - intersection.streetInfo.Width * 5;
                    newEdge = (intersection.truePos.z - this.truePos.z) + intersection.streetInfo.Width * 5;
                }
                float center = interEdge - Mathf.Abs(previousEdge - interEdge)/2;
                float scale = Mathf.Abs(previousEdge - interEdge)/10;
                this.addWall(center, scale);
                previousEdge = newEdge;
            }
        }
        float finalEdge = middle + edge;
        float finalCenter = finalEdge - Mathf.Abs(previousEdge - finalEdge)/2;
        float finalScale = Mathf.Abs(previousEdge - finalEdge)/10;
        this.addWall(finalCenter, finalScale);
    }

    public void addWall(float center, float scale){
        Quaternion planeAngleLeft;
        Quaternion planeAngleRight;
        Vector3 planePosLeft;
        Vector3 planePosRight;
        if (xOriented){
            planeAngleLeft = Quaternion.Euler(new Vector3(90, 180, 0));
            planeAngleRight = Quaternion.Euler(new Vector3(90, 0, 0));
            planePosLeft = new Vector3(center, 0, streetInfo.Width*5);
            planePosRight = new Vector3(center, 0, streetInfo.Width*-5);
        } else {
            planeAngleLeft = Quaternion.Euler(new Vector3(90, 270, 0));
            planeAngleRight = Quaternion.Euler(new Vector3(90, 90, 0));
            planePosLeft = new Vector3(streetInfo.Width*5, 0, center);
            planePosRight = new Vector3(streetInfo.Width*-5, 0, center);
        }

        Vector3 planeScale = new Vector3 (scale, 1, 30);

        var objLeft = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var objRendererL = objLeft.GetComponent<MeshRenderer>();
        objRendererL.material = streetInfo.Color;
        objLeft.transform.parent = wallsParent.transform;
        objLeft.GetComponent<Transform>().localPosition = planePosLeft;
        objLeft.GetComponent<Transform>().rotation = planeAngleLeft;
        objLeft.GetComponent<Transform>().localScale = planeScale;

        var objRight = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var objRendererR = objRight.GetComponent<MeshRenderer>();
        objRendererR.material = streetInfo.Color;
        objRight.transform.parent = wallsParent.transform;
        objRight.GetComponent<Transform>().localPosition = planePosRight;
        objRight.GetComponent<Transform>().rotation = planeAngleRight;
        objRight.GetComponent<Transform>().localScale = planeScale;
    }

    public void wraparound(float distance, Vector3 playerWorldPos, int index = -1){
        float playerStreetPos;
        if (xOriented){
            playerStreetPos = playerWorldPos.x - truePos.x;
        } else {
            playerStreetPos = playerWorldPos.z - truePos.z;
        }
        

        bool offTopEdge = playerStreetPos + distance >= middle + edge ;
        bool offBottomEdge =  playerStreetPos - distance <= middle - edge;

        if (offTopEdge && offBottomEdge){
            edge += 40;
            this.destroyObjects();
            this.render(index);
        } else if (offTopEdge){
            if (Mathf.Abs(playerStreetPos) > (streetInfo.Length * 5)){
                if (xOriented){
                    this.truePos = this.truePos + new Vector3(streetInfo.Length * 10, 0, 0);
                } else {
                    this.truePos = this.truePos + new Vector3(0, 0, (streetInfo.Length * 10));
                }
                this.parent.GetComponent<Transform>().position = this.truePos;
                middle *= -1;
            }
            middle = playerStreetPos % (streetInfo.Length * 10);
            this.destroyObjects();
            this.render(index);
        } else if (offBottomEdge){
            if (Mathf.Abs(playerStreetPos) > (streetInfo.Length * 5)){
                if (xOriented){
                    this.truePos = this.truePos - new Vector3(streetInfo.Length * 10, 0, 0);
                } else {
                    this.truePos = this.truePos - new Vector3(0, 0, (streetInfo.Length * 10));
                }
                this.parent.GetComponent<Transform>().position = this.truePos;
                middle *= -1;
            }
            middle = playerStreetPos % (streetInfo.Length * 10);
            this.destroyObjects();
            this.render(index);
        }
    }

    public RenderedStreet onIntersection(float distance, Vector3 playerWorldPos){

        bool turned = false;
        RenderedStreet turnedOnto = this;
        for (int i = 0; i < myIntersections.Length; i++){
            var intersecting = myIntersections[i];
            if (intersecting != null){
                var width = intersecting.streetInfo.Width;
                float relativePos;
                bool offEdge;
                if (xOriented){
                    relativePos = Mathf.Abs(playerWorldPos.x - intersecting.truePos.x);
                    offEdge = Mathf.Abs(playerWorldPos.z - this.truePos.z) > this.streetInfo.Width * 5;
                } else {
                    relativePos = Mathf.Abs(playerWorldPos.z - intersecting.truePos.z);
                    offEdge = Mathf.Abs(playerWorldPos.x - this.truePos.x) > this.streetInfo.Width * 5;
                }
                var temp = intersecting.getOtherIntersectionId(streetInfo);
                var index = this.getOtherIntersectionId(this.streetInfo.intersections[temp].other);
                var interWidth = 25.0f;
                if (this.ignoreIndex != -1){
                    interWidth = 8.0f;
                }
                if (relativePos <= ((width * 5) + 5)){
                    if (intersecting.edge <= interWidth){
                        intersecting.edge = intersecting.streetInfo.Length * 5;
                    }
                    intersecting.edge = this.edge;
                    intersecting.destroyObjects();
                    intersecting.render(index);
                } else {
                    if (intersecting.edge > interWidth){
                        intersecting.edge = interWidth;
                        var selfId = intersecting.getOtherIntersectionId(this.streetInfo);
                        var inter = intersecting.streetInfo.intersections[selfId];
                        intersecting.destroyObjects();
                        intersecting.render(index);
                    }
                }

                if (relativePos <= width * 5 && offEdge){
                    turned = true;
                    turnedOnto = intersecting;
                }
            }
            
        }
        if (turned){
            return turnedOnto;
        } else {
            return this;
        }
    }

    // This destroys all gameObjects associated with the street.
    public void destroyStreet(){
        GameObject.Destroy(parent);
        for (int i = 0; i < myIntersections.Length; i++){
            var inter = myIntersections[i];
            if (inter != null && i != ignoreIndex){
                inter.destroyStreet();
            }
        }
    }

    public void destroyObjects(){
        GameObject.Destroy(objectParent);
        GameObject.Destroy(wallsParent);
        for (int i = 0; i < myIntersections.Length; i++){
            var inter = myIntersections[i];
            if (inter != null && i != ignoreIndex){
                inter.destroyStreet();
            }
        }
    }

    // This gets an index of intersection with another street.
    public int getOtherIntersectionId(ScriptObjStreet otherStreet){
        int index = -1;
        for (int i = 0; i < otherStreet.intersections.Length; i++){
            var maybeThis = otherStreet.intersections[i].other;
            if (maybeThis == streetInfo){
                index = i;
                break;
            }
        }
        return index;
    }
}