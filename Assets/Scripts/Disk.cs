using UnityEngine;

public class Disk : MonoBehaviour
{
    public int size;
    public Tower currentTower;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private bool isDragging = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }
    
    void OnMouseDown()
    {
        if (!CanPickUp()) return;
        
        startPosition = transform.position;
        isDragging = true;
        rb.isKinematic = true; // Izslēdz fiziku, kamēr velk
    }
    
    void OnMouseDrag()
    {
        if (!isDragging) return;
        
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = objPosition;
    }
    
    void OnMouseUp()
    {
        if (!isDragging) return;
        
        isDragging = false;
        rb.isKinematic = false; // Ieslēdz fiziku atpakaļ
        
        Tower nearestTower = FindNearestTower();
        if (nearestTower != null && nearestTower.CanPlaceDisk(this))
        {
            nearestTower.PlaceDisk(this);
        }
        else
        {
            // Atgriež atpakaļ uz sākuma stabiņa
            if (currentTower != null)
            {
                currentTower.PlaceDisk(this);
            }
            else
            {
                transform.position = startPosition;
            }
        }
    }
    
    private bool CanPickUp()
    {
        // Var pacelt tikai augšējo disku
        if (currentTower == null) return true;
        
        Disk topDisk = currentTower.GetTopDisk();
        return topDisk == this;
    }
    
    private Tower FindNearestTower()
    {
        Tower[] towers = FindObjectsOfType<Tower>();
        Tower nearest = null;
        float minDistance = Mathf.Infinity;
        
        foreach (Tower tower in towers)
        {
            float distance = Vector3.Distance(transform.position, tower.transform.position);
            if (distance < minDistance && distance < 2f)
            {
                minDistance = distance;
                nearest = tower;
            }
        }
        return nearest;
    }
}