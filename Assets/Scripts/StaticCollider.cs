using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCollider : MonoBehaviour {

    public GameObject physics;
    public GameObject[] item;

	// Use this for initialization
	void Start () {
        item = new GameObject[1];
        item[0] = this.gameObject;
        physics.GetComponent<PhysicsWorld>().addObjects(item);

    }

    // Update is called once per frame
    void Update () {
		
	}
}
