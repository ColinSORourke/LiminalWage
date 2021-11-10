using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct VisualWrapConfig
{
    public bool renderWrappingInstances;
    public float wrapDistanceVertical;
    public int wrapCountVertical;
    public int wrapCountHorizontal;
    public int wrapCountParallel;
    public float wrapDistanceParallel;
}

[System.Serializable]
public struct derivedConfig
{
    public int level;
    public int ignore;
    [System.NonSerialized]
    public RenderedStreet ignoredStreet;
    public float middle;
    public float range;
}

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

    public VisualWrapConfig wrapConfig;

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
        derivedConfig notDerived;
        notDerived.level = 0;
        notDerived.ignore = -1;
        notDerived.ignoredStreet = null;
        notDerived.middle = 0;
        notDerived.range = myStreet.Length * 5;
        // We start by instantiating a renderedStreet from a Script Object attached to the player.
        onStreet = new RenderedStreet(wrapConfig, myStreet, new Vector3(0, 0, 0), true, notDerived);
    }

    // Update is called once per frame
    void Update()
    {
        // Keeps some object names up to date for easier debugging.
        onStreet.parent.name = "onStreet";
        // COLIN: !!!!!!! Got an error here^ saying that "GameObject" was destroyed, but you are still trying to access it.

        // Calculate the players LOCAL POSITION. Their position relative to the center of the street they are on.
        var relPos = playerTrans.position - onStreet.truePos;

        var teleWrap = onStreet.wraparound(playerTrans);
        if (teleWrap != new Vector3(0,0,0)){
            Debug.Log("Trying to wrap");
            var realPlayer = GameObject.Find("Player").GetComponent<CharacterController>();
            realPlayer.enabled = false;
            realPlayer.transform.position = realPlayer.transform.position + teleWrap;
            realPlayer.enabled = true;
        }
        var maybeTurned = onStreet.onIntersection(renderDistance, playerTrans.position);
        if (maybeTurned != onStreet)
        {
            myStreet = maybeTurned.streetInfo;
            var pos = maybeTurned.truePos;
            
            var orient = maybeTurned.xOriented;
            onStreet.destroyStreet();

            derivedConfig notDerived;
            notDerived.level = 0;
            notDerived.ignore = -1;
            notDerived.ignoredStreet = null;
            notDerived.middle = 0;
            notDerived.range = myStreet.Length * 5;

            onStreet = new RenderedStreet(wrapConfig, myStreet, pos, orient, notDerived);
        } 

        if (onStreet.VisualWrap != null){
            onStreet.VisualWrap.RenderFrame();
            onStreet.mirrorChildren();
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

    // kyle addition for wrapping:
    public InstancedIndirectGridReplicator VisualWrap;
    public VisualWrapConfig wrapConfig;

    [System.NonSerialized]
    public RenderedStreet[] myIntersections;

    public GameObject objectParent;
    public GameObject wallsParent;
    public GameObject[] myObjects;

    GameObject ground;

    public derivedConfig derived;

    // The constructer takes:
    // ScriptObjStreet, the scriptable object storing juicy information
    // Vector3 Pos, the true-game position of the street-center.
    // bool intersection - is this a full street we want to render, or the small intersection portion.
    // int interIndex - the index of intersection between this and another street. Helps avoid double-rendering some stuff, as well as correctly positioning the street.
    public RenderedStreet(VisualWrapConfig wrapConfig, ScriptObjStreet street, Vector3 pos, bool orient, derivedConfig derivedInfo)
    {
        this.wrapConfig = wrapConfig;
        streetInfo = street;
        truePos = pos;
        xOriented = orient;

        derived = derivedInfo;

        // Create a Parent Game object at the street's true-game position. All other game objects will be childed to this parent, and thus their coordinates will be local to that position.
        parent = new GameObject();
        parent.name = "Street" + derived.level;
        var parentTrans = parent.GetComponent<Transform>();
        parentTrans.position = pos;

        parent.layer = 8;

        // Create and color the ground.
        /* ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Middle: " + middle;
        ground.layer = 8;
        ground.transform.parent = parent.transform;
        var groundRenderer = ground.GetComponent<MeshRenderer>();
        groundRenderer.material = street.Color; */

        if (derived.level < 2){
            this.render();
        }
        else {
            this.partialRender();
        }
        
    }

    public void partialRender(){
        myIntersections = new RenderedStreet[1];
        myIntersections[0] = derived.ignoredStreet;
        derived.ignore = 0;

        myObjects = new GameObject[streetInfo.objects.Length];

        var newAngle = Quaternion.Euler(new Vector3(0, 90, 0));

        objectParent = new GameObject();
        objectParent.transform.parent = parent.transform;
        objectParent.name = "Objects";
        objectParent.GetComponent<Transform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        for (int j = 0; j < myObjects.Length; j++)
        {
            var obj = streetInfo.objects[j];
            if (obj.streetPos.x > derived.middle - derived.range && obj.streetPos.x < derived.middle + derived.range){
                myObjects[j] = GameObject.Instantiate(obj.myPrefab);
                myObjects[j].transform.parent = objectParent.transform;

                var objTransform = myObjects[j].GetComponent<Transform>();
                if (xOriented)
                {
                    objTransform.localPosition = obj.streetPos;
                    objTransform.localRotation = obj.rotation;

                }
                else
                {
                    objTransform.localPosition = new Vector3(obj.streetPos.z, obj.streetPos.y, obj.streetPos.x);
                    objTransform.localRotation = Quaternion.Euler(obj.rotation.eulerAngles - new Vector3(0, 90, 0));
                }
            } else {
                myObjects[j] = null;
            }
        }        

        int totalIntersections = myIntersections.Length;
        float previousEdge = derived.middle - derived.range;
        float intersectWidth = 0; 
        for (int j = 0; j < totalIntersections; j++)
        {
            var intersection = myIntersections[j];
            
            if (intersection != null)
            {
                intersectWidth = intersection.streetInfo.Width * 5;
                float interEdge;
                float newEdge;
                if (xOriented)
                {
                    interEdge = (intersection.truePos.x - this.truePos.x) - intersectWidth;
                    newEdge = (intersection.truePos.x - this.truePos.x) + intersectWidth;
                }
                else
                {
                    interEdge = (intersection.truePos.z - this.truePos.z) - intersectWidth;
                    newEdge = (intersection.truePos.z - this.truePos.z) + intersectWidth;
                }
                float center = interEdge - Mathf.Abs(previousEdge - interEdge) / 2;
                float scale = Mathf.Abs(previousEdge - interEdge) / 10;
                this.addWall(center, scale);
                previousEdge = newEdge;
            }
        }
        float finalEdge = derived.middle + derived.range + intersectWidth;
        float finalCenter = finalEdge - Mathf.Abs(previousEdge - finalEdge) / 2;
        float finalScale = Mathf.Abs(previousEdge - finalEdge) / 10;
        this.addWall(finalCenter, finalScale);

        if (wrapConfig.renderWrappingInstances)
        {
            Vector3Int repetitionCount;
            Vector3 repetitionSpacing;
            float verticalSpacing = wrapConfig.wrapDistanceVertical;
            int vWrapCount = wrapConfig.wrapCountVertical;
            int hWrapCount = wrapConfig.wrapCountHorizontal;
            int pWrapCount = wrapConfig.wrapCountParallel;
            float pSpacing = wrapConfig.wrapDistanceParallel;
            if (xOriented)
            {
                repetitionCount = new Vector3Int(hWrapCount, vWrapCount, pWrapCount);
                repetitionSpacing = new Vector3(streetInfo.Length * 10, verticalSpacing, pSpacing);
            }
            else // must be z-oriented
            {
                repetitionCount = new Vector3Int(pWrapCount, vWrapCount, hWrapCount);
                repetitionSpacing = new Vector3(pSpacing, verticalSpacing, streetInfo.Length * 10);
            }
            VisualWrap = new InstancedIndirectGridReplicator(repetitionCount, repetitionSpacing, this.xOriented);
            VisualWrap.AddAllChildGameObjects(objectParent);
        }
    }

    public void render()
    {
        /* var groundTransform = ground.GetComponent<Transform>();

        if (xOriented){
            groundTransform.localPosition = new Vector3(middle,0.0f,0.0f);
            groundTransform.localScale = new Vector3(edge/5, 1, streetInfo.Width);
        } else {
            groundTransform.localPosition = new Vector3(0.0f,0.0f,middle);
            groundTransform.localScale = new Vector3(streetInfo.Width, 1, edge/5);
        } */

        myIntersections = new RenderedStreet[streetInfo.intersections.Length];
        for (int j = 0; j < myIntersections.Length; j++)
        {
            var inter = streetInfo.intersections[j];
            Vector3 pos;
            if (xOriented)
            {
                pos = truePos + new Vector3(inter.position, 0.0f, 0.0f) - new Vector3(0.0f, 0.0f, inter.otherPosition);
            }
            else
            {
                pos = truePos + new Vector3(0.0f, 0.0f, inter.position) - new Vector3(inter.otherPosition, 0.0f, 0.0f);
            }
            var otherStreet = inter.other;

            derivedConfig derivedInter;
            VisualWrapConfig newWrap = this.wrapConfig;

            derivedInter.level = this.derived.level + 1;
            derivedInter.ignore = inter.oppositeIndex;
            derivedInter.ignoredStreet = this;
            if (this.derived.level == 0){
                derivedInter.middle = 0;
                derivedInter.range = otherStreet.Length;
                newWrap.wrapCountParallel = this.wrapConfig.wrapCountHorizontal;
                newWrap.wrapDistanceParallel = this.streetInfo.Length * 10;
            } else {
                derivedInter.middle = inter.otherPosition;
                derivedInter.range = streetInfo.Width + 5.0f * otherStreet.Width;
                newWrap.wrapCountParallel = 0;
                newWrap.wrapCountHorizontal = 0;
            }

            if (j != derived.ignore)
            {
                var renderedInter = new RenderedStreet(newWrap, otherStreet, pos, !xOriented, derivedInter);
                myIntersections[j] = renderedInter;
            }
            else
            {
                // Don't! Render a small intersection that we are already standing on.
                myIntersections[j] = derived.ignoredStreet;
            }
        }

        myObjects = new GameObject[streetInfo.objects.Length];

        var newAngle = Quaternion.Euler(new Vector3(0, 90, 0));

        objectParent = new GameObject();
        objectParent.transform.parent = parent.transform;
        objectParent.name = "Objects";
        objectParent.GetComponent<Transform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        for (int j = 0; j < myObjects.Length; j++)
        {
            var obj = streetInfo.objects[j];
            myObjects[j] = GameObject.Instantiate(obj.myPrefab);
            myObjects[j].transform.parent = objectParent.transform;

            var objTransform = myObjects[j].GetComponent<Transform>();
            if (xOriented)
            {
                objTransform.localPosition = obj.streetPos;
                objTransform.localRotation = obj.rotation;

            }
            else
            {
                objTransform.localPosition = new Vector3(obj.streetPos.z, obj.streetPos.y, obj.streetPos.x);
                objTransform.localRotation = Quaternion.Euler(obj.rotation.eulerAngles - new Vector3(0, 90, 0));
            }
        }

        int totalIntersections = myIntersections.Length;
        float previousEdge = streetInfo.Length * -5;
        float intersectWidth = 0; 
        for (int j = 0; j < totalIntersections; j++)
        {
            var intersection = myIntersections[j];
            
            if (intersection != null)
            {
                intersectWidth = intersection.streetInfo.Width * 5;
                float interEdge;
                float newEdge;
                if (xOriented)
                {
                    interEdge = (intersection.truePos.x - this.truePos.x) - intersectWidth;
                    newEdge = (intersection.truePos.x - this.truePos.x) + intersectWidth;
                }
                else
                {
                    interEdge = (intersection.truePos.z - this.truePos.z) - intersectWidth;
                    newEdge = (intersection.truePos.z - this.truePos.z) + intersectWidth;
                }
                float center = interEdge - Mathf.Abs(previousEdge - interEdge) / 2;
                float scale = Mathf.Abs(previousEdge - interEdge) / 10;
                this.addWall(center, scale);
                previousEdge = newEdge;
            }
        }
        float finalEdge = (streetInfo.Length * 5) + intersectWidth;
        float finalCenter = finalEdge - Mathf.Abs(previousEdge - finalEdge) / 2;
        float finalScale = Mathf.Abs(previousEdge - finalEdge) / 10;
        this.addWall(finalCenter, finalScale);


        // kyle create wraparound instancing class

        if (wrapConfig.renderWrappingInstances)
        {
            Vector3Int repetitionCount;
            Vector3 repetitionSpacing;
            float verticalSpacing = wrapConfig.wrapDistanceVertical;
            int vWrapCount = wrapConfig.wrapCountVertical;
            int hWrapCount = wrapConfig.wrapCountHorizontal;
            int pWrapCount = wrapConfig.wrapCountParallel;
            float pSpacing = wrapConfig.wrapDistanceParallel;
            if (xOriented)
            {
                repetitionCount = new Vector3Int(hWrapCount, vWrapCount, pWrapCount);
                repetitionSpacing = new Vector3(streetInfo.Length * 10, verticalSpacing, pSpacing);
            }
            else // must be z-oriented
            {
                repetitionCount = new Vector3Int(pWrapCount, vWrapCount, hWrapCount);
                repetitionSpacing = new Vector3(pSpacing, verticalSpacing, streetInfo.Length * 10);
            }
            VisualWrap = new InstancedIndirectGridReplicator(repetitionCount, repetitionSpacing, this.xOriented);
            VisualWrap.AddAllChildGameObjects(objectParent);
        }
    }

    public void mirrorChildren(){
        for(int i = 0; i < myIntersections.Length; i++){
            var otherStreet = myIntersections[i];
            if (otherStreet != null && i != derived.ignore){
                if (otherStreet.VisualWrap != null){
                    otherStreet.VisualWrap.RenderFrame();
                    otherStreet.mirrorChildren();
                }
            }
        }
    }

    public void addWall(float center, float scale)
    {
        Quaternion planeAngleLeft;
        Quaternion planeAngleRight;
        Vector3 planePosLeft;
        Vector3 planePosRight;
        if (xOriented)
        {
            planeAngleLeft = Quaternion.Euler(new Vector3(90, 180, 0));
            planeAngleRight = Quaternion.Euler(new Vector3(90, 0, 0));
            planePosLeft = new Vector3(center, 0, streetInfo.Width * 5);
            planePosRight = new Vector3(center, 0, streetInfo.Width * -5);
        }
        else
        {
            planeAngleLeft = Quaternion.Euler(new Vector3(90, 270, 0));
            planeAngleRight = Quaternion.Euler(new Vector3(90, 90, 0));
            planePosLeft = new Vector3(streetInfo.Width * 5, 0, center);
            planePosRight = new Vector3(streetInfo.Width * -5, 0, center);
        }

        Vector3 planeScale = new Vector3(scale, 1, 10);

        var objLeft = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var objRendererL = objLeft.GetComponent<MeshRenderer>();
        objRendererL.material = streetInfo.Color;
        objRendererL.material.shader = Shader.Find("Instanced/InstancedIndirectSurfaceShader");
        objLeft.transform.parent = objectParent.transform;
        objLeft.GetComponent<Transform>().localPosition = planePosLeft;
        objLeft.GetComponent<Transform>().rotation = planeAngleLeft;
        objLeft.GetComponent<Transform>().localScale = planeScale;

        var objRight = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var objRendererR = objRight.GetComponent<MeshRenderer>();
        objRendererR.material = streetInfo.Color;
        objRendererR.material.shader = Shader.Find("Instanced/InstancedIndirectSurfaceShader");
        objRight.transform.parent = objectParent.transform;
        objRight.GetComponent<Transform>().localPosition = planePosRight;
        objRight.GetComponent<Transform>().rotation = planeAngleRight;
        objRight.GetComponent<Transform>().localScale = planeScale;
    }

    public Vector3 wraparound(Transform playerTransform)
    {
        float playerStreetPos;
        Vector3 adjust;
        if (xOriented){
            playerStreetPos = playerTransform.position.x - truePos.x;
            adjust = new Vector3(streetInfo.Length * 10, 0, 0);
        } else {
            playerStreetPos = playerTransform.position.z - truePos.z;
            adjust = new Vector3(0, 0, streetInfo.Length * 10);
        }

        if (playerStreetPos > streetInfo.Length*5){
            return adjust * -1;
        } else if (playerStreetPos < streetInfo.Length * -5){
            return adjust;
        } else {
            return new Vector3(0,0,0);
        }
        
    }

    public RenderedStreet onIntersection(float distance, Vector3 playerWorldPos)
    {

        bool turned = false;
        RenderedStreet turnedOnto = this;
        for (int i = 0; i < myIntersections.Length; i++)
        {
            var intersecting = myIntersections[i];
            if (intersecting != null)
            {
                var width = intersecting.streetInfo.Width;
                float relativePos;
                bool offEdge;
                if (xOriented)
                {
                    relativePos = Mathf.Abs(playerWorldPos.x - intersecting.truePos.x);
                    offEdge = Mathf.Abs(playerWorldPos.z - this.truePos.z) > this.streetInfo.Width * 5;
                }
                else
                {
                    relativePos = Mathf.Abs(playerWorldPos.z - intersecting.truePos.z);
                    offEdge = Mathf.Abs(playerWorldPos.x - this.truePos.x) > this.streetInfo.Width * 5;
                }
                if (relativePos <= width * 5 && offEdge)
                {
                    turned = true;
                    turnedOnto = intersecting;
                }
            }
        }
        if (turned)
        {
            return turnedOnto;
        }
        else
        {
            return this;
        }
    }

    // This destroys all gameObjects associated with the street.
    public void destroyStreet()
    {
        GameObject.Destroy(parent);
        for (int i = 0; i < myIntersections.Length; i++)
        {
            var inter = myIntersections[i];
            if (inter != null && i != derived.ignore)
            {
                inter.destroyStreet();
            }
        }
        // destroy VisualWrap class
        if (VisualWrap != null){
            VisualWrap.cleanupForDeletion();
            VisualWrap = null;
        }
        
    }

    public void destroyObjects()
    {
        GameObject.Destroy(objectParent);
        GameObject.Destroy(wallsParent);
        for (int i = 0; i < myIntersections.Length; i++)
        {
            var inter = myIntersections[i];
            if (inter != null && i != derived.ignore)
            {
                inter.destroyStreet();
            }
        }
        // destroy VisualWrap class
        if (VisualWrap != null){
            VisualWrap.cleanupForDeletion();
            VisualWrap = null;
        }
    }
}