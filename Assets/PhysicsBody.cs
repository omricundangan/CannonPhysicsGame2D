using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour {


	public Vector2 center = new Vector2(0, 0); //Center of mass
	public float MinX, MinY, MaxX, MaxY; //Min/Max coordinates of the bounding box
	public int vertexCount;
	public int edgeCount;
	public float mass;
	public Vertex[] vertices = new Vertex[ Physics.Max_BODY_VERTICES ];
	public Edge[]   edges    = new Edge[ Physics.Max_BODY_EDGES    ];

	public PhysicsBody(Physics parent, int pMass) {
		vertexCount = edgeCount = 0;
		mass = pMass;
		parent.addBody( this ); //Add body to the physics simulator
	}

	public void addEdge(Edge E) {
		edges[ edgeCount++ ] = E;
	}
	
	public void addVertex(Vertex V ) {
		vertices[ vertexCount++ ] = V;
	}

	public MinMax ProjectToAxis(Vector2 Axis) {
		float dotP = Axis.x * vertices[ 0 ].position.x + Axis.y * vertices[ 0 ].position.y;
		MinMax data = new MinMax();
		data.Min = dotP;
		data.Max = dotP; //Set the Minimum and Maximum values to the projection of the first vertex

		for (int i = 0; i < vertexCount; i++ ) {
			dotP = Axis.x * vertices[ i ].position.x + Axis.y * vertices[ i ].position.y; //Project the rest of the vertices onto the axis and extend the interval to the left/right if necessary
			data.Min = Mathf.Min( dotP, data.Min );
			data.Max = Mathf.Max( dotP, data.Max );
		}
		
		return data;
	}

	/**
	 * Calculates the center of mass
	 */
	public void calculateCenter() {
		center.x = 0;
		center.y = 0;

		MinX = 10000.0f;
		MinY = 10000.0f;
		MaxX = -10000.0f;
		MaxY = -10000.0f;

		for (int i = 0; i < vertexCount; i++) {
			center.x += vertices[ i ].position.x;
			center.y += vertices[ i ].position.y;

			MinX = Mathf.Min( MinX, vertices[ i ].position.x );
			MinY = Mathf.Min( MinY, vertices[ i ].position.y );
			MaxX = Mathf.Max( MaxX, vertices[ i ].position.x );
			MaxY = Mathf.Max( MaxY, vertices[ i ].position.y );
		}

		center.x /= vertexCount;
		center.y /= vertexCount;
	}

	/**
	 * Helper function to create a box primitive.
	 * @param x
	 * @param y
	 * @param width
	 * @param height
	 */
	public void createBox(Physics world, int x, int y, int width, int height ) {
		Vertex V1 = new Vertex( world, this, x        , y          );
		Vertex V2 = new Vertex( world, this, x + width, y          );
		Vertex V3 = new Vertex( world, this, x + width, y + height );
		Vertex V4 = new Vertex( world, this, x        , y + height );

		new Edge( world, this, V1, V2, true );
		new Edge( world, this, V2, V3, true );
		new Edge( world, this, V3, V4, true );
		new Edge( world, this, V4, V1, true );

		new Edge( world, this, V1, V3, false );
		new Edge( world, this, V2, V4, false );
	}

}

public class MinMax
{
    public float Min;
    public float Max;
}