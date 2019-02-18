using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool Placed = false;
    public Vector3 SnapPos;
    Mesh Mesh;

    public VoxPiece VoxPiece;

    // Start is called before the first frame update
    void Start()
    {
        Transform snap = transform.Find("Snap");
        if (snap == null)
            snap = transform;
        SnapPos = transform.position;
        MeshFilter filter = GetComponentInChildren<MeshFilter>();
        Mesh = filter.mesh;
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
        float distance = Vector3.Distance(App.Inst.CurrentCubie.Base.position, SnapPos);
        if (distance < App.Inst.SnapDistance)
        {
            DrawHighlight();
        }
    }

    public void TrySnap()
    {
        float distance = Vector3.Distance(App.Inst.CurrentCubie.Base.position, SnapPos);
        if (distance < App.Inst.SnapDistance)
        {
            SnapIntoPlace();
        }
    }

    public void DrawHighlight()
    {
        //Transform anchor = App.Inst.CurrentCubie.Base;
        //Vector3 offset = transform.position - SnapPos;
        //Vector3 pos = anchor.position + offset;
        //Vector3 pos = anchor.position + SnapPos;
        Transform anchor = App.Inst.CurrentCubie.transform;
        Transform tx = App.Inst.CurrentCubie.Pieces[0].transform;
        Vector3 pos = anchor.position + Vector3.Scale(SnapPos, tx.transform.localScale);
        int layer = 1;
        Vector3 scale = Vector3.Scale(tx.localScale, tx.transform.localScale);
        Matrix4x4 m = Matrix4x4.TRS(pos, anchor.rotation, scale);
        Graphics.DrawMesh(Mesh, m, App.Inst.HighlightMaterial, layer);
    }

    public void SnapIntoPlace()
    {
        Transform anchor = App.Inst.CurrentCubie.Base;
        transform.parent = App.Inst.CurrentCubie.transform;
        transform.position = SnapPos;
        transform.rotation = Quaternion.identity;
        SetPlaced();
    }
}
