using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This makes my Sphere Prefab bounce up and down.

public class BallController : MonoBehaviour
{
    public float bounce = 0.0f;
    public float baseY;

    // Start is called before the first frame update
    void Start()
    {
        var trans = this.GetComponent<Transform>();
        this.baseY = trans.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        this.bounce += 0.01f;
        var trans = this.GetComponent<Transform>();
        trans.position = new Vector3(trans.position.x, this.baseY + (Mathf.Sin(this.bounce) * 0.25f), trans.position.z);
    }
}