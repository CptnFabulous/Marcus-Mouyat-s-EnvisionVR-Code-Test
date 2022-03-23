using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Collider2DHighlight : MonoBehaviour
{
    public Collider highlighted;

    [Header("Setup")]
    public Camera viewingCamera;
    public Canvas canvas;
    public Image graphic;

    // Update is called once per frame
    void LateUpdate()
    {
        Rect r = ViewportRectFromCollider(highlighted, viewingCamera);
        r.position *= canvas.pixelRect.size;

        Debug.Log(r.position);

        //Vector2 centreToCornerOffset = canvas.pixelRect.size * 0.5f;
        //r.position -= centreToCornerOffset;
        //r.position += r.size * 0.5f;


        //graphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, r.width);
        //graphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, r.height);
        graphic.rectTransform.localPosition = r.position;
    }

    public static Rect ViewportRectFromCollider(Collider selected, Camera camera)
    {
        Quaternion rotation = Quaternion.LookRotation(selected.bounds.center - camera.transform.position, camera.transform.up);
        float maxExtent = MiscMath.Vector3Max(selected.bounds.extents);


        Vector3[] points = new Vector3[]
        {
            Vector3.left,
            Vector3.right,
            Vector3.down,
            Vector3.up,
        };
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = rotation * points[i];
            points[i] = selected.ClosestPoint(selected.bounds.center + points[i] * maxExtent);
            points[i] = camera.WorldToViewportPoint(points[i]);
        }

        Rect r = new Rect();
        r.xMin = points[0].x;
        r.xMax = points[1].x;
        r.yMin = points[2].y;
        r.yMax = points[3].y;

        return r;
    }
}
