using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;

    [Header("Towers (assign tower parent objects)")]
    public Transform[] towers;

    [Header("Game Settings")]
    public int totalDisks = 6;
    public int targetTowerIndex = 2;
    public float verticalSpacing = 30f;
    public float fallDuration = 0.3f; // Added: Duration of the falling animation

    private List<List<DiskDrag>> towerContents = new List<List<DiskDrag>>();

    public System.Action OnWin;
    public System.Action<DiskDrag, int, int> OnDiskMoved;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        FindAndSetupTowers();
        InitializeTowers();
    }

    private void FindAndSetupTowers()
    {
        if (towers == null || towers.Length == 0 || towers[0] == null)
        {
            Tower[] towerComponents = FindObjectsOfType<Tower>();
            if (towerComponents.Length > 0)
            {
                System.Array.Sort(towerComponents, (a, b) => a.towerIndex.CompareTo(b.towerIndex));
                towers = new Transform[towerComponents.Length];
                for (int i = 0; i < towerComponents.Length; i++)
                    towers[i] = towerComponents[i].transform;
            }
            else
            {
                Debug.LogError("No towers found in scene!");
            }
        }
    }

    private void InitializeTowers()
    {
        towerContents.Clear();
        for (int i = 0; i < towers.Length; i++)
            towerContents.Add(new List<DiskDrag>());
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        foreach (var tower in towerContents) tower.Clear();

        DiskDrag[] disks = FindObjectsOfType<DiskDrag>();
        System.Array.Sort(disks, (a, b) => b.diskSize.CompareTo(a.diskSize));

        foreach (var disk in disks)
        {
            RegisterDiskAtTower(disk, 0, false);
        }

        for (int i = 0; i < towers.Length; i++)
            UpdateTowerVisuals(i);

        UpdateTopDiskDraggables();
    }

    public void RegisterDiskAtTower(DiskDrag disk, int towerIndex, bool updateVisuals = true)
    {
        // Remove disk from any tower first
        foreach (var list in towerContents)
            list.Remove(disk);

        // Add disk to the **top of the tower**
        towerContents[towerIndex].Add(disk);
        disk.currentTowerIndex = towerIndex;
        disk.transform.SetParent(towers[towerIndex]);

        if (updateVisuals)
            SnapDiskToTower(disk, towerIndex);

        UpdateTopDiskDraggables();
    }

    public bool IsTopDisk(DiskDrag disk)
    {
        if (disk == null) return false;
        int t = disk.currentTowerIndex;
        if (t < 0 || t >= towerContents.Count) return false;
        var list = towerContents[t];
        return list.Count > 0 && list[list.Count - 1] == disk;
    }

    public bool TryMove(DiskDrag disk, Transform targetTower)
    {
        if (disk == null || targetTower == null) return false;

        int toIndex = -1;
        for (int i = 0; i < towers.Length; i++)
            if (towers[i] == targetTower) toIndex = i;

        if (toIndex < 0 || toIndex == disk.currentTowerIndex || !IsTopDisk(disk))
            return false;

        var targetList = towerContents[toIndex];
        if (targetList.Count == 0 || targetList[targetList.Count - 1].diskSize > disk.diskSize)
        {
            int fromIndex = disk.currentTowerIndex;
            RegisterDiskAtTower(disk, toIndex);
            OnDiskMoved?.Invoke(disk, fromIndex, toIndex);
            CheckWin();
            return true;
        }

        return false;
    }

    public void SnapDiskToTower(DiskDrag disk, int towerIndex)
    {
        if (disk == null || towerIndex < 0 || towerIndex >= towers.Length) return;

        int indexInTower = towerContents[towerIndex].IndexOf(disk);
        if (indexInTower < 0) return;

        Vector2 pos = disk.originalAnchoredPosition;
        pos.y += indexInTower * verticalSpacing;
        
        // Start falling animation instead of directly setting position
        StartCoroutine(FallDiskToPosition(disk, pos));
    }

    private IEnumerator FallDiskToPosition(DiskDrag disk, Vector2 targetPosition)
    {
        Vector2 startPosition = disk.GetComponent<RectTransform>().anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fallDuration;
            // Smooth fall using ease-out
            t = 1f - (1f - t) * (1f - t); // Quadratic ease-out
            disk.SetPosition(Vector2.Lerp(startPosition, targetPosition, t));
            yield return null;
        }

        // Ensure final position is exact
        disk.SetPosition(targetPosition);
    }

    public void UpdateTowerVisuals(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerContents.Count) return;
        foreach (var disk in towerContents[towerIndex])
            SnapDiskToTower(disk, towerIndex);
    }

    public void UpdateTopDiskDraggables()
    {
        for (int i = 0; i < towerContents.Count; i++)
        {
            var tower = towerContents[i];
            for (int j = 0; j < tower.Count; j++)
            {
                DiskDrag disk = tower[j];
                bool isTop = (j == tower.Count - 1);
                disk.draggable = isTop;
                if (disk.canvasGroup != null)
                    disk.canvasGroup.blocksRaycasts = isTop;
            }
        }
    }

    public int FindTowerUnderPointer(Vector2 screenPosition)
    {
        for (int i = 0; i < towers.Length; i++)
        {
            RectTransform towerRect = towers[i].GetComponent<RectTransform>();
            if (towerRect == null) continue;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                towerRect,
                screenPosition,
                towers[i].GetComponentInParent<Canvas>().worldCamera,
                out localPoint
            );

            if (towerRect.rect.Contains(localPoint))
                return i;
        }
        return -1;
    }

    private void CheckWin()
    {
        if (targetTowerIndex < 0 || targetTowerIndex >= towerContents.Count) return;

        var list = towerContents[targetTowerIndex];
        if (list.Count != totalDisks) return;

        for (int i = 0; i < list.Count - 1; i++)
            if (list[i].diskSize <= list[i + 1].diskSize)
                return;

        Debug.Log("YOU WIN!");
        OnWin?.Invoke();

        foreach (var tower in towerContents)
            foreach (var disk in tower)
                disk.draggable = false;
    }
}