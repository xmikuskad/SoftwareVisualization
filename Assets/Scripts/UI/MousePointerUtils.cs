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

using Unity.Mathematics;
using UnityEngine;

public static class MousePointerUtils {
    public static float2 GetScreenPosition() {
        Vector3 posn = Input.mousePosition;
        return new float2(posn.x, posn.y);
    }

    public static float2 GetBoundedScreenPosition() {
        float2 raw = GetScreenPosition();
        return math.clamp(raw, new float2(0, 0), new float2(Screen.width - 1, Screen.height - 1));
    }

    public static float2 GetViewportPosition() {
        float2 screenPos = GetScreenPosition();
        return screenPos / new float2(Screen.width, Screen.height);
    }

    public static float2 GetViewportPosition(Camera camera) {
        float2 screenPos = GetScreenPosition();
        float3 viewportPos = camera.ScreenToViewportPoint(new float3(screenPos, 0));
        return viewportPos.xy;
    }

    public static float3 GetWorldPosition(Camera camera) {
        return GetWorldPosition(camera, camera.nearClipPlane);
    }

    public static float3 GetWorldPosition(Camera camera, float worldDepth) {
        float2 screenPos = GetBoundedScreenPosition();
        float3 screenPosWithDepth = new float3(screenPos, worldDepth);
        return camera.ScreenToWorldPoint(screenPosWithDepth);
    }

    public static Ray GetWorldRay(Camera camera) {
        float2 screenPos = GetBoundedScreenPosition();
        float3 screenPosWithDepth = new float3(screenPos, camera.nearClipPlane);
        return camera.ScreenPointToRay(screenPosWithDepth);
    }
}