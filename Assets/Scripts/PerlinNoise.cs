using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour {

    long seed;

    public PerlinNoise(long seed)
    {
        this.seed = seed;
    }

    private int random(int x, int range)
    {
        return (int)((x+seed)^5) % range;
    }

    public int getNoise(int x, int range) {
        int chunkSize = 16;
        float noise = 0;
        range = range / 2;

        while (chunkSize > 0)
        {
            int chunkIndex = x / chunkSize;
            float prog = (x % chunkSize) / (chunkSize * 1f);
            float leftRandom = random(chunkIndex, range);
            float rightRandom = random(chunkIndex + 1, range);

            noise += (1 - prog) * leftRandom + prog * rightRandom;
            chunkSize = chunkSize / 2;
            range = range / 2;
            range = Mathf.Max(1, range);
        }

        return (int) Mathf.Round(noise);
    }
}
