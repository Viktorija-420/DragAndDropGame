using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;

    [Header("Towers (assign tower parent objects)")]
    public Transform[] towers;

    [Header("Game Settings")]
    public int totalDisks = 6;
    public int targetTowerIndex = 2;
    public float verticalSpacing = 30f; // UI spacing

    // Internal state: each tower stores bottom->top list of disks
    private List<List<DiskDrag>> towerContents = new List<List<DiskDrag>>();

    // Events for game state
    public System.Action OnWin;
    public System.Action<DiskDrag, int, int> OnDiskMoved;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
        
        // Ensure we have towers before initializing
        FindAndSetupTowers();
        InitializeTowers();
    }

    private void FindAndSetupTowers()
    {
        // If towers are not assigned in inspector, find them automatically
        if (towers == null || towers.Length == 0 || towers[0] == null)
        {
            Debug.Log("Auto-finding towers...");
            
            // Method 1: Find by Tower component
            Tower[] towerComponents = FindObjectsOfType<Tower>();
            if (towerComponents.Length > 0)
            {
                System.Array.Sort(towerComponents, (a, b) => a.towerIndex.CompareTo(b.towerIndex));
                towers = new Transform[towerComponents.Length];
                for (int i = 0; i < towerComponents.Length; i++)
                {
                    towers[i] = towerComponents[i].transform;
                    Debug.Log($"Found tower {i}: {towerComponents[i].name}");
                }
            }
            else
            {
                // Method 2: Find by name
                GameObject[] towerObjects = GameObject.FindGameObjectsWithTag("Tower");
                if (towerObjects.Length == 0)
                {
                    // Find objects with "Tower" in name
                    towerObjects = System.Array.FindAll(FindObjectsOfType<GameObject>(), 
                        go => go.name.Contains("Tower"));
                }

                if (towerObjects.Length > 0)
                {
                    System.Array.Sort(towerObjects, (a, b) => a.name.CompareTo(b.name));
                    towers = new Transform[towerObjects.Length];
                    for (int i = 0; i < towerObjects.Length; i++)
                    {
                        // Add Tower component if missing
                        Tower towerComp = towerObjects[i].GetComponent<Tower>();
                        if (towerComp == null)
                        {
                            towerComp = towerObjects[i].AddComponent<Tower>();
                        }
                        towerComp.towerIndex = i;
                        
                        towers[i] = towerObjects[i].transform;
                        Debug.Log($"Found tower {i}: {towerObjects[i].name}");
                    }
                }
                else
                {
                    // Create fallback towers
                    CreateFallbackTowers();
                }
            }
        }

        // Validate all towers
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i] == null)
            {
                Debug.LogError($"Tower {i} is null! Creating fallback.");
                CreateFallbackTowers();
                break;
            }
        }
    }

    private void CreateFallbackTowers()
    {
        Debug.Log("Creating fallback towers...");
        GameObject canvasObject = FindObjectOfType<Canvas>().gameObject;
        if (canvasObject == null)
        {
            canvasObject = new GameObject("Canvas");
            canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        towers = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject towerObj = new GameObject($"Tower_{i}");
            towerObj.transform.SetParent(canvasObject.transform);
            
            // Add RectTransform
            RectTransform rect = towerObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f + i * 0.3f, 0.1f);
            rect.anchorMax = new Vector2(0.4f + i * 0.3f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Add Tower component
            Tower towerComp = towerObj.AddComponent<Tower>();
            towerComp.towerIndex = i;
            
            // Add Image for visual (optional)
            Image image = towerObj.AddComponent<Image>();
            image.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            towers[i] = rect.transform;
            Debug.Log($"Created fallback tower {i}");
        }
    }

    private void InitializeTowers()
    {
        towerContents.Clear();
        for (int i = 0; i < towers.Length; i++)
        {
            towerContents.Add(new List<DiskDrag>());
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        // Clear all towers
        foreach (var tower in towerContents)
            tower.Clear();

        // Find and register all disks
        DiskDrag[] disks = FindObjectsOfType<DiskDrag>();
        Debug.Log($"Found {disks.Length} disks in scene");
        
        // Sort disks by size (largest first)
        System.Array.Sort(disks, (a, b) => b.diskSize.CompareTo(a.diskSize));
        
        // Put all disks on first tower initially
        foreach (var disk in disks)
        {
            if (towers.Length > 0 && towers[0] != null)
            {
                RegisterDiskAtTower(disk, 0, false);
            }
            else
            {
                Debug.LogError("No valid towers available!");
                return;
            }
        }

        // Update visual positions
        for (int i = 0; i < towers.Length; i++)
        {
            UpdateTowerVisuals(i);
        }
        
        DebugTowerContents();
    }

    public void RegisterDiskAtTower(DiskDrag disk, int towerIndex, bool updateVisuals = true)
    {
        if (disk == null || towerIndex < 0 || towerIndex >= towers.Length || towers[towerIndex] == null)
        {
            Debug.LogError($"RegisterDiskAtTower: Invalid parameters - disk: {disk}, towerIndex: {towerIndex}");
            return;
        }

        Debug.Log($"Registering disk {disk.name} (size: {disk.diskSize}) at tower {towerIndex}");

        // Remove from all towers first
        foreach (var list in towerContents)
            list.Remove(disk);

        towerContents[towerIndex].Add(disk);
        disk.currentTowerIndex = towerIndex;

        // Set parent to tower
        disk.transform.SetParent(towers[towerIndex]);

        if (updateVisuals)
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
        if (targetTower == null)
        {
            Debug.LogError("TryMove: targetTower is null");
            return false;
        }

        int toIndex = -1;
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i] == targetTower)
            {
                toIndex = i;
                break;
            }
        }

        if (toIndex < 0)
        {
            Debug.LogError("TryMove: targetTower not found in towers array");
            return false;
        }

        if (disk == null) return false;

        int fromIndex = disk.currentTowerIndex;
        if (fromIndex == toIndex) 
        {
            Debug.Log("Same tower, move ignored");
            return false; // Same tower
        }
        
        if (!IsTopDisk(disk)) 
        {
            Debug.Log("Not top disk, move ignored");
            return false;
        }

        var targetList = towerContents[toIndex];

        // Check if move is valid (empty tower or smaller disk on top)
        if (targetList.Count == 0 || targetList[targetList.Count - 1].diskSize > disk.diskSize)
        {
            // Valid move
            Debug.Log($"Valid move: disk {disk.diskSize} from tower {fromIndex} to tower {toIndex}");
            RegisterDiskAtTower(disk, toIndex);
            OnDiskMoved?.Invoke(disk, fromIndex, toIndex);
            CheckWin();
            return true;
        }
        else
        {
            Debug.Log($"Invalid move: disk {disk.diskSize} cannot go on top of disk {targetList[targetList.Count - 1].diskSize}");
        }

        return false;
    }

    public void SnapDiskToTower(DiskDrag disk, int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towers.Length || towers[towerIndex] == null)
        {
            Debug.LogError($"SnapDiskToTower: invalid towerIndex {towerIndex}");
            return;
        }

        int diskIndexInTower = towerContents[towerIndex].IndexOf(disk);
        if (diskIndexInTower >= 0)
        {
            float yPosition = verticalSpacing * diskIndexInTower;
            disk.SetPosition(new Vector2(0, yPosition));
        }
    }

    public void UpdateTowerVisuals(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerContents.Count) return;

        var towerDisks = towerContents[towerIndex];
        for (int i = 0; i < towerDisks.Count; i++)
        {
            SnapDiskToTower(towerDisks[i], towerIndex);
        }
    }

    private void CheckWin()
    {
        if (targetTowerIndex < 0 || targetTowerIndex >= towerContents.Count) return;

        var list = towerContents[targetTowerIndex];
        if (list.Count != totalDisks) return;

        // Check if disks are in correct order (smallest on top)
        for (int i = 0; i < list.Count - 1; i++)
        {
            if (list[i].diskSize <= list[i + 1].diskSize)
                return;
        }

        Debug.Log("YOU WIN!");
        OnWin?.Invoke();
        
        // Disable all disks
        foreach (var diskList in towerContents)
        {
            foreach (var disk in diskList)
            {
                disk.draggable = false;
            }
        }
    }

    // Debug method to show tower contents
    public void DebugTowerContents()
    {
        for (int i = 0; i < towerContents.Count; i++)
        {
            string diskSizes = "";
            foreach (var disk in towerContents[i])
            {
                diskSizes += disk.diskSize + " ";
            }
            Debug.Log($"Tower {i} ({towers[i].name}): {diskSizes}");
        }
    }
}