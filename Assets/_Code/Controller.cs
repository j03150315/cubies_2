using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameObject Pointer;
    public SphereCollider Grabber;
    public Collider GrabbedCollider;
    public Transform PlayerTransform;
    public Material RedMat;
    public Material GreenMat;
    public Material BlueMat;
    Renderer GrabberRenderer = null;
    Renderer PointerRenderer = null;
    public float GrabDistance = 16f;
    public float GrabSpeed = 20f;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        GrabberRenderer = Grabber.GetComponent<Renderer>();
        PointerRenderer = Pointer.GetComponent<Renderer>();
        ModifyPointerMesh();
    }

    public void ProjectGrabber(Ray ray)
    {
        RaycastHit hit;
        int mask = (int)App.Mask.Piece + (int)App.Mask.Default;
        if (Physics.Raycast(ray, out hit, 1000f, mask))
        {
            Piece piece = hit.transform.GetComponent<Piece>();
            if (piece == null)
            {
                GrabberRenderer.material = BlueMat;
                PointerRenderer.material = BlueMat;
            }
            else if (piece.Placed)
            {
                GrabberRenderer.material = RedMat;
                PointerRenderer.material = RedMat;
            }
            else
            {
                GrabberRenderer.material = GreenMat;
                PointerRenderer.material = GreenMat;
            }

            // Move the grabber
            Grabber.transform.position = hit.point;

            // Update the pointer
            float distance = hit.distance;
            Vector3 scale = Pointer.transform.localScale;
            scale.y = distance * 0.5f;
            Pointer.transform.localScale = scale;
        }
    }

    public void PullGrabbedObject()
    {
        Transform gtx = Grabber.transform;
        float scale = gtx.localScale.z;
        Vector3 vdist = gtx.localPosition;
        float dist = vdist.magnitude;
        // If grabber is further away than our desired hold distance
        if (dist > GrabDistance * scale)
        {
            // Pull it closer
            dist -= Time.deltaTime * GrabSpeed;
            Vector3 vdir = vdist.normalized * dist;
            gtx.localPosition = vdir;
        }
    }

    public void Grab()
    {
        // Try grab a collider that we are touching
        int mask = (int)App.Mask.Piece; // + (int)Mask.Controller;
        float radius = Grabber.radius * Grabber.transform.localScale.x;
        Collider[] colliders = Physics.OverlapSphere(Grabber.transform.position, radius, mask);
        foreach (Collider collider in colliders)
        {
            // Don't grab placed pieces
            Piece piece = collider.GetComponent<Piece>();
            if (piece == null || piece.Placed)
                continue;

            GrabbedCollider = collider;
            //collider.transform.parent = Grabber.transform;
            collider.transform.parent = Grabber.transform;

            // Turn off physics
            Rigidbody body = GrabbedCollider.GetComponentInParent<Rigidbody>();
            body.isKinematic = true;
            break;
        }
    }

    public void Release()
    {
        Piece piece = GrabbedCollider.GetComponentInParent<Piece>();

        // Turn on physics
        Rigidbody body = GrabbedCollider.GetComponentInParent<Rigidbody>();
        body.isKinematic = false;

        // Drop the object
        GrabbedCollider.transform.parent = null;
        GrabbedCollider = null;

        piece.TrySnap();
    }

    public void ModifyPointerMesh()
    {
        MeshFilter filter = Pointer.GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        Vector3 offset = Vector3.zero;
        offset.y = mesh.bounds.extents.y;

        // Rebuild the verts
        int numVerts = mesh.vertexCount;
        Vector3[] oldVerts = mesh.vertices;
        Vector3[] newVerts = new Vector3[numVerts];
        for (int i = 0; i < numVerts; i++)
        {
            newVerts[i] = oldVerts[i] + offset;
        }
        mesh.vertices = newVerts;
        mesh.UploadMeshData(true);
    }

    public void Teleport(Vector3 dest)
    {
        dest.y = PlayerTransform.position.y;
        PlayerTransform.position = dest;
    }
}
