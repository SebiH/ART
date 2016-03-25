using UnityEngine;
using Assets.Code.DataProvider;
using System.Linq;
using Assets.Code.Graph;

public class BarObjectsGraph : MonoBehaviour
{
    // Prefab representing a single bar
    // TODO: more requirements in the future? e.g., interfaces?
    public GameObject prefabBar;

    // data that will be visualised
    private float[,] data;

    // matching bars
    private GameObject[,] ingameBars;

    private IDataProvider dataProvider;

	// Use this for initialization
	void Start ()
    {
        dataProvider = new RandomDataProvider();
	}
	

	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // use random data for this event only
            data = new RandomDataProvider().GetData();

            RegenerateGraph();

            // select a random amount of data
            var startPoint = new Vector2(Random.Range(0, data.GetLength(0)), Random.Range(0, data.GetLength(1)));
            var selectedRadius = Random.Range(1f, Mathf.Max(data.GetLength(0), data.GetLength(1)) / 2);

            for (int x = 0; x < ingameBars.GetLength(0); x++)
            {
                for (int y = 0; y < ingameBars.GetLength(1); y++)
                {
                    ingameBars[x, y].GetComponent<DataPoint>().IsHighlighted = ((new Vector2(x, y) - startPoint).magnitude < selectedRadius);
                }
            }
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

        ingameBars = new GameObject[data.GetLength(0), data.GetLength(1)];

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
                    dataPoint.TargetHeight = data[x, y];
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
                        dataPoint.TargetHeight = data[x, y];
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
