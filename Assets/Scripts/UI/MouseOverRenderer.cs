// MIT License

// Copyright (c) 2021 NedMakesGames

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public abstract class MouseOverRenderer : MonoBehaviour
{
    [Header("Properties")] [SerializeField]
    private float offsetY = 1.5f;

    private Canvas canvas;

    protected virtual void Start()
    {
        OnMouseExit();
    }

    private void OnMouseEnter()
    {
        if (SingletonManager.Instance.pauseManager.IsInteractionPaused())
        {
            return;
        }

        if (GetCanvas() == null) return;
        SetHoverPosition();
        OnHoverEnter();
    }

    private void OnMouseExit()
    {
        OnHoverExit();
    }

    private void OnMouseUp()
    {
        if (SingletonManager.Instance.pauseManager.IsInteractionPaused())
        {
            return;
        }

        // TODO show window in click?
        // Debug.Log("CLICKED");
        OnClick();
    }

    public abstract void OnHoverEnter();
    public abstract void OnHoverExit();

    public abstract void OnClick();
    public abstract Canvas GetCanvas();
    public abstract GameObject GetHoverObject();


    private void SetHoverPosition()
    {
        if (canvas == null)
        {
            canvas = GetCanvas();
        }

        // Offset position above object bbox (in world space)
        float offsetPosY = this.transform.position.y + offsetY;

        // Final position of marker above GO in world space
        Vector3 offsetPos = new Vector3(transform.position.x, offsetPosY, transform.position.z);

        // Calculate *screen* position (note, not a canvas/recttransform position)
        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

        // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, null,
            out canvasPos);

        // Set
        GetHoverObject().transform.localPosition = canvasPos;
    }
}