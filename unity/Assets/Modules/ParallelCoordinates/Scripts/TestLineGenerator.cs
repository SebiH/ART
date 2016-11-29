using UnityEngine;
using System.Collections;

public class TestLineGenerator : MonoBehaviour
{
    public Material Mat;
    public int Amount = 100;
    public bool Reload = false;

	void Start ()
    {
        GenerateLines();
    }

    void Update()
    {
        if (Reload)
        {
            //Reload = false;
            GenerateLines();
        }
    }


    void GenerateLines()
    {
        for (int i = transform.childCount; i > 0; i--)
        {
            var child = transform.GetChild(i-1);
            Destroy(child.gameObject);
        }

        for (int i = 0; i < Amount; i++)
        {
            var go = new GameObject();
            go.transform.parent = transform;
            go.hideFlags = HideFlags.HideAndDontSave;

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.SetWidth(0.01f, 0.01f);
            lr.material = Mat;
            lr.SetVertexCount(2);
            lr.SetPosition(0, transform.TransformPoint(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0)));
            lr.SetPosition(1, transform.TransformPoint(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 1)));
        }
    }
	
}
