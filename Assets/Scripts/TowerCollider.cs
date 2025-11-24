// TowerCollider.cs - pievieno katrai torņa bāzei
using UnityEngine;

public class TowerCollider : MonoBehaviour
{
    public Tower tower;
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Disk disk = collision.gameObject.GetComponent<Disk>();
        if (disk != null && tower.CanPlaceDisk(disk))
        {
            tower.PlaceDisk(disk);
        }
    }
}