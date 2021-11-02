using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertWrapScript : MonoBehaviour
{

    public float maxY;
    public float minY;
    private CharacterController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.transform.position.y > maxY)
        {
            controller.enabled = false;
            controller.transform.position = new Vector3(controller.transform.position.x, minY, controller.transform.position.z);
            controller.enabled = true;
        }
        else if (controller.transform.position.y < minY)
        {
            controller.enabled = false;
            controller.transform.position = new Vector3(controller.transform.position.x, maxY, controller.transform.position.z);
            controller.enabled = true;
        }
    }
}
