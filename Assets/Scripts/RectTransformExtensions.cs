using UnityEngine;

public static class RectTransformExtensions
{
    public static bool RectangleOverlap(this RectTransform rect1, RectTransform rect2)
    {
        Rect rect1World = GetWorldRect(rect1);
        Rect rect2World = GetWorldRect(rect2);
        
        return rect1World.Overlaps(rect2World);
    }

    private static Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        Vector2 min = corners[0];
        Vector2 max = corners[2];
        
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
}