using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EXTREMELY simple character controller. Imported from my CMPM 121 project.
// Spacebar rotates the direction the player is facing. Arrow/WASD move in the cardinal directions relative to the direcion player is Facing.

public class PlayerController : MonoBehaviour
{
	// horizontal rotation speed
    public float horizontalSpeed = 1f;
    // vertical rotation speed
    public float verticalSpeed = 1f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    public float speed = 1.0f;
    public float camAngle = 0.0f;
    public GameObject firstCam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate moveVector by the direction player is facing.
        Vector3 moveVector = Quaternion.Euler(0.0f, yRotation, 0.0f) * new Vector3(Input.GetAxis("Horizontal") * this.speed, 0.0f, Input.GetAxis("Vertical") * this.speed);
        this.GetComponent<CharacterController>().SimpleMove(moveVector);

        // Rotate Camera
		float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;
 
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
 
        firstCam.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);
    }
}
