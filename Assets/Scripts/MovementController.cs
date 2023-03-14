using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = -60.0f;
    public float zoomSpeed = 500.0f;
    public float rotateSpeed = -3.0f;
    public Vector3 startingPosition = Vector3.zero;

    private Vector3 lastMousePosition;

    void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        // Move the object when the left mouse button is held down
        if (Input.GetMouseButton(0))
        {
            mainCamera.transform.position += mainCamera.transform.right * Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            mainCamera.transform.position += mainCamera.transform.up * Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
        }

        // Zoom when the mouse scroll wheel is moved
        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        mainCamera.transform.position += mainCamera.transform.forward * zoom;

        // Rotate when the right mouse button is held down
        if (Input.GetMouseButton(1))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;

            mainCamera.transform.Rotate(Vector3.up, mouseDelta.x * rotateSpeed * Time.deltaTime, Space.World);
            mainCamera.transform.Rotate(Vector3.right, -mouseDelta.y * rotateSpeed * Time.deltaTime, Space.Self);
        }

        // Reset camera to starting position when Space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mainCamera.transform.position = startingPosition;
            mainCamera.transform.rotation = Quaternion.identity;
        }

        lastMousePosition = Input.mousePosition;
    }
}
