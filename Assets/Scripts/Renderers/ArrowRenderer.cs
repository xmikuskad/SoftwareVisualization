using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowRenderer : MonoBehaviour
{

    private List<LineRenderer> renderers;
    private float arrowDistance = 5f;

    private Camera mainCamera;
    private TextMeshPro lineText;

    private Direction direction;

    private void Awake()
    {
        mainCamera = Camera.main;
        lineText = GetComponentInChildren<TextMeshPro>();
    }


    private void LateUpdate()
    {

        // if (direction == Direction.DOWN)
        // {
        // Calculate the rotation of the text based on the camera's position and direction
        Vector3 directionToCamera = mainCamera.transform.position - lineText.transform.position;
        Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera);

        // Apply the rotation to the text
        lineText.transform.rotation = rotationToCamera * Quaternion.Euler(0, 180, 0);
        // }
    }

    // private void LateUpdate() {
    //     lineText.transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
    //         mainCamera.transform.rotation * Vector3.up);
    // }

    public void SetUp(Vector3 from, Vector3 to, float width, string text, Direction direction)
    {
        renderers = GetComponentsInChildren<LineRenderer>().ToList();
        this.direction = direction;

        LineRenderer main = renderers[0];
        main.positionCount = 2;
        main.SetPosition(1, to);
        main.startWidth = width;
        main.endWidth = width;
        main.SetPosition(0, from);

        if (direction == Direction.RIGHT)
        {
            LineRenderer lr = GetDefaultRenderer(1, width, to);
            Vector3 tmp = to;
            tmp.x -= arrowDistance;
            tmp.y += arrowDistance;
            lr.SetPosition(1, tmp);

            lr = GetDefaultRenderer(2, width, to);
            tmp = to;
            tmp.x -= arrowDistance;
            tmp.y -= arrowDistance;
            lr.SetPosition(1, tmp);

            lr = GetDefaultRenderer(3, width, to);
            tmp = to;
            tmp.x -= arrowDistance;
            tmp.z += arrowDistance;
            lr.SetPosition(1, tmp);
            lr = GetDefaultRenderer(4, width, to);
            tmp = to;
            tmp.x -= arrowDistance;
            tmp.z -= arrowDistance;
            lr.SetPosition(1, tmp);

            lineText.text = text;
            lineText.transform.position = (from - to) / 2 + to + Vector3.down * 5f;
        }
        else if (direction == Direction.LEFT)
        {
            LineRenderer lr = GetDefaultRenderer(1, width, to);
            Vector3 tmp = to;
            tmp.z -= arrowDistance;
            tmp.y += arrowDistance;
            lr.SetPosition(1, tmp);

            lr = GetDefaultRenderer(2, width, to);
            tmp = to;
            tmp.z -= arrowDistance;
            tmp.y -= arrowDistance;
            lr.SetPosition(1, tmp);

            lr = GetDefaultRenderer(3, width, to);
            tmp = to;
            tmp.z -= arrowDistance;
            tmp.x += arrowDistance;
            lr.SetPosition(1, tmp);
            lr = GetDefaultRenderer(4, width, to);
            tmp = to;
            tmp.z -= arrowDistance;
            tmp.x -= arrowDistance;
            lr.SetPosition(1, tmp);

            lineText.text = text;
            lineText.transform.position = (from - to) / 2 + to + Vector3.down * 5f;
        }

    }

    public void SetUpNoArrows(Vector3 from, Vector3 to, float width, string text, Direction direction)
    {
        renderers = GetComponentsInChildren<LineRenderer>().ToList();
        this.direction = direction;

        LineRenderer main = renderers[0];
        main.positionCount = 2;
        main.SetPosition(1, to);
        main.startWidth = width;
        main.endWidth = width;
        main.SetPosition(0, from);
        lineText.text = text;
        lineText.transform.position = from + Vector3.right * 2f + Vector3.up * 3f;
    }

    private LineRenderer GetDefaultRenderer(int index, float width, Vector3 to)
    {
        LineRenderer main = renderers[index];
        main.positionCount = 2;
        main.SetPosition(0, to);
        main.startWidth = width / 3;
        main.endWidth = width / 3;
        return main;
    }
}

public enum Direction
{
    DOWN, LEFT, RIGHT
}
