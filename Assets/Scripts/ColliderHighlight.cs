using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderHighlight : MonoBehaviour
{
    public Collider highlighted;

    public Mesh boxPrimitive;
    public Mesh spherePrimitive;
    public Mesh capsulePrimitive;
    public Material material;
    GameObject highlight;
    MeshFilter filter;
    MeshRenderer renderer;

    BoxCollider bc;
    SphereCollider sc;
    CapsuleCollider cc;
    MeshCollider mc;

    // Start is called before the first frame update
    void Start()
    {
        highlight = new GameObject(name + "'s highlight");
        filter = highlight.AddComponent<MeshFilter>();
        renderer = highlight.AddComponent<MeshRenderer>();
        renderer.material = material;
    }
    private void OnEnable()
    {
        if (highlight == null)
        {
            return;
        }
        highlight.SetActive(true);
    }
    void OnDisable()
    {
        if (highlight == null)
        {
            return;
        }
        highlight.SetActive(false);
    }
    private void OnDestroy()
    {
        Destroy(highlight);
    }

    public void AssignFromRaycastHit(RaycastHit rh)
    {
        enabled = rh.collider != null;
        if (rh.collider == null)
        {
            return;
        }

        highlighted = rh.collider;

        bc = highlighted as BoxCollider;
        sc = highlighted as SphereCollider;
        cc = highlighted as CapsuleCollider;
        mc = highlighted as MeshCollider;

        if (bc)
        {
            filter.mesh = boxPrimitive;
        }
        else if (sc)
        {
            filter.mesh = spherePrimitive;
        }
        else if (cc)
        {
            filter.mesh = capsulePrimitive;
        }
        else if (mc)
        {
            filter.mesh = mc.sharedMesh;
        }
    }


    // Update is called once per frame
    void Update()
    {
        /*
        if (bc)
        {

        }
        else if (sc)
        {

        }
        else if (cc)
        {
            highlight.transform.position = cc.transform.position + cc.transform.TransformPoint(cc.center);
            highlight.transform.rotation = cc.transform.rotation;

            switch(cc.direction)
            {
                case 0: // If aligned on X axis
                    highlight.transform.Rotate(0, 0, 90);
                    break;
                case 2: // If aligned on Z axis
                    highlight.transform.Rotate(90, 0, 0);
                    break;
                        // Y naturally lines up with transform, so do nothing
            }

            Vector3 scale = cc.transform.lossyScale;
            scale.x *= cc.radius;
            scale.y *= cc.radius;
            scale.z *= cc.height / 2;
            highlight.transform.localScale = scale;
        }
        else if (mc)
        {
            highlight.transform.position = mc.transform.position;
            highlight.transform.rotation = mc.transform.rotation;
            highlight.transform.localScale = mc.transform.lossyScale;
        }
        */
    }


    void LateUpdate()
    {
        if (highlighted == null)
        {
            enabled = false;
            return;
        }

        highlight.transform.position = highlighted.transform.position;
        highlight.transform.rotation = highlighted.transform.rotation;
        highlight.transform.localScale = highlighted.transform.lossyScale;

        if (bc)
        {
            highlight.transform.position = bc.transform.TransformPoint(bc.center);

            Vector3 localScale = highlight.transform.localScale;
            localScale.x *= bc.size.x;
            localScale.y *= bc.size.y;
            localScale.z *= bc.size.z;
            highlight.transform.localScale = localScale;
        }
        else if (sc)
        {
            highlight.transform.position = sc.transform.TransformPoint(sc.center);
            highlight.transform.localScale *= sc.radius * 2;
        }
        else if (cc)
        {
            highlight.transform.position = cc.transform.TransformPoint(cc.center);

            switch (cc.direction)
            {
                case 0: // If aligned on X axis
                    highlight.transform.Rotate(0, 0, 90);
                    break;
                case 2: // If aligned on Z axis
                    highlight.transform.Rotate(90, 0, 0);
                    break;
                    // Y naturally lines up with transform, so do nothing
            }

            Vector3 localScale = highlight.transform.localScale;
            localScale.x *= cc.radius * 2;
            localScale.y *= cc.height / 2;
            localScale.z *= cc.radius * 2;
            highlight.transform.localScale = localScale;
        }
    }

    
}
