// PlacementRandomizer.cs
// Uzstādījumi: pievieno šo komponentu tukšam GameObject scēnā (piem., "GameController").
// Aizpildi sarakstus ar pārnēsājamajiem objektiem (UI elementi ar RectTransform) un ar to "pareizajām vietām" (tukši RectTransform objekti vai vizuāli mērķi).
// Movable objektiem jābūt ar komponentēm: ObjectScript, DragAndDropScript un SnapToTarget.

using System.Collections.Generic;
using UnityEngine;

public class PlacementRandomizer : MonoBehaviour
{
    [Header("UI Rects")]
    public RectTransform spawnArea;       // kur parādās pārnēsājamie objekti
    public RectTransform targetsArea;     // kur izvietotas "pareizās vietas"

    [Header("Objects")]
    public List<RectTransform> movableObjects = new List<RectTransform>();
    public List<RectTransform> targetPlaces = new List<RectTransform>();

    [Header("Options")]
    public float minDistanceBetweenObjects = 50f; // lai nesakristu viens uz otra startā
    public int maxAttempts = 50; // cik mēģinājumu atrast brīvu pozīciju
    public int randomSeed = 0; // 0 = katru reizi atšķirīgs

    void Start()
    {
        if (randomSeed != 0) Random.InitState(randomSeed);
        ShuffleTargets();
        PlaceTargetsRandomly();
        PlaceMovablesRandomly();
        AssignPairs();
    }

    void ShuffleTargets()
    {
        // vienkāršs Fisher-Yates sajaukums
        for (int i = 0; i < targetPlaces.Count; i++)
        {
            int j = Random.Range(i, targetPlaces.Count);
            var tmp = targetPlaces[i];
            targetPlaces[i] = targetPlaces[j];
            targetPlaces[j] = tmp;
        }
    }

    void PlaceTargetsRandomly()
    {
        if (targetsArea == null) return;

        for (int i = 0; i < targetPlaces.Count; i++)
        {
            var t = targetPlaces[i];
            t.SetParent(targetsArea, false);
            t.anchoredPosition = GetRandomPointInRect(targetsArea);
        }
    }

    void PlaceMovablesRandomly()
    {
        if (spawnArea == null) return;

        for (int i = 0; i < movableObjects.Count; i++)
        {
            var m = movableObjects[i];
            m.SetParent(spawnArea, false);
            Vector2 candidate;
            int attempts = 0;
            do
            {
                candidate = GetRandomPointInRect(spawnArea);
                attempts++;
            } while (!IsFarFromOthers(candidate, m, i) && attempts < maxAttempts);

            m.anchoredPosition = candidate;
        }
    }

    Vector2 GetRandomPointInRect(RectTransform area)
    {
        var rect = area.rect;
        float x = Random.Range(rect.xMin, rect.xMax);
        float y = Random.Range(rect.yMin, rect.yMax);
        return new Vector2(x, y);
    }

    bool IsFarFromOthers(Vector2 candidate, RectTransform current, int currentIndex)
    {
        // pārliecināmies, ka nepārklājas ar jau izvietotajiem movableObjects
        for (int i = 0; i < currentIndex; i++)
        {
            var other = movableObjects[i];
            if (Vector2.Distance(candidate, other.anchoredPosition) < minDistanceBetweenObjects)
                return false;
        }
        return true;
    }

    void AssignPairs()
    {
        // Ja movableObjects un targetPlaces ir dažādi izmēri, piesaista tikai līdz mazākajam skaitam
        int pairs = Mathf.Min(movableObjects.Count, targetPlaces.Count);
        for (int i = 0; i < pairs; i++)
        {
            var objRect = movableObjects[i];
            var targetRect = targetPlaces[i];

            // mēģinām atrast ObjectScript pie objekta
            var obj = objRect.GetComponent<ObjectScript>();
            if (obj != null)
            {
                obj.correctTarget = targetRect; // nododam pareizo transform
            }

            // ja ir SnapToTarget komponents, nododam arī to
            var snap = objRect.GetComponent<SnapToTarget>();
            if (snap != null)
            {
                snap.targetRect = targetRect;
            }
        }
    }
}