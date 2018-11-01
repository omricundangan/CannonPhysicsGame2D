using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsWorld : MonoBehaviour {

    public GameObject[][] items;
    public int index = 0;
    public int size = 0;

	// Use this for initialization
	void Start () {
        items = new GameObject[size][];
    }

    public void addObjects(GameObject[] objects)
    {
        items[index] = objects;
        index++;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
