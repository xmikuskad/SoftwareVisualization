using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = -60.0f;
    public float zoomSpeed = 500.0f;
    public float rotateSpeed = -3.0f;
    public Vector3 startingPosition = Vector3.zero;

    private Vector3 lastMousePosition;
    private Transform cameraTransform;

    void Start()
    {
        startingPosition = transform.position;
        cameraTransform = mainCamera.transform;
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

        // Rotate when the right mouse button is held down
        // if (Input.GetMouseButton(1))
        // {
        //     Vector3 currentMousePosition = Input.mousePosition;
        //     Vector3 mouseDelta = currentMousePosition - lastMousePosition;
        //
        //     cameraTransform.Rotate(Vector3.up, mouseDelta.x * rotateSpeed * Time.deltaTime, Space.World);
        //     cameraTransform.Rotate(Vector3.right, -mouseDelta.y * rotateSpeed * Time.deltaTime, Space.Self);
        // }
        
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
            cameraTransform.rotation = Quaternion.identity;
        }

        lastMousePosition = Input.mousePosition;
    }
}
