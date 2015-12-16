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

            GenerateGraph();
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

                bar.transform.parent = transform;
                bar.transform.localScale = new Vector3(1, data[x, y], 1);
                bar.transform.localPosition = new Vector3(x, data[x,y] / 2, y);

                ingameBars[x, y] = bar;
            }
        }
    }
}
