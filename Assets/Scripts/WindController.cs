using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindController : MonoBehaviour {

    public float rangeMin;
    public float rangeMax;
    public float wind;
    public bool active;

	// Use this for initialization
	void Start () {
        if (enabled)
        {
            InvokeRepeating("changeWind", 0.0f, 0.5f);
        }
	}

    void changeWind()
    {
        // [rangeMin, rangeMax)
        wind = Random.Range(rangeMin, rangeMax);
    }
}
