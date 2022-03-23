using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class SceneViewer : MonoBehaviour
{
    /*
    I still have some annoying bugs to figure out though

    the rotation being way too fast when the axis is near vertical
    figuring out using code the ideal distance to zoom back so the default view shows the whole object
    */

    public Collider viewedObject;

    [Header("Settings")]
    public Camera camera;
    public float panSensitivity = 0.01f;
    public float rotateSensitivity = 60;
    public float zoomSensitivity = 100;
    public float maxZoomDistance = 5;
    public float minZoomDistance = 0.3f;
    public float resetLookTime = 2f;
    public float defaultZoomDistance = 1;

    [Header("Cosmetics")]
    public float cameraShiftTime = 5;
    public AnimationCurve resetAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    #region Internal variables
    List<Collider> allObjectColliders;
    bool isPanning;
    bool isRotating;
    IEnumerator currentViewAction = null;

    Vector3 axis; // The point in space that the camera rotates around
    Vector3 desiredCameraPosition;
    Quaternion desiredCameraRotation;
    float zoomValue; // The distance between the camera and the axis being focused on
    #endregion

    /// <summary>
    /// Does the behaviour currently accept player inputs? Disables if the behaviour is disabled or in the middle of an automatic action.
    /// </summary>
    public bool canControl
    {
        get
        {
            return enabled && currentViewAction == null;
        }
    }
    float minExtent => MiscMath.Vector3Min(viewedObject.bounds.extents);
    float maxExtent => MiscMath.Vector3Max(viewedObject.bounds.extents);

    private void Start()
    {
        AssignNewObject(viewedObject);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(viewedObject.bounds.center, viewedObject.bounds.size);
        Gizmos.DrawLine(axis, camera.transform.position);
        Gizmos.DrawLine(viewedObject.bounds.center, axis);
    }
    private void LateUpdate()
    {
        camera.transform.position = Vector3.Lerp(camera.transform.position, desiredCameraPosition, cameraShiftTime * Time.deltaTime);
        camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, desiredCameraRotation, cameraShiftTime * Time.deltaTime);
        Debug.DrawRay(desiredCameraPosition, desiredCameraRotation * Vector3.forward, Color.blue);
        Debug.DrawRay(desiredCameraPosition, desiredCameraRotation * Vector3.up, Color.green);
    }

    public void AssignNewObject(Collider newObject)
    {
        viewedObject = newObject;
        allObjectColliders = new List<Collider>(viewedObject.GetComponentsInChildren<Collider>());

        OnResetView();
    }

    #region Input functions
    public void OnPan(InputValue input)
    {
        isPanning = input.isPressed;
    }
    public void OnRotate(InputValue input)
    {
        isRotating = input.isPressed;
    }
    public void OnZoom(InputValue input)
    {
        if (canControl == false)
        {
            return;
        }
        
        Vector2 inputValue = input.Get<Vector2>();

        // Reset zoom value from the unclamped value to the current actual value, so the response is immediate
        zoomValue = Vector3.Distance(axis, desiredCameraPosition);
        /*
        Calculate zoom amount based on:
        Input
        Sensitivity
        Delta time
        World space
        Existing zoom distance (so zooming way out and in is quick but fine control is permitted when zoomed in)
        */
        float zoomInput = (inputValue.x + inputValue.y) * zoomSensitivity * maxExtent * Time.deltaTime * zoomValue;
        zoomValue -= zoomInput; // Add zoom value
        CameraSanityCheck(); // Clamp zoom values to ensure the camera doesn't clip through the object or background
    }
    public void OnMoveCursor(InputValue input)
    {
        if (canControl == false)
        {
            return;
        }

        Vector2 direction = -input.Get<Vector2>();

        if (isPanning) // Pan camera by shifting rotation axis
        {
            Pan(direction);
        }
        if (isRotating) // Rotate camera around axis transform
        {
            Rotate(direction);
        }

        CameraSanityCheck();
    }
    /// <summary>
    /// Shifts the camera view to the default position and rotation. Normally triggered by a PlayerInput class, but can be activated from elsewhere.
    /// </summary>
    public void OnResetView()
    {
        if (canControl == false)
        {
            return;
        }
        Debug.Log("Resetting view");
        currentViewAction = ResetLook();
        StartCoroutine(currentViewAction);
    }
    #endregion

    #region Internal functions
    /// <summary>
    /// Shifts the camera axis perpendicular to the camera's forward direction.
    /// </summary>
    /// <param name="input"></param>
    void Pan(Vector2 input)
    {
        // Convert direction values to world space relative to the camera
        Vector3 axisMoveDirection = camera.transform.TransformDirection(input);
        // Scale by time, sensitivity and the size of the object being viewed
        axisMoveDirection *= (Time.deltaTime * panSensitivity * minExtent);
        // Shift axis along direction
        axis += axisMoveDirection;
        // Clamp within boundaries of collider
        axis = MiscMath.Vector3Clamp(axis, viewedObject.bounds.min, viewedObject.bounds.max);
    }
    /// <summary>
    /// Twists the camera around to look in different directions.
    /// </summary>
    /// <param name="input"></param>
    void Rotate(Vector2 input)
    {
        // Obtain rotation angles modified with sensitivity and delta time
        Vector3 eulerAngles = rotateSensitivity * Time.deltaTime * new Vector2(input.y, input.x);
        // Multiplies angle by dot product between camera and building up axes, to prevent awkward jittering when direction is vertical
        eulerAngles *= Vector3.Dot(desiredCameraRotation * Vector3.up, viewedObject.transform.up);
        // Rotates camera by angles
        desiredCameraRotation *= Quaternion.Euler(eulerAngles);
        // Tweaks so world up is consistent
        desiredCameraRotation = Quaternion.LookRotation(desiredCameraRotation * Vector3.forward, viewedObject.transform.up);
    }
    IEnumerator ResetLook()
    {
        Vector3 oldAxisPosition = axis;
        Quaternion oldLookRotation = camera.transform.rotation;
        float oldZoomDistance = zoomValue;

        Quaternion defaultRotation = viewedObject.transform.rotation * Quaternion.Euler(0, 180, 0); // Rotates to face the object's front


        /*
        I tried to set up this code to automatically determine how far to zoom out to show the full building,
        but it caused strange bugs with different aspect ratios and proportions.

        // To calculate the correct distance to zoom the camera out, we need to use trigonometry.
        // Produce the smallest possible field of view constraint based on the aspect ratio
        float minAngle = Mathf.Min(camera.fieldOfView, Camera.VerticalToHorizontalFieldOfView(camera.fieldOfView, camera.aspect));
        float otherAngle = 90 - minAngle;
        float tangent = Mathf.Tan(otherAngle * Mathf.Deg2Rad);
        float newZoomDistance = maxExtent / tangent;
        */
        float newZoomDistance = defaultZoomDistance; // Stop-gap measure to allow some control over zoom

        float timer = 0;
        while (timer < 1)
        {
            timer = Mathf.Clamp01(timer + Time.deltaTime / resetLookTime);
            float t = resetAnimationCurve.Evaluate(timer);

            desiredCameraRotation = Quaternion.Lerp(oldLookRotation, defaultRotation, t);
            axis = Vector3.Lerp(oldAxisPosition, viewedObject.bounds.center, t);
            zoomValue = Mathf.Lerp(oldZoomDistance, newZoomDistance, t);

            CameraSanityCheck();

            yield return null;
        }

        currentViewAction = null;
    }

    /// <summary>
    /// <para>Adjusts the camera's position to properly look at the aim axis, and remain within acceptable zoom distances without clipping.</para>
    /// <para>This code might have been easier to make if I simply had different layers for the viewed object and other scene elements. But that might cause problems later down the line, with certain objects needing to on different layers for other reasons, e.g. selecting certain objects.</para>
    /// </summary>
    void CameraSanityCheck()
    {
        float maxZoomDistanceFromCentre = maxZoomDistance * maxExtent; // Max possible allowed distance from the central axis
        Vector3 cameraForward = desiredCameraRotation * Vector3.forward; // Hypothetical forward direction of camera. We can't use the actual camera's forward axis because its position shifts in LateUpdate.
        float minDistance = 0; // Minimum and maximum allowed distance for the camera
        float maxDistance = maxZoomDistanceFromCentre;

        #region Launches raycasts forwards and backwards to check for visible objects that the camera might clip through, and updating minDistance and maxDistance to account for them.
        
        // forwardChecks only looks for colliders in viewed object, to tell how far the camera needs to back up
        List<RaycastHit> forwardChecks = new List<RaycastHit>(Physics.RaycastAll(axis + -cameraForward * maxZoomDistanceFromCentre, cameraForward, maxZoomDistanceFromCentre, camera.cullingMask));
        forwardChecks.RemoveAll((c) => allObjectColliders.Contains(c.collider) == false);
        if (forwardChecks.Count > 0) // Sort checks by distance and obtain closest values
        {
            forwardChecks.Sort((a, b) => a.distance.CompareTo(b.distance));
            minDistance = maxZoomDistanceFromCentre - forwardChecks[0].distance; // Subtract the value from maxZoomDistanceFromCentre, to reflect distance from the axis rather than the raycast origin
        }

        // backwardChecks looks for everything but colliders in viewed object, to tell how far the camera needs to move forward
        List<RaycastHit> backwardChecks = new List<RaycastHit>(Physics.RaycastAll(axis, -cameraForward, maxZoomDistanceFromCentre, camera.cullingMask));
        backwardChecks.RemoveAll((c) => allObjectColliders.Contains(c.collider));
        if (backwardChecks.Count > 0) // Sort checks by distance and obtain closest values
        {
            backwardChecks.Sort((a, b) => a.distance.CompareTo(b.distance));
            maxDistance = backwardChecks[0].distance;
        }

        #endregion

        // Clamps current distance between closest points in front and behind
        float finalDistance = Mathf.Clamp(zoomValue, minDistance + minZoomDistance, maxDistance);
        desiredCameraPosition = axis + -cameraForward * finalDistance;
    }
    #endregion
}