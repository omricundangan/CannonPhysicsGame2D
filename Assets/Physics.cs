using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physics : MonoBehaviour {

    public static int Max_BODIES = 256;  //Maximum body/vertex/edgecount the physics simulation can handle
    public static int Max_VERTICES = 1024;
    public static int Max_EDGES = 1024;
    public static int Max_BODY_VERTICES = 10; //Maximum body/edge count a body can contain
    public static int Max_BODY_EDGES = 10;
    private static Vector2 axis = new Vector2(0, 0);

    private int screenWidth;
    private int screenHeight;

    private Vector2 gravity;
    private int bodyCount;
    private int vertexCount;
    private int edgeCount;
    private Vertex[] vertices = new Vertex[Max_VERTICES];
    private Edge[] edges = new Edge[Max_EDGES];
    private PhysicsBody[] bodies = new PhysicsBody[Max_BODIES];
    private float timestep;
    private int iterations;

    /**
	 * Inner class CollisionInfo
	 * @author Craig Mitchell
	 */
    public static class CollisionInfo
    {
        public static float depth;
        public static Vector2 normal = new Vector2(0, 0);
        public static Edge e;
        public static Vertex v;
    };

    /**
	 * Sets the force on each vertex to the gravity force. You could of course apply other forces like magnetism etc.
	 */
    private void updateForces()
    {
        for (int I = 0; I < vertexCount; I++)
        {
            this.vertices[I].acceleration = this.gravity;
        }
    }

    /**
	 * Updates the vertex position
	 */
    private void updateVerlet()
    {
        float tempX;
        float tempY;

        for (int i = 0; i < vertexCount; i++)
        {
            Vertex v = vertices[i];
            tempX = v.position.x;
            tempY = v.position.y;
            v.position.x += v.position.x - v.oldPosition.x + v.acceleration.x * timestep * timestep;
            v.position.y += v.position.y - v.oldPosition.y + v.acceleration.y * timestep * timestep;
            v.oldPosition.x = tempX;
            v.oldPosition.y = tempY;
        }
    }

    private void updateEdges()
    {
        for (int i = 0; i < edgeCount; i++)
        {
            Edge e = edges[i];

            // Calculate the vector between the two vertices
            float v1v2x = e.v2.position.x - e.v1.position.x;
            float v1v2y = e.v2.position.y - e.v1.position.y;

            float v1v2Length = Mathf.Sqrt(v1v2x * v1v2x + v1v2y * v1v2y); ; //Calculate the current distance
            float diff = v1v2Length - e.length; //Calculate the difference from the original length

            // Normalise
            float len = 1.0f / Mathf.Sqrt(v1v2x * v1v2x + v1v2y * v1v2y);
            v1v2x *= len;
            v1v2y *= len;

            // Push both vertices apart by half of the difference respectively so the distance between them equals the original length
            e.v1.position.x += v1v2x * diff * 0.5f;
            e.v1.position.y += v1v2y * diff * 0.5f;
            e.v2.position.x -= v1v2x * diff * 0.5f;
            e.v2.position.y -= v1v2y * diff * 0.5f;
        }
    }

    private void iterateCollisions()
    {
        for (int i = 0; i < iterations; i++)
        { //Repeat this a few times to give more exact results
          //A small 'hack' that keeps the vertices inside the screen. You could of course implement static objects and create
          //four to serve as screen boundaries, but the Max/Min method is faster
            for (int t = 0; t < vertexCount; t++)
            {
                Vector2 pos = vertices[t].position;
                pos.x = Mathf.Max(Mathf.Min(pos.x, (float)screenWidth), 0.0f);
                pos.y = Mathf.Max(Mathf.Min(pos.y, (float)screenHeight), 0.0f);
            }

            updateEdges(); //Edge correction step

            for (int j = 0; j < this.bodyCount; j++)
            {
                this.bodies[j].calculateCenter(); //Recalculate the center
            }

            for (int b1 = 0; b1 < bodyCount; b1++)
            { //Iterate trough all bodies
                for (int b2 = 0; b2 < bodyCount; b2++)
                {
                    if (b1 != b2)
                    {
                        if (bodiesOverlap(bodies[b1], bodies[b2]))
                        { //Test the bounding boxes
                            if (detectCollision(bodies[b1], bodies[b2]))
                            { //If there is a collision, respond to it
                                processCollision();
                            }
                        }
                    }
                }
            }
        }
    }

    private static bool detectCollision(PhysicsBody b1, PhysicsBody b2)
    {
        float MinDistance = 10000.0f; //Initialize the length of the collision vector to a relatively large value

        for (int i = 0; i < b1.edgeCount + b2.edgeCount; i++)
        { //Just a fancy way of iterating through all of the edges of both bodies at once
            Edge e;

            if (i < b1.edgeCount)
            {
                e = b1.edges[i];
            }
            else
            {
                e = b2.edges[i - b1.edgeCount];
            }

            //This will skip edges that lie totally inside the bodies, as they don't matter.
            //The boundary flag has to be set manually and defaults to true
            if (e.boundary == false)
            {
                continue;
            }

            // Calculate the perpendicular to this edge and normalize it
            axis.x = e.v1.position.y - e.v2.position.y;
            axis.y = e.v2.position.x - e.v1.position.x;

            // Normalise
            float len = 1.0f / Mathf.Sqrt(axis.x * axis.x + axis.y * axis.y);
            axis.x *= len;
            axis.y *= len;

            // Project both bodies onto the perpendicular
            MinMax dataA = b1.ProjectToAxis(axis);
            MinMax dataB = b2.ProjectToAxis(axis);

            float distance = IntervalDistance(dataA.Min, dataA.Max, dataB.Min, dataB.Max); //Calculate the distance between the two intervals

            // If the intervals don't overlap, return, since there is no collision
            if (distance > 0.0f)
            {
                return false;
            }
            else if (Mathf.Abs(distance) < MinDistance)
            {
                MinDistance = Mathf.Abs(distance);

                // Save collision information for later
                CollisionInfo.normal.x = axis.x;
                CollisionInfo.normal.y = axis.y;
                CollisionInfo.e = e;    //Store the edge, as it is the collision edge
            }
        }

        CollisionInfo.depth = MinDistance;

        if (CollisionInfo.e.parent != b2)
        { //Ensure that the body containing the collision edge lies in B2 and the one conatining the collision vertex in B1
            PhysicsBody temp = b2;
            b2 = b1;
            b1 = temp;
        }

        // int Sign = SGN( CollisionInfo.Normal.multiplyVal( B1.Center.Minus(B2.Center) ) ); //This is needed to make sure that the collision normal is pointing at B1
        float xx = b1.center.x - b2.center.x;
        float yy = b1.center.y - b2.center.y;
        float mult = CollisionInfo.normal.x * xx + CollisionInfo.normal.y * yy;

        // Remember that the line equation is N*( R - R0 ). We choose B2->Center as R0; the normal N is given by the collision normal

        if (mult < 0)
        {
            // Revert the collision normal if it points away from B1
            CollisionInfo.normal.x = 0 - CollisionInfo.normal.x;
            CollisionInfo.normal.y = 0 - CollisionInfo.normal.y;
        }

        float smallestD = 10000.0f; //Initialize the smallest distance to a large value

        for (int i = 0; i < b1.vertexCount; i++)
        {
            // Measure the distance of the vertex from the line using the line equation
            // float Distance = CollisionInfo.Normal.multiplyVal( B1.Vertices[I].Position.Minus(B2.Center) );
            xx = b1.vertices[i].position.x - b2.center.x;
            yy = b1.vertices[i].position.y - b2.center.y;
            float distance = CollisionInfo.normal.x * xx + CollisionInfo.normal.y * yy;

            if (distance < smallestD)
            { //If the measured distance is smaller than the smallest distance reported so far, set the smallest distance and the collision vertex
                smallestD = distance;
                CollisionInfo.v = b1.vertices[i];
            }
        }

        return true; //There is no separating axis. Report a collision!
    }

    private static void processCollision()
    {
        Vertex e1 = CollisionInfo.e.v1;
        Vertex e2 = CollisionInfo.e.v2;

        float collisionVectorX = CollisionInfo.normal.x * CollisionInfo.depth;
        float collisionVectorY = CollisionInfo.normal.y * CollisionInfo.depth;

        float t;
        if (Mathf.Abs(e1.position.x - e2.position.x) > Mathf.Abs(e1.position.y - e2.position.y))
        {
            t = (CollisionInfo.v.position.x - collisionVectorX - e1.position.x) / (e2.position.x - e1.position.x);
        }
        else
        {
            t = (CollisionInfo.v.position.y - collisionVectorY - e1.position.y) / (e2.position.y - e1.position.y);
        }

        float lambda = 1.0f / (t * t + (1 - t) * (1 - t));
        float edgeMass = t * e2.parent.mass + (1f - t) * e1.parent.mass; //Calculate the mass at the intersection point
        float invCollisionMass = 1.0f / (edgeMass + CollisionInfo.v.parent.mass);

        float ratio1 = CollisionInfo.v.parent.mass * invCollisionMass;
        float ratio2 = edgeMass * invCollisionMass;

        e1.position.x -= collisionVectorX * ((1 - t) * ratio1 * lambda);
        e1.position.y -= collisionVectorY * ((1 - t) * ratio1 * lambda);
        e2.position.x -= collisionVectorX * (t * ratio1 * lambda);
        e2.position.y -= collisionVectorY * (t * ratio1 * lambda);

        CollisionInfo.v.position.x += collisionVectorX * ratio2;
        CollisionInfo.v.position.y += collisionVectorY * ratio2;
    }

    private static float IntervalDistance(float MinA, float MaxA, float MinB, float MaxB)
    {
        if (MinA < MinB)
        {
            return MinB - MaxA;
        }
        else
        {
            return MinA - MaxB;
        }
    }

    /**
	 * Used for optimization to test if the bounding boxes of two bodies overlap.
	 * @param B1
	 * @param B2
	 * @return
	 */
    private static bool bodiesOverlap(PhysicsBody B1, PhysicsBody B2)
    {
        return (B1.MinX <= B2.MaxX) && (B1.MinY <= B2.MaxY) && (B1.MaxX >= B2.MinX) && (B2.MaxY >= B1.MinY);
    }


    public void update()
    {
        updateForces();
        updateVerlet();
        iterateCollisions();
    }

    /**
	 * Adds new elements to the simulation
	 * @param Body
	 */
    public void addBody(PhysicsBody Body)
    {
        this.bodies[this.bodyCount++] = Body;
    }

    public void addEdge(Edge E)
    {
        this.edges[this.edgeCount++] = E;
    }

    public void addVertex(Vertex V)
    {
        this.vertices[this.vertexCount++] = V;
    }

    public Vertex findVertex(int X, int Y)
    {
        Vertex nearestVertex = null;
        float MinDistance = 1000.0f;

        Vector2 coords = new Vector2((float)X, (float)Y);

        for (int i = 0; i < vertexCount; i++)
        {
            float distance = Mathf.Sqrt((vertices[i].position.x - coords.x) * (vertices[i].position.x - coords.x) +
                (vertices[i].position.y - coords.y) * (vertices[i].position.y - coords.y));

            if (distance < MinDistance)
            {
                nearestVertex = vertices[i];
                MinDistance = distance;
            }
        }

        return nearestVertex;
    }



    public Physics(int width, int height, float GravitationX, float GravitationY, int pIterations)
    {
        this.screenWidth = width;
        this.screenHeight = height;
        this.bodyCount = 0;
        this.vertexCount = 0;
        this.edgeCount = 0;
        this.gravity = new Vector2(GravitationX, GravitationY);
        this.iterations = pIterations;
        this.timestep = 1.0f;
    }
}
