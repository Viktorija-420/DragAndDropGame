using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingObjectsControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    public float fadeDuration = 1.5f;
    private ObjectScript objectScript;
    private ScreenBoundaries screenBoundriesScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isFadingOut = false;
    private bool isExploding = false;
    private Image image;
    private Color orginalColor;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();

        image = GetComponent<Image>();
        orginalColor = image.color;

        objectScript = Object.FindFirstObjectByType<ObjectScript>();
        screenBoundriesScript = Object.FindFirstObjectByType<ScreenBoundaries>();
        StartCoroutine(FadeIn());
        canvasGroup.alpha = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);

        // Iznīcinās ja lido pa kreisi
        if (speed > 0 && transform.position.x < (screenBoundriesScript.worldBounds.xMin + 80) && !isFadingOut)
        {
            isFadingOut = true;
            StartCoroutine(FadeOutAndDestroy());
        }

        // Iznīcinās ja lido pa labi
        if (speed < 0 && transform.position.x > (screenBoundriesScript.worldBounds.xMax - 80) && !isFadingOut)
        {
            isFadingOut = true;
            StartCoroutine(FadeOutAndDestroy());
        }

        // Ja neko nevelk un kursors pieskaras bumbai
        Vector2 inputPosition;
        if (!TryGetInputPosition(out inputPosition))
            return;

        // FIXED: Add validation for screen coordinates before using RectangleContainsScreenPoint
        if (!IsValidScreenPosition(inputPosition))
            return;

        // FIXED: Add null checks for Camera.main
        if (Camera.main == null)
            return;

        if(CompareTag("Bomb") && !isExploding && 
            RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform, inputPosition, Camera.main))
        {
            Debug.Log("Bomb hit by cursor (without dragging)");
            TriggerExplosion();
        } 

        if(ObjectScript.drag && !isFadingOut && 
            RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform, inputPosition, Camera.main))
        {
            Debug.Log("Obstacle hit by drag");
           if(ObjectScript.lastDragged != null)
            {
                StartCoroutine(ShrinkAndDestroy(ObjectScript.lastDragged, 0.5f));
                ObjectScript.lastDragged = null;
                ObjectScript.drag = false;
            }

           if(CompareTag("Bomb"))
                StartToDestroy(Color.red);
            else
                StartToDestroy(Color.cyan);
        }
    }

    // FIXED: Added validation method for screen positions
    private bool IsValidScreenPosition(Vector2 position)
    {
        // Check for infinity or NaN values
        if (float.IsInfinity(position.x) || float.IsInfinity(position.y) ||
            float.IsNaN(position.x) || float.IsNaN(position.y))
        {
            return false;
        }

        // Check if position is within reasonable screen bounds
        // Adding some padding to account for edge cases
        float padding = 100f;
        if (position.x < -padding || position.x > Screen.width + padding ||
            position.y < -padding || position.y > Screen.height + padding)
        {
            return false;
        }

        return true;
    }

    public void TriggerExplosion()
    {
        isExploding = true;
        objectScript.effects.PlayOneShot(objectScript.audioCli[14]);

        if(TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetBool("explode", true);
        }

        image.color = Color.red;
        StartCoroutine(RecoverColor(0.4f));
        StartCoroutine(Vibrate());
        StartCoroutine(WaitBeforeExplode());
    }

    IEnumerator WaitBeforeExplode()
    {
        float radius = 0;
        if(TryGetComponent<CircleCollider2D>(out CircleCollider2D circleCollider))
        {
            radius = circleCollider.radius * transform.lossyScale.x;
            ExploadAndDestroyNearbyObjects(radius);
            yield return new WaitForSeconds(1f);
            ExploadAndDestroyNearbyObjects(radius);
            Destroy(gameObject);
        }
    }

    void ExploadAndDestroyNearbyObjects(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            if(hit != null && hit.gameObject != gameObject)
            {
                FlyingObjectsControllerScript obj = hit.GetComponent<FlyingObjectsControllerScript>();
                if(obj != null && !obj.isExploding)
                {
                    obj.StartToDestroy(Color.cyan);
                }
            }
        }
    }

    public void StartToDestroy(Color c)
    {
        if(!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;

            image.color = c;
            StartCoroutine(RecoverColor(0.5f));

            StartCoroutine(Vibrate());
            objectScript.effects.PlayOneShot(objectScript.audioCli[15]);
        }
    }

    IEnumerator FadeIn()
    {
        float a = 0f;
        while(a < fadeDuration)
        {
            a += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndDestroy()
    {
        float a = 0f;
        float startAlpha = canvasGroup.alpha;

        while (a < fadeDuration)
        {
            a += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, a / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        Destroy(gameObject);
    }

    IEnumerator ShrinkAndDestroy(GameObject target, float duration)
    {
        Vector3 orginalScale = target.transform.localScale;
        Quaternion orginalRotation = target.transform.rotation;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(orginalScale, Vector3.zero, t / duration);
            float angle = Mathf.Lerp(0, 360, t / duration);
            target.transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
        // Ko darīt ar māšinu tālāk?
        // Nav obligāti jāiznīcina, varbūt jāatgriež sākuma pozīcijā?
        ObjectScript.carsLeft--;
        ObjectScript.carsDestroyed++;
        Destroy(target);
    }

    IEnumerator RecoverColor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        image.color = orginalColor;
    }

    IEnumerator Vibrate()
    {
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif

        Vector2 orginalPosition = rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elpased = 0f;
        float intensity = 5f;

        while (elpased < duration)
        {
            rectTransform.anchoredPosition = orginalPosition + Random.insideUnitCircle * intensity;
            elpased += Time.deltaTime;
            yield return null;
        }
    }
    
    bool TryGetInputPosition(out Vector2 position)
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
        position = Input.mousePosition;
        return true;

        #elif UNITY_ANDROID
            if(Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                // FIXED: Only use valid touch phases
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    position = touch.position;
                    return true;
                }
            }
            
            position = Vector2.zero;
            return false;
        #else
            position = Vector2.zero;
            return false;
        #endif
    }
}