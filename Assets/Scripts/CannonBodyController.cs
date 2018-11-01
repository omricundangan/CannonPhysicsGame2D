using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBodyController : MonoBehaviour {

    public float speed;
    public float leftBoundary = -2.8f;
    public float rightBoundary = 3.7f;

	// Use this for initialization
	void Start () {

	}
	
	// Move left only if we haven't hit the left limit and right if we haven't hit the right limit
	void Update () {
        float moveHorizontal = Input.GetAxis("Horizontal");
        if (moveHorizontal < 0 && transform.position.x > leftBoundary)
        {
            transform.position += new Vector3(moveHorizontal * speed, 0, 0);
        }
        else if(moveHorizontal > 0 && transform.position.x < rightBoundary)
        {
            transform.position += new Vector3(moveHorizontal * speed, 0, 0);
        }
    }
}
