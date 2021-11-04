using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script heavily borrows from the wonderful Brackeys tutorial on portals in unity here: https://github.com/Brackeys/Portal-In-Unity
// as well as this very helpful answer: https://gamedev.stackexchange.com/questions/166757/how-can-i-transform-a-world-space-point-to-a-cameras-screen-coordinate

public class InfiniteView : MonoBehaviour
{

    // user accesable vars
    public Camera playerCamera;
    public Camera infViewCamera; // the camera that shows the scene from the perspective of the other side of the portal viewed fromt the player camera's angle
    public Renderer infViewRenderingPlane; // The object to render the infinite view onto, it doesn't need to be a plane though.
    public Material materialForViewRenderPlane; // this material will get applied to the rendering plane, it should have the infiViewShader
    public float oppositeSideDistance; // since in my settup, I just want visual repetition along one axis, the faces must be parralell, we can ignore rotation and assume the center of the "portal viewport" is this distance exactly behind this trasform which shoud be the parent of the viewRenderingPlane and define the realworld "portal gate location"

    public float fieldOfViewMultiplier; // sets the render width

    // private vars:
    private RenderTexture renderTextr;
    private MaterialPropertyBlock renderMatBlock;
    private Vector3 renderPlanePositionCorrectionVector;
    private static Matrix4x4 Cam180FlipTransform;

    private Vector3 StartingVectorFromPlayerToPortal;


    // Start is called before the first frame update
    void Start()
    {
        // Setup the inf view camera to render to a new rendertexture
        if (infViewCamera.targetTexture != null)
        {
            infViewCamera.targetTexture.Release();
        }
        renderTextr = new RenderTexture(Mathf.FloorToInt(Screen.width * fieldOfViewMultiplier), Mathf.FloorToInt(Screen.height * fieldOfViewMultiplier), 24, RenderTextureFormat.ARGBHalf);
        renderTextr.antiAliasing = 4; /// for unknown reasons enabling anti-aliasing is the only way unity will "see" the rendertexture infinitely in frame
        infViewCamera.targetTexture = renderTextr;

        // Setup the view plane with the material set in the inspector (that material should use the infiniteViewShader), then setup a new set of properties to that material that will get sent to the shader.
        infViewRenderingPlane.material = materialForViewRenderPlane;
        renderMatBlock = new MaterialPropertyBlock();
        renderMatBlock.SetTexture("_MainTex", renderTextr);
        infViewRenderingPlane.SetPropertyBlock(renderMatBlock);

        // save a tranform matrix to flip the camera view upside down (we could also just make the camera rotated 180 degrees along the view axis instead).
        Cam180FlipTransform = RotationMatrixAroundAxis(new Ray(new Vector3(0, 0, 0), new Vector3(0, 0, 1)), 180);

        StartingVectorFromPlayerToPortal = this.transform.position - playerCamera.transform.position;
    }

    private Matrix4x4 RotationMatrixAroundAxis(Ray axis, float rotation)
    {
        // from: https://gamedev.stackexchange.com/questions/138637/unity-matrix4x4-to-rotate-a-point-around-a-pivot-axis
        return Matrix4x4.TRS(-axis.origin, Quaternion.AngleAxis(rotation, axis.direction), Vector3.one)
            * Matrix4x4.TRS(axis.origin, Quaternion.identity, Vector3.one);
    }

    void setInfViewCameraPos()
    {
        // code here based on the portal tutorial by brakeys..
        Vector3 PortalPostion = this.transform.position; // this should be the empty game object which this script is applied to.
        Vector3 opositeSidePoint = this.transform.forward * -oppositeSideDistance;

        Vector3 playerOffsetFromPortal = playerCamera.transform.position + opositeSidePoint;
        infViewCamera.transform.position = playerOffsetFromPortal;


        // This is here to solve the problem of the render texture shrinking to a point as the camera gets very close to it.
        // the idea is we send a vector to how close the player camera is to the face of the render plane (ie: dist between teh player camera and the portal along the axis perpendicular to the portal/ parent transform)
        float camToPortalPerpendicularDist = Vector3.Dot(this.transform.forward, PortalPostion - playerCamera.transform.position);
        float portalToRenderPlanePerpendicularDist = Vector3.Dot(this.transform.forward, this.infViewRenderingPlane.transform.position - PortalPostion);

        // you may want to edit this formula to suit your setup, or set to zero. it's only an aproximation in y=mx+b + (portalToRenderPlanePerpendicularDist = undoing any physical distance between the portal locaton and render plane);
        float correctionAmmount = -0.3f * camToPortalPerpendicularDist + 2.5f;
        renderPlanePositionCorrectionVector = this.transform.forward * correctionAmmount;


        // If the oposite-side view (view through the "portal") and render plane ("portal" gate location) were at different angles, we would need this:
        // See: https://github.com/Brackeys/Portal-In-Unity/blob/master/Portal/Assets/PortalCamera.cs for the full funtion body
        // float angularDifferenceBetweenPortalRotations = Quaternion.Angle(portal.rotation, viewPoint.rotation);
        // Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);
        // ...

        // since the portals are parallel, this will suffice:
        Vector3 newCameraDirection = playerCamera.transform.forward;
        infViewCamera.transform.rotation = playerCamera.transform.rotation;// Quaternion.LookRotation(newCameraDirection, Vector3.up);
        infViewCamera.fieldOfView = playerCamera.fieldOfView * fieldOfViewMultiplier;
        this.transform.position = StartingVectorFromPlayerToPortal + playerCamera.transform.position;
    }

    public void RenderThisInfiniteView()
    {
        // Key idea: The rendertexture must be perspective corrected to appear to have been projected from the Player Camera, as if the camera was a movie projector.
        // we acomplish this using the view projection matrix of the player camera and sending it to the shader. The shader applies (multiplies) the matrix transform of the player camera to the render texture.

        // Set this infinite view plane to be perspective corrected to the player's camera:
        // Matrix4x4 camProjectionMatrix = infViewCamera.nonJitteredProjectionMatrix// playerCamera.nonJitteredProjectionMatrix
        Matrix4x4 viewProjection = infViewCamera.nonJitteredProjectionMatrix * Cam180FlipTransform * playerCamera.transform.worldToLocalMatrix;
        renderMatBlock.SetMatrix("_CameraMatrix", viewProjection);
        renderMatBlock.SetVector("_CamCorrectionOffset", renderPlanePositionCorrectionVector); // this is a translation vector that fakes moving the Plane that the view is projected onto in the direction of this vector, without moving the verticies.
        infViewRenderingPlane.SetPropertyBlock(renderMatBlock);
    }

    // Update is called once per frame
    void Update()
    {
        setInfViewCameraPos();
    }

    void LateUpdate()
    {
        RenderThisInfiniteView();
    }

}
