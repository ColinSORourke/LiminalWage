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
    public renderedStreet onStreet;
    public renderedStreet wrapStreet;
    public renderedStreet interStreet;
    public renderedStreet interStreetWrap;

    // Start is called before the first frame update
    void Start()
    {
        // We start by instantiating a renderedStreet from a Script Object attached to the player.
        onStreet = new renderedStreet(myStreet, new Vector3(0, 0, 0), false, false);
        wrapStreet = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Keeps some object names up to date for easier debugging.
        onStreet.parent.name = "onStreet";
        if (interStreet != null){
            interStreet.parent.name = "interStreet";
        }

        // Calculate the players LOCAL POSITION. Their position relative to the center of the street they are on.
        var playerTrans = this.GetComponent<Transform>();
        var relPos = playerTrans.position - onStreet.truePos;  

        // Call wraparound Logic - this takes a float representing the render distance, a reference to the street the player is currently on, and a reference to a place to put the wraparound street.
        this.wraparoundLogic(25.0f, ref this.onStreet, ref this.wrapStreet);

        // This is crossing the wraparound 'edge'. We check orientation to know which edge to compare to.
        if (onStreet.xOriented) {
            // When we cross the edge, we simple swap what is considered the main street and what is considered the clone.
            // myStreet (the scriptable object) does not need to change because it's the same for both of these streets.
            if (Mathf.Abs(relPos.x) > myStreet.Length * 5.0f){
                var temp = wrapStreet;
                wrapStreet = onStreet;
                onStreet = temp;
            }
        } else {
            // When we cross the edge, we simple swap what is considered the main street and what is considered the clone.
            // myStreet (the scriptable object) does not need to change because it's the same for both of these streets.
            if (Mathf.Abs(relPos.z) > myStreet.Length * 5.0f){
                var temp = wrapStreet;
                wrapStreet = onStreet;
                onStreet = temp;
            }
        }

        // After this, we deal with the even more complex logic for rendering intersections. 

        // We start with a check for if the player EXITED an intersection - this means we can de-render most of the intersecting street + it's wrap around clone.
        var otherStreetMaybe = onStreet.onIntersection(relPos);
        if(interStreet != null && otherStreetMaybe == null){
            int id = onStreet.getOtherIntersectionId(interStreet.streetInfo);
            interStreet.downSize(id);
            interStreetWrap.destroyStreet();
            this.interStreetWrap = null;
        }
        
        // Then this next check is for if we are IN an intersection, and thus need to render the intersecting street + wraparound clone.
        interStreet = otherStreetMaybe;
        if (interStreet != null){
            if (!interStreet.fullyRendered){
                // We are right here if the player is standing IN an intersection & we have not yet rendered the intersecting street.
                int id = onStreet.getOtherIntersectionId(interStreet.streetInfo);
                interStreet.fullRender(id);
            } else {
                // We are in here if the player is standing IN an intersection, & we now have to maybe render a wrap around clone.
                this.wraparoundLogic(25.0f, ref this.interStreet, ref this.interStreetWrap);
            }
            
            // This is still the player is standing inside an intersection & now we start checking for if they move into the intersecting street. Similar to the wraparound edges earlier, but a little tougher.
            if (onStreet.xOriented){
                if (Mathf.Abs(relPos.z) > myStreet.Width * 5){
                    // Destroy everything, and reset with our mainStreet set to the intersecting street.
                    myStreet = interStreet.streetInfo;
                    var pos = interStreet.truePos;
                    var orient = interStreet.xOriented;
                    onStreet.destroyStreet();
                    if (wrapStreet != null){
                        wrapStreet.destroyStreet();
                        wrapStreet = null;
                    }
                    interStreet.destroyStreet();
                    interStreet = null;
                    if (interStreetWrap != null){
                        interStreetWrap.destroyStreet();
                        interStreetWrap = null;
                    }
                    onStreet = new renderedStreet(myStreet, pos, orient, false);
                }
            } else {
                if (Mathf.Abs(relPos.x) > myStreet.Width * 5){
                    // Destroy everything, and reset with our mainStreet set to the intersecting street.
                    myStreet = interStreet.streetInfo;
                    var pos = interStreet.truePos;
                    var orient = interStreet.xOriented;
                    onStreet.destroyStreet();
                    if (wrapStreet != null){
                        wrapStreet.destroyStreet();
                        wrapStreet = null;
                    }
                    interStreet.destroyStreet();
                    interStreet = null;
                    if (interStreetWrap != null){
                        interStreetWrap.destroyStreet();
                        interStreetWrap = null;
                    }
                    onStreet = new renderedStreet(myStreet, pos, orient, false);
                }
            }
        }

    }

    // BIG NOTE: WRAPAROUND LOGIC DOES NOT WORK IF THE STREET IS <= RENDER DISTANCE SIZE
    // This function looks at a street the player is standing on, and determines if the 'edge' of the street is within the players render distance
    // If that's the case, we possibly need to render the wraparound clone.
    // If that's not the case, we possibly need to de-render the wraparound clone.
    void wraparoundLogic(float distance, ref renderedStreet street, ref renderedStreet wrapStreet){
        var myInfo = street.streetInfo;
        var playerTrans = this.GetComponent<Transform>();
        var relPos = playerTrans.position - street.truePos; 

        // Uses the RenderedStreet's atEdge check.
        if (street.atEdge(25.0f, relPos))
        {
            // We ARE at the edge. Render some wraparound.
            if (wrapStreet is null){
                Vector3 wrapPos;
                float direction;
                if (street.xOriented){
                    // Determine clone street's true game position.
                    direction = Mathf.Sign(relPos.x);
                    wrapPos = street.truePos + new Vector3(myInfo.Length * 10.0f * direction, 0.0f,0.0f);
                    wrapStreet = new renderedStreet(myInfo, wrapPos, street.xOriented, false);
                    wrapStreet.parent.name = "Wraparound";
                } else {
                    // Determine clone street's true game position.
                    direction = Mathf.Sign(relPos.z);
                    wrapPos = street.truePos + new Vector3(0.0f, 0.0f,myInfo.Length * 10.0f * direction);
                    wrapStreet = new renderedStreet(myInfo, wrapPos, street.xOriented, false);
                    wrapStreet.parent.name = "Wraparound";
                }
                
            }
        } 
        else if (!(wrapStreet is null))
        {
            // We ARE NOT at the edge. Destroy the wraparound.
            wrapStreet.destroyStreet();
            wrapStreet = null;
        }
    }
}

