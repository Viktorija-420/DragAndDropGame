using UnityEngine;

public class Tower : MonoBehaviour
{
    [Tooltip("Index of this tower (0,1,2)")]
    public int towerIndex = -1;

    private void Start()
    {
        // Auto-assign index if not set
        if (towerIndex == -1)
        {
            if (int.TryParse(name.Replace("Tower", "").Trim(), out int parsedIndex))
            {
                towerIndex = parsedIndex;
            }
        }
    }
}