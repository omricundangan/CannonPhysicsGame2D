using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour {

    public Vertex v1;
    public Vertex v2;
    public float length;
    public bool boundary; //Value used for optimization - see Physics::DetectCollision for more information
    public PhysicsBody parent;

    public Edge(Physics world, PhysicsBody body, Vertex pV1, Vertex pV2, bool pBoundary)
    {
        v1 = pV1; //Set boundary vertices
        v2 = pV2;

        length = Mathf.Sqrt((pV2.position.x - pV1.position.x) * (pV2.position.x - pV1.position.x) + (pV2.position.y - pV1.position.y) * (pV2.position.y - pV1.position.y)); //Calculate the original length
        boundary = pBoundary;

        parent = body;

        body.addEdge(this); //Add the edge to the given body and to the physics simulator
        world.addEdge(this);
    }
}