// Class RenderedStreet. This directly handles all the game objects that make up a street.
public class renderedStreet {

    public ScriptObjStreet streetInfo;
    public Vector3 truePos;
    public GameObject parent;
    public bool fullyRendered;
    public bool xOriented;

    public renderedStreet[] myIntersections;

    public GameObject[] myObjects;

    GameObject ground;

    // The constructer takes:
    // ScriptObjStreet, the scriptable object storing juicy information
    // Vector3 Pos, the true-game position of the street-center.
    // bool intersection - is this a full street we want to render, or the small intersection portion.
    // int interIndex - the index of intersection between this and another street. Helps avoid double-rendering some stuff, as well as correctly positioning the street.
    public renderedStreet(ScriptObjStreet street, Vector3 pos, bool orient, bool intersection, int interIndex = -1){
        streetInfo = street;
        truePos = pos;
        xOriented = orient;
        // Create a Parent Game object at the street's true-game position. All other game objects will be childed to this parent, and thus their coordinates will be local to that position.
        parent = new GameObject();
        parent.name = "Street";
        var parentTrans = parent.GetComponent<Transform>();
        parentTrans.position = pos;

        // Create and color the ground.
        ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.parent = parent.transform;
        var groundRenderer = ground.GetComponent<MeshRenderer>();
        groundRenderer.material = street.Color;

        if (intersection){
            // If this is a small intersection, we have a different rendering process than the whole street.
            this.renderIntersection(interIndex);
        } else {
            // Render the full distance of the street.
            this.fullRender(interIndex);
        }        
    }

    void renderIntersection(int i){
        parent.name = "Small intersection";
        // Place a much smaller ground square appropriately centered on the point of intersection.
        var groundTransform = ground.GetComponent<Transform>();
        
        if (xOriented){
            groundTransform.localPosition = new Vector3 (streetInfo.intersections[i].position, 0.0f, 0.0f);
            groundTransform.localScale = new Vector3(2.0f, 1.0f, streetInfo.Width);
        } else {
            groundTransform.localPosition = new Vector3 (0.0f, 0.0f, streetInfo.intersections[i].position);
            groundTransform.localScale = new Vector3(streetInfo.Width, 1.0f, 2.0f);
        }

        myIntersections = new renderedStreet[0];

        fullyRendered = false;
    }

