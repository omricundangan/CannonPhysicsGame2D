using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorController : MonoBehaviour {

    public GameObject dirtPrefab;
    public GameObject topPrefab;

    public int minX;
    public int maxX;
    public int minY;
    public int maxY;

    private float leftMax;
    private float rightMax;

    public GameObject[] mtnTopBlocks;
    public GameObject[] mtnLeftBlocks;
    public GameObject[] mtnRightBlocks;

    public GameObject physics;
    public int numOuterBlocks;

    PerlinNoise noise;

	// Use this for initialization
	void Start () {
        // Initialize the array
        mtnTopBlocks = new GameObject[(maxX - minX) * 3];

        // Indicate how many outer blocks we want for collision. 4 works well
        numOuterBlocks = 4;

        // Generate mountaintop with 1D Perlin
        noise = new PerlinNoise(Random.Range(1000000,10000000));
        Regenerate();

        // Generate Edges of the mountain
        generateRightEdge();
        generateLeftEdge();

        // Spawn the middle of the mountain
        var spawnedBlock = Instantiate(dirtPrefab, new Vector3(0, -7.06f, 0), Quaternion.identity);
        spawnedBlock.transform.localScale = new Vector3(12.1f, 3.91f, 0);

        // Add the outer mountain blocks as objects to our Physics Controller for collision detection
        physics.GetComponent<PhysicsWorld>().addObjects(mtnTopBlocks);
        physics.GetComponent<PhysicsWorld>().addObjects(mtnRightBlocks);
        physics.GetComponent<PhysicsWorld>().addObjects(mtnLeftBlocks);
    }

    // Generate hilltop via Perlin
    private void Regenerate()   
    {
        int index = 0;

        bool toggle = false;
        float width = dirtPrefab.transform.lossyScale.x;
        float height = dirtPrefab.transform.lossyScale.y;

        for (int i = minX; i < maxX; i++)   // columns
        {
            int colHeight = 2 + noise.getNoise(i - minX, maxY - minY - 2);
            for(int j = minY; j < minY + colHeight; j++) // rows
            {
                
                var block = Instantiate(dirtPrefab, new Vector3(i * width, j * height, 0), Quaternion.identity);
                if (i == maxX - 1 && j == minY + colHeight - 1)
                {
                    rightMax = block.transform.position.y;
                }else if(i == minX && !toggle && j == minY + colHeight - 1)
                {
                    leftMax = block.transform.position.y;
                    toggle = true;
                }
                if(j == minY + colHeight - 1 || j == minY + colHeight - 2 || j == minY + colHeight - 3)
                {
                    GameObject spawnedBlock = (GameObject)Instantiate(topPrefab, new Vector3(i * width, j * height, 0), Quaternion.identity);
                    mtnTopBlocks[index] = spawnedBlock; // store ONLY the outer blocks in array for collision detection
                    index++;
                }
            }
        }
    }

    private void generateRightEdge()
    {
        float width = dirtPrefab.transform.lossyScale.x;
        float height = dirtPrefab.transform.lossyScale.y;

        float reduction = 0.35f;

        int index = 0;
        mtnRightBlocks = new GameObject[(((int)Mathf.Ceil((-9.5f - rightMax) / -reduction)) * numOuterBlocks)];

        // These max values are just to check if Ive gotten the highest block of this column.
        // I collect the two top blocks of each column on the edge of the mountain and use them for collision detection
        
        float j = rightMax - 0.20f;

        // Start at the highest block on the right side of the generated perlin terrain
        // spawn the entire column below the starting block then for the next column move the starting block down
        // keep iterating until the starting block is ground level
        float counter = 0.25f;
        while (rightMax - counter > -9.5f)
        {
            int max = 0;
            while (j > -9.5)
            {
                GameObject prefab;
                if (max < numOuterBlocks)    // Just changing the spawning prefabs if its the top two blocks of the column
                {
                    prefab = topPrefab;
                    max++;
                }
                else
                {
                    prefab = dirtPrefab;
                }

                GameObject spawnedBlock = (GameObject)Instantiate(prefab, new Vector3((maxX + counter -0.25f) * width, j - height, 0), Quaternion.identity);
                j = spawnedBlock.transform.position.y;

                if (prefab == topPrefab)    // topPrefab = top 2 blocks of the column
                {
                    mtnRightBlocks[index] = spawnedBlock;   // store ONLY the outer blocks for collision detection
                    index++;
                }
            }
            counter += reduction;
            j = rightMax - counter;

        }
    }

    // the exact same thing as rightEdge with some reversed x values
    private void generateLeftEdge()
    {
        float width = dirtPrefab.transform.lossyScale.x;
        float height = dirtPrefab.transform.lossyScale.y;

        float reduction = 0.35f;
        int index = 0;
        mtnLeftBlocks = new GameObject[(((int)Mathf.Ceil((-9.5f - leftMax) / -reduction)) * numOuterBlocks)];

        float j = leftMax - 0.20f;

        float counter = 0.25f;
        while (leftMax - counter > -9.5f)
        {
            int max = 0;
            while (j > -9.5)
            {
                GameObject prefab;
                if (max < numOuterBlocks)    // Just changing the spawning prefabs if its the top two blocks of the column
                {
                    prefab = topPrefab;
                    max++;
                }
                else
                {
                    prefab = dirtPrefab;
                }

                GameObject spawnedBlock = (GameObject) Instantiate(prefab, new Vector3((minX - counter - 0.25f) * width, j - height, 0), Quaternion.identity);
                j = spawnedBlock.transform.position.y;

                if (prefab == topPrefab)
                {
                    mtnLeftBlocks[index] = spawnedBlock; // store the outer blocks for collision detection
                    index++;
                }
            }
            counter += reduction;
            j = leftMax - counter;

        }
    }
}
