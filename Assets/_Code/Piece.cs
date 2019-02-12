using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool Placed = false;
    Transform Snap;
    Mesh Mesh;

    // Start is called before the first frame update
    void Start()
    {
        Snap = transform.Find("Snap");
        MeshFilter filter = GetComponentInChildren<MeshFilter>();
        Mesh = filter.mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlaced()
    {
        // Turn off physics
        Rigidbody body = GetComponentInParent<Rigidbody>();
        body.isKinematic = true;
        Placed = true;
    }

    public void TryHighlight()
    {
        float distance = Vector3.Distance(App.Inst.CurrentCubie.Base.position, Snap.position);
        if (distance < App.Inst.SnapDistance)
        {
            DrawHighlight();
        }
    }

    public void TrySnap()
    {
        float distance = Vector3.Distance(App.Inst.CurrentCubie.Base.position, Snap.position);
        if (distance < App.Inst.SnapDistance)
        {
            SnapIntoPlace();
        }
    }

    public void DrawHighlight()
    {
        Transform anchor = App.Inst.CurrentCubie.Base;
        Vector3 offset = transform.position - Snap.position;
        Vector3 pos = anchor.position + offset;
        int layer = 1;
        Transform tx = App.Inst.CurrentCubie.Pieces[0].transform;
        Vector3 scale = Vector3.Scale(tx.localScale, tx.parent.transform.localScale);
        Matrix4x4 m = Matrix4x4.TRS(pos, anchor.rotation, scale);

        Graphics.DrawMesh(Mesh, m, App.Inst.HighlightMaterial, layer);
    }

    public void SnapIntoPlace()
    {
        Transform anchor = App.Inst.CurrentCubie.Base;
        Vector3 offset = transform.position - Snap.position;
        Vector3 pos = anchor.position + offset;

        transform.position = pos;
        transform.rotation = anchor.rotation;
        transform.parent = App.Inst.CurrentCubie.transform;

        SetPlaced();
    }
}