    public void fullRender(int id){
        // Place the full ground.
        var groundTransform = ground.GetComponent<Transform>();
        groundTransform.localPosition = new Vector3(0.0f,0.0f,0.0f);
        if (xOriented){
            groundTransform.localScale = new Vector3(streetInfo.Length, 1, streetInfo.Width);
        } else {
            groundTransform.localScale = new Vector3(streetInfo.Width, 1, streetInfo.Length);
        }
        

        fullyRendered = true;

        // We still want to render intersections the player is not in for A) objects & B) line of sight
        // so we recursively create sub-streets where intersection = true causing them to show less.
        myIntersections = new renderedStreet[streetInfo.intersections.Length];
        
        for(int j = 0; j < streetInfo.intersections.Length; j++){
            var inter = streetInfo.intersections[j];
            Vector3 pos;
            if (xOriented){
                pos = truePos + new Vector3(inter.position, 0.0f, 0.0f) - new Vector3(0.0f, 0.0f, inter.otherPosition);
            } else {
                pos = truePos + new Vector3(0.0f, 0.0f, inter.position) - new Vector3(inter.otherPosition, 0.0f, 0.0f);
            }
            var otherStreet = inter.other;
            int index = this.getOtherIntersectionId(otherStreet);
            
            if (j != id){
                var renderedInter = new renderedStreet(otherStreet, pos, !xOriented, true, index);
                myIntersections[j] = renderedInter;
            } else {
                // Don't! Render a small intersection that we are already standing on.
                myIntersections[j] = null;
            }
        }

        // Then! Go and render all the gameObjects on this street.
        myObjects = new GameObject[streetInfo.objects.Length];
        for (int i = 0; i < streetInfo.objects.Length; i++){
            streetObj info = streetInfo.objects[i];
            myObjects[i] = GameObject.Instantiate(info.myPrefab);
            myObjects[i].transform.parent = parent.transform;

            var objTransform = myObjects[i].GetComponent<Transform>();
            if (xOriented){
                objTransform.localPosition = info.streetPos;
            } else {
                objTransform.localPosition = new Vector3 (info.streetPos.z, info.streetPos.y, info.streetPos.x);
            }
            
        }
    }

    // This takes a players local position on the street & determines if the 'edge' is visible.
    public bool atEdge(float distance, Vector3 playerRelPos){
        bool answer = false;
        if (xOriented){
            answer = Mathf.Abs(playerRelPos.x) + distance > streetInfo.Length * 5.0f;
        } else {
            answer = Mathf.Abs(playerRelPos.z) + distance > streetInfo.Length * 5.0f;
        }
        return answer;
    }

    // This destroys all gameObjects associated with the street.
    public void destroyStreet(){
        GameObject.Destroy(ground);
        GameObject.Destroy(parent);
        for (int i = 0; i < myIntersections.Length; i++){
            var inter = myIntersections[i];
            if (inter != null){
                inter.destroyStreet();
            }
        }
    }

    // This changes a street from being fully rendered to being intersection rendered.
    public void downSize(int j){
        parent.name = "Small Intersection";
        for (int i = 0; i < myIntersections.Length; i++){
            var inter = myIntersections[i];
            if (inter != null){
                inter.destroyStreet();
            }
        }
        var groundTransform = ground.GetComponent<Transform>();
        Vector3 pos;
        if (xOriented){
            pos = new Vector3(streetInfo.intersections[j].position, 0.0f, 0.0f);
        } else {
            pos = new Vector3(0.0f, 0.0f, streetInfo.intersections[j].position);
        }   
        groundTransform.localPosition = pos;
        if (xOriented){
            groundTransform.localScale = new Vector3(2.0f, 1.0f, streetInfo.Width);
        } else {
            groundTransform.localScale = new Vector3(streetInfo.Width, 1.0f, 2.0f);
        }

        myIntersections = new renderedStreet[0];

        fullyRendered = false;
    }

    // This takes a players relative position, and returns the rendered street of an intersection they are standing on.
    public renderedStreet onIntersection(Vector3 playerRelPos){
        int index = -1;
        for (int i = 0; i < streetInfo.intersections.Length; i++){
            Intersection inter = streetInfo.intersections[i];
            if (xOriented){
                if (Mathf.Abs(inter.position - playerRelPos.x) <= 5.0f){
                    index = i;
                }
            } else {
                if (Mathf.Abs(inter.position - playerRelPos.z) <= 5.0f){
                    index = i;
                }
            }
        }
        if (index != -1){
            return myIntersections[index];
        } else {
            return null;
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