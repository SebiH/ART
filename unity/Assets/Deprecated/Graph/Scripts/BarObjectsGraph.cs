using UnityEngine;

namespace Assets.Deprecated.Graph
{
    public class BarObjectsGraph : MonoBehaviour
    {
        // Prefab representing a single bar
        // TODO: more requirements in the future? e.g., interfaces?
        public GameObject prefabBar;

        // data that will be visualised
        private float[,] data;

        // matching bars
        private GameObject[,] ingameBars;

        private DataProvider dataProvider;

        // Use this for initialization
        void Start()
        {
            dataProvider = new RandomDataProvider();
            data = dataProvider.GetData();
            RegenerateGraph();
        }


        // Update is called once per frame
        void Update()
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
                        ingameBars[x, y].GetComponent<DataPointOld>().IsHighlighted = ((new Vector2(x, y) - startPoint).magnitude < selectedRadius);
                    }
                }
            }
        }

        public void HighlightRandomData()
        {
            var startPoint = new Vector2(Random.Range(0, data.GetLength(0)), Random.Range(0, data.GetLength(1)));
            var selectedRadius = Random.Range(1f, Mathf.Max(data.GetLength(0), data.GetLength(1)) / 2);

            for (int x = 0; x < ingameBars.GetLength(0); x++)
            {
                for (int y = 0; y < ingameBars.GetLength(1); y++)
                {
                    ingameBars[x, y].GetComponent<DataPointOld>().IsHighlighted = ((new Vector2(x, y) - startPoint).magnitude < selectedRadius);
                }
            }
        }

        public void ClearBars()
        {
            if (ingameBars != null)
            {
                foreach (var bar in ingameBars)
                {
                    Destroy(bar);
                }
            }

            ingameBars = null;
        }

        private void GenerateGraph()
        {
            ClearBars();

            ingameBars = new GameObject[data.GetLength(0), data.GetLength(1)];

            for (int x = 0; x < ingameBars.GetLength(0); x++)
            {
                for (int y = 0; y < ingameBars.GetLength(1); y++)
                {
                    var bar = Instantiate(prefabBar);
                    var dataPoint = bar.GetComponent<DataPointOld>();

                    if (!dataPoint)
                    {
                        print("Error: Attach DataPointOld script to graph prefab object!");
                    }
                    else
                    {
                        bar.transform.parent = transform;
                        dataPoint.TargetHeight = data[x, y];
                        dataPoint.SetPosition(2 * x, 2 * y);
                    }

                    ingameBars[x, y] = bar;
                }
            }
        }

        /**
         *  Reuses existing bars, if possible.
         */
        public void RegenerateGraph()
        {
            data = new RandomDataProvider().GetData();

            var isInitialised = (ingameBars != null);
            var hasSameDimensions = (isInitialised && ingameBars.GetLength(0) == data.GetLength(0) && ingameBars.GetLength(1) == data.GetLength(1));

            if (isInitialised && hasSameDimensions)
            {
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    for (int y = 0; y < data.GetLength(1); y++)
                    {
                        var dataPoint = ingameBars[x, y].GetComponent<DataPointOld>();

                        if (!dataPoint)
                        {
                            print("Error: Attach DataPointOld script to graph prefab object!");
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
}
