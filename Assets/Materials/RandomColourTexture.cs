using UnityEngine;
using System.Collections;

public class RandomColourTexture : MonoBehaviour {

    private static Color[] RandomColours =
    {
        new Color(244, 67, 54),
        new Color(156, 27, 176),
        new Color(63, 81, 181),
        new Color(33, 150, 243),
        new Color(76, 175, 80),
        new Color(205, 220, 57),
        new Color(255, 87, 34)
    };

    private static Color GetRandomColour()
    {
        return RandomColours[(int)(Random.value * (RandomColours.Length - 1))];

    }

	// Use this for initialization
	void Start () {
        var renderer = GetComponent<MeshRenderer>();
        var colour = GetRandomColour();
        renderer.material.color = new Color(colour.r / 255f, colour.g /255f, colour.b / 255f, Random.value);
    }
}
