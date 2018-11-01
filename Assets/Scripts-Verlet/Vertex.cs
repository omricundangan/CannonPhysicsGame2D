using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour {

    public Vector2 position;
    public Vector2 oldPosition;
    public Vector2 acceleration;
    public PhysicsBody parent;

    public Vertex(Physics world, PhysicsBody body, float posX, float posY)
    {
        position = new Vector2(posX, posY);
        oldPosition = new Vector2(posX, posY);

        parent = body;

        body.addVertex(this); //Add the vertex to the given body and to the physics simulator
        world.addVertex(this);
    }
}
