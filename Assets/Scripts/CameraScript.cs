using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float maxZoom = 300f,
                    minZoom = 150f,
                    panSpeed = 6f;
    Vector3 bottomLeft, topRight;
    float cameraMaxX, cameraMinX, cameraMaxY, cameraMinY, x, y;
    public Camera cam;
    
    // Pievienojam mainīgos, lai saglabātu sākotnējās vērtības
    private Vector3 initialPosition;
    private float initialOrthographicSize;
    
    // Kontroles flags
    private bool zoomEnabled = true;
    private bool panEnabled = true;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Saglabājam sākotnējās vērtības
        initialPosition = transform.position;
        initialOrthographicSize = cam.orthographicSize;
        
        topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));
        cameraMaxX = topRight.x;
        cameraMaxY = topRight.y;
        cameraMinX = bottomLeft.x;
        cameraMinY = bottomLeft.y;
    }

    void Update(){
    if(Time.timeScale != 0f){
        // Tikai pārvietošanās, ja panEnabled ir true
        if (panEnabled)
        {
            x = Input.GetAxis("Mouse X") * panSpeed;
            y = Input.GetAxis("Mouse Y") * panSpeed;
            transform.Translate(x, y, 0);
        }

        // Tikai zoom, ja zoomEnabled ir true
        if (zoomEnabled)
        {
            if ((Input.GetAxis("Mouse ScrollWheel") > 0) && cam.orthographicSize > minZoom)
            {
                cam.orthographicSize = cam.orthographicSize - 50f;
            }

            if ((Input.GetAxis("Mouse ScrollWheel") < 0) && cam.orthographicSize < maxZoom)
            {
                cam.orthographicSize = cam.orthographicSize + 50f;
            }
        }

        // Boundary checks paliek vienmēr aktīvi
        topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        if (topRight.x > cameraMaxX)
        {
            transform.position = new Vector3(
                transform.position.x - (topRight.x - cameraMaxX), transform.position.y, transform.position.z);
        }

        if (topRight.y > cameraMaxY)
        {
            transform.position = new Vector3
            (transform.position.x, transform.position.y - (topRight.y - cameraMaxY), transform.position.z);
        }

        if (bottomLeft.x < cameraMinX)
        {
            transform.position = new Vector3(
                transform.position.x + (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);
        }

        if (bottomLeft.y < cameraMinY)
        {
            transform.position = new Vector3(
                transform.position.x, transform.position.y + (cameraMinY - bottomLeft.y), transform.position.z);
        }
    }}
        
    // Kameras resetēšanas metode
    public void ResetCamera()
    {
        // Atjauno sākotnējo pozīciju
        transform.position = initialPosition;
        
        // Atjauno sākotnējo tāluma izmēru
        cam.orthographicSize = initialOrthographicSize;
        
        Debug.Log("Kamera resetēta uz sākotnējo stāvokli");
    }
    
    // Metodes kontroles ieslēgšanai/izslēgšanai
    public void SetZoomEnabled(bool enabled)
    {
        zoomEnabled = enabled;
        Debug.Log("Zoom " + (enabled ? "ieslēgts" : "izslēgts"));
    }
    
    public void SetPanEnabled(bool enabled)
    {
        panEnabled = enabled;
        Debug.Log("Pan " + (enabled ? "ieslēgts" : "izslēgts"));
    }
    
    public void SetAllControlsEnabled(bool enabled)
    {
        zoomEnabled = enabled;
        panEnabled = enabled;
        Debug.Log("Visas kameras kontroles " + (enabled ? "ieslēgtas" : "izslēgtas"));
    }
    
    // Īsās metodes ātrai izmantošanai
    public void DisableAllControls()
    {
        SetAllControlsEnabled(false);
    }
    
    public void EnableAllControls()
    {
        SetAllControlsEnabled(true);
    }
}