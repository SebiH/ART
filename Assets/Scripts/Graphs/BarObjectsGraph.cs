using UnityEngine;
using System.Collections;

public class BarObjectsGraph : MonoBehaviour
{
    [Range(1, 100)]
    public int width;

    [Range(1, 100)]
    public int height;

    // Prefab representing a single bar
    // TODO: more requirements in the future? e.g., interfaces?
    public GameObject prefabBar;

    // data that will be visualised
    private float[,] data;

    // matching bars
    private GameObject[,] ingameBars;

	// Use this for initialization
	void Start ()
    {
	
	}
	

	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKey(KeyCode.Mouse0))
        {
            data = new float[width, height];

            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    data[x, y] = Random.Range(0f, 5f);
                }
            }

            RegenerateGraph();
        }
	}


    private void GenerateGraph()
    {
        // clean up previous bars
        if (ingameBars != null)
        {
            foreach (var bar in ingameBars)
            {
                Destroy(bar);
            }
        }

        ingameBars = new GameObject[width, height];

        for (int x = 0; x < ingameBars.GetLength(0); x++)
        {
            for (int y = 0; y < ingameBars.GetLength(1); y++)
            {
                var bar = Instantiate(prefabBar);
                var dataPoint = bar.GetComponent<DataPoint>();

                if (!dataPoint)
                {
                    print("Error: Attach DataPoint script to graph prefab object!");
                }
                else
                {
                    bar.transform.parent = transform;
                    dataPoint.SetHeight(data[x, y]);
                    dataPoint.SetPosition(x, y);
                }

                ingameBars[x, y] = bar;
            }
        }
    }

    /**
     *  Reuses existing bars, if possible.
     */
    private void RegenerateGraph()
    {
        var isInitialised = (ingameBars != null);
        var hasSameDimensions = (isInitialised && ingameBars.GetLength(0) == data.GetLength(0) && ingameBars.GetLength(1) == data.GetLength(1));

        if (isInitialised && hasSameDimensions)
        {
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    var dataPoint = ingameBars[x, y].GetComponent<DataPoint>();

                    if (!dataPoint)
                    {
                        print("Error: Attach DataPoint script to graph prefab object!");
                    }
                    else
                    {
                        dataPoint.SetHeight(data[x, y]);
                    }
                }
            }
        }
        else
        {
            GenerateGraph();
        }

    }
}
