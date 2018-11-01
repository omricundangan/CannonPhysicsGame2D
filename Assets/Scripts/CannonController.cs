using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour {

    public float minAngle = -85.0f;
    public float maxAngle = 20.0f;
    public float cannonAngle = 0.0f;
    public float initialVelocityCRB;
    public GameObject cannonballCRB;
    public GameObject wind;
    public GameObject physics;
    public GameObject poof;
    public float cannonballRestitution;

	// Use this for initialization
	void Start () {
        cannonAngle = -cannonAngle;     // Rotation in Z axis is inverted
	}
	
	// Update is called once per frame
	void Update () {


        // Control cannon
        float moveVertical = Input.GetAxis("Vertical");
        cannonAngle += -moveVertical;
        cannonAngle = Mathf.Clamp(cannonAngle, -maxAngle, minAngle);    // We reverse it because the rotation in Z axis is inverted
        transform.localRotation = Quaternion.AngleAxis(cannonAngle, Vector3.forward);

        // Verlet Integration
        if (Input.GetKeyDown(KeyCode.Space) && moveVertical < 0.75 && moveVertical > -0.75)
        {
            Quaternion r = this.gameObject.transform.GetChild(0).GetChild(0).rotation;

            var cannon = (GameObject)Instantiate(
                cannonballCRB,
                 this.gameObject.transform.GetChild(0).GetChild(0).position,
                 Quaternion.Euler(r.x, r.y, 0));

            CustomRigidBody crb = cannon.GetComponent<CustomRigidBody>();

            // Set our cannonball's properties, i.e. initial velocity, restitution, etc.
            crb.theta = -cannonAngle;
            crb.v0 = initialVelocityCRB;
            crb.w = this.wind.GetComponent<WindController>();
            crb.affectedByWind = true;
            crb.restitution = cannonballRestitution;
            crb.physics = physics;

            var boom = (GameObject)Instantiate(
                poof,
                 this.gameObject.transform.GetChild(0).GetChild(0).position,
                 this.gameObject.transform.GetChild(0).GetChild(0).rotation);

            Destroy(boom, 0.05f);
        }
    }
}
