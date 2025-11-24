using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public List<Disk> disks = new List<Disk>();
    public Transform[] diskPositions;
    public string towerName;
    
    public bool CanPlaceDisk(Disk newDisk)
    {
        if (disks.Count == 0) return true;
        
        Disk topDisk = GetTopDisk();
        return newDisk.size < topDisk.size;
    }
    
    public Disk GetTopDisk()
    {
        if (disks.Count == 0) return null;
        return disks[disks.Count - 1];
    }
    
    public void PlaceDisk(Disk disk)
    {
        // Noņem no vecā stabiņa
        if (disk.currentTower != null)
        {
            disk.currentTower.disks.Remove(disk);
        }
        
        // Pievieno šim stabiņam
        disks.Add(disk);
        disk.currentTower = this;
        
        // Iestata pozīciju un izslēdz fiziku uz brīdi
        Rigidbody2D rb = disk.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        
        // Novieto disku uz stabiņa
        int positionIndex = disks.Count - 1;
        if (positionIndex < diskPositions.Length && diskPositions[positionIndex] != null)
        {
            disk.transform.position = diskPositions[positionIndex].position;
        }
        
        // Pēc īsa laika atkal ieslēdz fiziku
        if (rb != null)
        {
            StartCoroutine(EnablePhysicsAfterDelay(rb, 0.1f));
        }
        
        CheckWinCondition();
    }
    
    private System.Collections.IEnumerator EnablePhysicsAfterDelay(Rigidbody2D rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.isKinematic = false;
    }
    
    private void CheckWinCondition()
    {
        Tower[] allTowers = FindObjectsOfType<Tower>();
        if (allTowers.Length >= 3)
        {
            Tower lastTower = allTowers[2];
            
            if (lastTower.disks.Count == 5)
            {
                Debug.Log("Spēle uzvarēta!");
                // Šeit var pievienot uzvaras ekrānu vai efektus
            }
        }
    }
}