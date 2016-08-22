using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectedPointGraph : MonoBehaviour
{
    public int NumberOfPoints = 1000;
    public Vector3 DataRange = new Vector3(100f, 100f, 100f);
    [Range(0, 1)]
    public float PointScale = 1f;

    private GameObject[] datapoints;

	void Start ()
    {
        datapoints = new GameObject[NumberOfPoints];
        for (int i = 0; i < NumberOfPoints; i++)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            var randomPosition = Random.insideUnitSphere;
            randomPosition.Scale(DataRange * NextGaussianDouble() / 2);
            sphere.transform.localPosition = randomPosition;
            sphere.transform.localScale = new Vector3(PointScale, PointScale, PointScale);
            sphere.isStatic = true;

            sphere.transform.parent = transform;
            datapoints[i] = sphere;
        }

    }

    // https://stackoverflow.com/questions/5817490/implementing-box-mueller-random-number-generator-in-c-sharp
    private float NextGaussianDouble()
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }

    void Update()
    {
    }
}
