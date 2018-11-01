using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRigidBody : MonoBehaviour {

    private Vector3 posOld;
    private Vector3 pos;
    private Vector3 accel;
    private Vector3 forces;
    public Vector3 v;
    // public float vM2;

    public float g = 9.8f;
    public float theta;
    public float v0;
    public float restitution;
    public bool affectedByWind = true;
    public WindController w;

    public GameObject physics;
    public GameObject[][] objects;

	// Use this for initialization
	void Start () {
        // get all the collidable objects in our game world
        objects = physics.GetComponent<PhysicsWorld>().items;

        // Initialize positions
        pos = transform.position;
        posOld = transform.position;

        // Apply initial velocity at angle theta as an impulse
        Vector3 initialVelocity = new Vector3(
            -(v0 * Mathf.Cos(theta * Mathf.PI / 180)), 
            v0 * Mathf.Sin(theta * Mathf.PI / 180), 
            0);
        AddForce(initialVelocity);
    }

    // FixedUpdate for physics
    void FixedUpdate () {
        // get velocity at this time (this is actually one step behind)
        v = (pos - posOld) / Time.deltaTime;

        // out of bounds, destroy
        if(pos.x > 20 || pos.x < -20 || pos.y < -8.75)
        {
            Destroy(this.gameObject);
        }
        if (affectedByWind)
        {
            AddWind();
        }
        // Collision detection
        if (CheckCollisions())
        {
            ResolveCollision(); // collision resolution
        }
        AddForce(new Vector3(0, -g * Time.deltaTime, 0)); // add gravity
        UpdateVerlet();
    }

    // Impulse add a force
    void AddForce(Vector3 impulse)     // Add force via vector
    {
        forces += impulse;
    }
    void AddForce(float velocity, float angle)  // Add force via magnitude and angle
    {
        forces += new Vector3(
            velocity * Mathf.Cos(angle * Mathf.PI / 180),
            velocity * Mathf.Sin(angle * Mathf.PI / 180),
            0);
    }

    // Adds a wind force but makes sure not to push back our item
    void AddWind()
    {
        if(pos.y > 0)   // Wind occurs at y > 0
        {
            if (pos.x - posOld.x <= 0)
            {
                AddForce(w.wind, 0);
            }else if (w.wind < 0 && !(pos.x - posOld.x <= 0))
            {
                AddForce(w.wind, 0);
            }
        }
    }

    // Verlet integration
    void UpdateVerlet()
    {
        Vector3 temp = pos;
        pos += pos - posOld + (forces * Time.deltaTime);
        posOld = temp;
        transform.position = pos;
        forces = new Vector3(0, 0, 0);  // reset the forces being applied
    }

    // Check for collisions using bounding boxes
    bool CheckCollisions()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            for (int j = 0; j < objects[i].Length; j++)
            {
                if (objects[i][j] != null)
                {
                    Bounds a = this.gameObject.GetComponent<SpriteRenderer>().bounds;
                    Bounds b = objects[i][j].GetComponent<SpriteRenderer>().bounds;

                    if(AABBAABB(this.gameObject, objects[i][j]))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Decouple collided objects before resolving so we don't end up resolving a resolved force
    // Resolve using impulse forces.
    void ResolveCollision()
    {
        if (ColInfo.nEnter.x >  ColInfo.nEnter.y)
        {
            pos = pos + (ColInfo.nEnter * ColInfo.penetration);
            AddForce(new Vector3(-(1 + restitution) * v.x, 0, 0));
        }
        else
        {
            pos = pos + (ColInfo.nEnter * ColInfo.penetration);
            AddForce(new Vector3(0, -(1 + restitution) * v.y, 0));  
        }
    }

    // Verlet integration
    void oldUpdateVerlet()
    {        
        // Gravity is always the acceleration ?
        accel = new Vector3(0, -g, 0);

        Vector3 temp = pos;
        pos += pos - posOld + (accel * Time.deltaTime * Time.deltaTime) + (forces * Time.deltaTime);
        posOld = temp;
        transform.position = pos;
        forces = new Vector3(0, 0, 0);  // reset the forces being applied
    }

    public static bool AABBAABB(GameObject objA, GameObject objB)
    {
        // Minimum Translation Vector
        float mtvDistance = float.MaxValue;             // Set current minimum distance (max float value so next value is always less)
        Vector3 mtvAxis = new Vector3();                // Axis along which to travel with the minimum distance

        Bounds a = objA.GetComponent<SpriteRenderer>().bounds;
        Bounds b = objB.GetComponent<SpriteRenderer>().bounds;

        // Test axes for separation
        if (!testAxis(Vector3.right, a.min.x, a.max.x, b.min.x, b.max.x, ref mtvAxis, ref mtvDistance))
            return false;

        if (!testAxis(Vector3.up, a.min.y, a.max.y, b.min.y, b.max.y, ref mtvAxis, ref mtvDistance))
            return false;

        ColInfo.isIntersecting = true;

        // Calculate Minimum Translation Vector (MTV) [normal * penetration]
        ColInfo.nEnter = Vector3.Normalize(mtvAxis);

        // Multiply the penetration depth by itself plus a small increment
        // When the penetration is resolved using the MTV, it will no longer intersect
        ColInfo.penetration = (float)Mathf.Sqrt(mtvDistance) * 1.001f;

        return true;
    }

    private static bool testAxis(Vector3 axis, float minA, float maxA, float minB, float maxB, ref Vector3 mtvAxis, ref float mtvDistance)
    {
        // Separating Axis Theorem
        float axisLengthSquared = Vector3.Dot(axis, axis);

        // If the axis is degenerate then ignore
        if (axisLengthSquared < 1.0e-8f)
            return true;

        // Calculate the two possible overlap ranges
        // Either we overlap on the left or the right sides
        float d0 = (maxB - minA);   // 'Left' side
        float d1 = (maxA - minB);   // 'Right' side

        // Intervals do not overlap, so no intersection
        if (d0 <= 0.0f || d1 <= 0.0f)
            return false;

        // Find out if we overlap on the 'right' or 'left' of the object.
        float overlap = (d0 < d1) ? d0 : -d1;

        // The mtd vector for that axis
        Vector3 sep = axis * (overlap / axisLengthSquared);

        // The mtd vector length squared
        float sepLengthSquared = Vector3.Dot(sep, sep);

        // If that vector is smaller than our computed Minimum Translation Distance use that vector as our current MTV distance
        if (sepLengthSquared < mtvDistance)
        {
            mtvDistance = sepLengthSquared;
            mtvAxis = sep;
        }

        return true;
    }
}

// Holds collision info
public static class ColInfo{
    public static bool isIntersecting;
    public static Vector3 nEnter;
    public static float penetration;
}
