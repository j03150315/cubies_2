using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VoxEdit : MonoBehaviour
{
    public bool BuilderMode;

    // Use this for initialization
    void Start()
    {
        //AddVox(Key(0, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (BuilderMode)
            UpdateBuilderInput();
    }

    void UpdateBuilderInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;

            // Test hide click tile
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            RaycastHit hit;
            int mask = (int)App.Mask.Piece;
            if (Physics.Raycast(ray, out hit, 1000f, mask))
            {
                Vox vx = hit.transform.GetComponent<Vox>();
                if (vx != null)
                    OnClick(vx, hit);
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
            App.Inst.CurrentCubie.SaveVoxels();

        if (Input.GetKeyDown(KeyCode.F2))
            App.Inst.CurrentCubie.LoadVoxels();
    }

    void OnClick(Vox vx, RaycastHit hit)
    {
        // Get the side
        Vector3 normal = hit.normal;
        Vector3 newpos = vx.transform.position + normal;

        // Add another Vox
        Vox vox = App.Inst.CurrentCubie.BuildVox(vx, (int)newpos.x, (int)newpos.y, (int)newpos.z, vx.V.C);
    }
}