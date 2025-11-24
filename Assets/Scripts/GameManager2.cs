using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public GameObject diskPrefab;
    public Tower[] towers;
    public Sprite[] diskSprites;
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject newDisk = Instantiate(diskPrefab);
            Disk disk = newDisk.GetComponent<Disk>();
            disk.size = i;
            
            SpriteRenderer renderer = newDisk.GetComponent<SpriteRenderer>();
            if (i < diskSprites.Length)
            {
                renderer.sprite = diskSprites[i];
            }
            
            // Iestata svaru/masu atbilstoši izmēram
            Rigidbody2D rb = newDisk.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.mass = 1f + (i * 0.2f); // Lielākiem diskiem lielāka masa
            }
            
            newDisk.name = "Disk_" + i;
            towers[0].PlaceDisk(disk);
        }
    }
}