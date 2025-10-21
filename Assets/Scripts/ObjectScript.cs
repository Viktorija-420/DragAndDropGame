using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    public GameObject[] vehicles;
    [HideInInspector]
    public static Vector2[] startCoordinates;
    public Canvas can;
    public AudioSource effects;
    public AudioClip[] audioCli;
    [HideInInspector]
    public bool rightPlace = false;
    public static GameObject lastDragged = null;
    public static bool drag = false;
    public static int carsLeft = 0;
    public static int carsCorrectlyPlaced = 0;
    public static int carsDestroyed = 0;
    public GameObject[] placesPrefabs;
    public GameObject[] carsPrefabs;

    public GameObject[] placesPlaces;
    public GameObject[] carsPlaces;

    // Define random scale ranges
    public float minScale = 0.7f;
    public float maxScale = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        MoveObjectsToRandomPositions(carsPrefabs, carsPlaces);
        MoveObjectsToRandomPositions(placesPrefabs, placesPlaces);
        ApplyRandomScales(carsPrefabs);
        ApplyRandomScales(placesPrefabs);

        carsLeft = vehicles.Length;
        startCoordinates = new Vector2[vehicles.Length];
        for (int i = 0; i < vehicles.Length; i++)
        {
            startCoordinates[i] = vehicles[i].GetComponent<RectTransform>().localPosition;
        }
    }

    void MoveObjectsToRandomPositions(GameObject[] objectsToMove, GameObject[] targetPositions)
    {
        int count = Mathf.Min(objectsToMove.Length, targetPositions.Length);

        GameObject[] shuffledTargets = ShuffleArray(targetPositions);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = objectsToMove[i];
            GameObject target = shuffledTargets[i];

            obj.transform.position = target.transform.position;
            obj.transform.rotation = target.transform.rotation;
        }
    }

    void ApplyRandomScales(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                float randomScaleX = Random.Range(minScale, maxScale);
                float randomScaleY = Random.Range(minScale, maxScale);
                obj.transform.localScale = new Vector3(randomScaleX, randomScaleY, 1f);
            }
        }
    }

    GameObject[] ShuffleArray(GameObject[] array)
    {
        GameObject[] newArray = (GameObject[])array.Clone();
        for (int i = newArray.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (newArray[i], newArray[j]) = (newArray[j], newArray[i]);
        }
        return newArray;
    }

    // Update is called once per frame
    void Update()
    {

    }
}