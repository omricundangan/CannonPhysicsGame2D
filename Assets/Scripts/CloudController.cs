using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour {

    private WindController w;
    public float movementSpeed;

	// Use this for initialization
	void Start () {
        w = GetComponent<WindController>();
	}

    private void Update()
    {
        if (transform.position.x < 15 && transform.position.x > -15)
        {
            transform.position = transform.position + new Vector3(w.wind * movementSpeed, 0, 0) * Time.deltaTime;
        }
    }
}
