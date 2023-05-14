using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = -60.0f;
    public float zoomSpeed = 500.0f;
    public float rotateSpeed = -3.0f;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private Vector3 lastMousePosition;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = mainCamera.transform;
        startingPosition = cameraTransform.position;
        startingRotation = cameraTransform.rotation;
    }

    void Update()
    {
        if (SingletonManager.Instance.pauseManager.IsInteractionPaused())
        {
            return;
        }
        
        // Move the object when the left mouse button is held down
        if (Input.GetMouseButton(2))
        {
            var position = cameraTransform.position;
            position += cameraTransform.right * (Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime);
            position += cameraTransform.up * (Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime);
            cameraTransform.position = position;
        }

        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        cameraTransform.position += cameraTransform.forward * zoom;

        if(Input.GetMouseButton(1)) {
            cameraTransform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime, -Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime, 0));
            float x = cameraTransform.rotation.eulerAngles.x;
            float y = cameraTransform.rotation.eulerAngles.y;
            cameraTransform.rotation = Quaternion.Euler(x, y, 0);
        }
        
        // Reset camera to starting position when Space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cameraTransform.position = startingPosition;
            cameraTransform.rotation = startingRotation;
        }

        lastMousePosition = Input.mousePosition;
    }
}
