using System.Collections.Generic;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;

    [Header("Towers (assign transforms in scene)")]
    public Transform[] towers;

    [Header("Game Settings")]
    public int totalDisks = 6;
    public int targetTowerIndex = 2;
    public float verticalSpacing = 0.5f; // world units between stacked disks

    // Internal state: each tower stores bottom->top list of disks
    private List<List<DiskDrag>> towerContents = new List<List<DiskDrag>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        towerContents.Clear();
        for (int i = 0; i < towers.Length; i++)
            towerContents.Add(new List<DiskDrag>());
    }

    private void Start()
    {
        // Automatically register all disks in scene
        DiskDrag[] disks = FindObjectsOfType<DiskDrag>();
        foreach (var disk in disks)
        {
            Transform closestTower = FindClosestTower(disk.transform.position);
            int towerIndex = System.Array.IndexOf(towers, closestTower);
            if (towerIndex >= 0)
                RegisterDiskAtTower(disk, towerIndex);
            else
                Debug.LogError("Disk not near any valid tower: " + disk.name);
        }
    }

    public void RegisterDiskAtTower(DiskDrag disk, int towerIndex)
    {
        if (disk == null || towerIndex < 0 || towerIndex >= towers.Length || towers[towerIndex] == null)
        {
            Debug.LogError("RegisterDiskAtTower: invalid towerIndex " + towerIndex);
            return;
        }

        // Remove from all towers first
        foreach (var list in towerContents)
            list.Remove(disk);

        towerContents[towerIndex].Add(disk);
        disk.currentTowerIndex = towerIndex;

        SnapDiskToTower(disk, towerIndex);
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
        int toIndex = System.Array.IndexOf(towers, targetTower);
        if (toIndex < 0 || targetTower == null || disk == null) return false;

        int fromIndex = disk.currentTowerIndex;
        if (!IsTopDisk(disk)) return false;

        var targetList = towerContents[toIndex];

        if (targetList.Count == 0 || targetList[targetList.Count - 1].diskSize > disk.diskSize)
        {
            // Valid move
            RegisterDiskAtTower(disk, toIndex);
            CheckWin();
            return true;
        }

        return false;
    }

    public void SnapDiskToTower(DiskDrag disk, int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towers.Length || towers[towerIndex] == null)
        {
            Debug.LogError("SnapDiskToTower: invalid towerIndex " + towerIndex);
            return;
        }

        Transform tower = towers[towerIndex];
        Vector3 pos = tower.position + Vector3.up * verticalSpacing * (towerContents[towerIndex].Count - 1);
        disk.SetTargetPosition(pos);
    }

    public Transform FindClosestTower(Vector3 pos)
    {
        Transform best = null;
        float bestDist = float.MaxValue;
        foreach (var t in towers)
        {
            if (t == null) continue;
            float dist = Vector3.Distance(pos, t.position);
            if (dist < bestDist)
            {
                best = t;
                bestDist = dist;
            }
        }
        return best;
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
    }

    public int GetTowerHeight(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerContents.Count) return 0;
        return towerContents[towerIndex].Count;
    }
}
