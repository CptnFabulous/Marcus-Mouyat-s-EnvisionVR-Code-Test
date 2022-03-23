using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInput))]
public class GameObjectSelection : MonoBehaviour
{
    public Camera camera;
    public LayerMask selectionMask;
    public UnityEvent<RaycastHit> onMouseOver;
    public UnityEvent<RaycastHit> onTrySelect;
    Vector2 viewportPosition;

    #region Input actions
    public void OnPositionCursor(InputValue input)
    {
        viewportPosition = camera.ScreenToViewportPoint(input.Get<Vector2>());
        CursorCast(viewportPosition, camera, out RaycastHit rh, selectionMask);
        onMouseOver.Invoke(rh);
    }
    public void OnSelect()
    {
        CursorCast(viewportPosition, camera, out RaycastHit rh, selectionMask);
        onTrySelect.Invoke(rh);
    }
    #endregion

    public static bool CursorCast(Vector2 viewportPosition, Camera camera, out RaycastHit hit, LayerMask validObjects)
    {
        Ray cursorRay = camera.ViewportPointToRay(viewportPosition);
        return Physics.Raycast(cursorRay, out hit, camera.farClipPlane, validObjects);
    }
}
