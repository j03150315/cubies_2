using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class VoxEdit : MonoBehaviour
{
    public Vox VoxPrefab;
    public Dictionary<int, Voxel> Voxels = new Dictionary<int, Voxel>();
    public Dictionary<int, Vox> Voxs = new Dictionary<int, Vox>();

    public int Key(int x, int y, int z)
    {
        return (x << 20) + (y << 10) + z;
    }

    public int Key(Voxel v)
    {
        return (v.X << 20) + (v.Y << 10) + v.Z;
    }

    public void XYZ(int key, out int x, out int y, out int z)
    {
        z = key & 1023;
        y = (key >> 10) & 1023;
        x = (key >> 20) & 1023;
    }

    // Use this for initialization
    void Start()
    {
        //AddVox(Key(0, 0, 0));
        ReadMagicaVoxel();
        SplitIntoPieces();
        BuildPieces();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
    }

    void UpdateInput()
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
            SaveVoxels();

        if (Input.GetKeyDown(KeyCode.F2))
            LoadVoxels();
    }

    void OnClick(Vox vx, RaycastHit hit)
    {
        // Get the side
        Vector3 normal = hit.normal;
        Vector3 newpos = vx.transform.position + normal;

        // Add another Vox
        Vox vox = BuildVox(vx, (int)newpos.x, (int)newpos.y, (int)newpos.z, vx.V.C);
    }

    public Voxel AddVoxel(int key, int c)
    {
        int x, y, z;
        XYZ(key, out x, out y, out z);
        return AddVoxel(x, y, z, c);
    }

    public Voxel AddVoxel(int x, int y, int z, int c)
    {
        Voxel v = new Voxel(x, y, z, c);
        int key = Key(x, y, z);
        Voxels[key] = v;

        return v;
    }

    public Vox BuildVox(Vox template, int x, int y, int z, int c)
    {
        Voxel v = AddVoxel(x, y, z, c);
        Vox vox = BuildVox(template, v);
        int key = Key(x, y, z);
        Voxs[key] = vox;
        return vox;
    }

    public Vox BuildVox(Vox template, Voxel v)
    {
        GameObject go = Instantiate(template.gameObject);
        go.SetActive(true);
        Vox vox = go.GetComponent<Vox>();
        go.transform.position = new Vector3(v.X, v.Y, v.Z);

        return vox;
    }

    string SavePath = Application.streamingAssetsPath + "/Test.QB";
    int kCubieFileVersion = 1;
    void SaveVoxels()
    {
        FileStream fs = new FileStream(SavePath, FileMode.Create);
        BinaryWriter w = new BinaryWriter(fs);

        // Write TAG
        w.Write((byte)'Q');
        w.Write((byte)'B');

        // Write version
        w.Write((byte)0);

        // Write part count
        w.Write((int)Voxels.Count);

        // Write part
        foreach (var pair in Voxels)
        {
            w.Write(pair.Key);
        }
    }

    bool LoadVoxels()
    {
        using (BinaryReader r = new BinaryReader(File.Open(SavePath, FileMode.Open)))
        {
            // Validate TAG
            if (r.ReadByte() != 'Q')
                return false;
            if (r.ReadByte() != 'B')
                return false;

            // Read version
            byte ver = r.ReadByte();
            if (ver > kCubieFileVersion)
                return false;

            // Read voxel count
            int voxelCount = r.ReadInt32();

            // Read voxels
            for (int vi = 0; vi < voxelCount; vi++)
            {
                // Read part name
                int key = r.ReadInt32();

                // Create a voxel at that point
                AddVoxel(key, 0);
            }

            return true;
        }
    }

    // Read MagicaVoxel
    //      Read as RAW Voxel data
    bool ReadMagicaVoxel()
    {
        string path = @"C:\git\cubies_2\Assets\StreamingAssets\monu0.vox";
        using (BinaryReader r = new BinaryReader(File.Open(path, FileMode.Open)))
        {
            // Validate TAG
            if (r.ReadByte() != 'V')
                return false;
            if (r.ReadByte() != 'O')
                return false;
            if (r.ReadByte() != 'X')
                return false;
            if (r.ReadByte() != ' ')
                return false;

            // Read version
            int ver = r.ReadInt32();
            if (ver > 150)
                return false;

            // Read model count
            int modelCount = 1;
            int numBytes = 0;
            int numChildren = 0;
            int sizeX = 0;
            int sizeY = 0;
            int sizeZ = 0;

            while (modelCount > 0)
            {
                // Read chunk header
                string cid = ReadChunkID(r);
                numBytes = r.ReadInt32();
                numChildren = r.ReadInt32();
                switch (cid)
                {
                    case "MAIN":
                        //modelCount = 0;
                        break;
                    case "PACK":
                        // TODO
                        modelCount = r.ReadInt32();
                        break;
                    case "SIZE":
                        // Gravity direction swizzled with Y for Unity
                        sizeX = r.ReadInt32();
                        sizeZ = r.ReadInt32();
                        sizeY = r.ReadInt32();
                        break;
                    case "XYZI":
                        int numVoxels = r.ReadInt32();
                        for (int vi = 0; vi < numVoxels; vi++)
                        {
                            // Gravity direction swizzled with Y for Unity
                            int x = r.ReadByte();
                            int z = r.ReadByte();
                            int y = r.ReadByte();
                            int c = r.ReadByte();
                            //BuildVox(VoxPrefab, x, y, z, c);
                            AddVoxel(x, y, z, c);
                        }
                        modelCount--;
                        break;
                }
            }
        }

        return true;
    }

    string ReadChunkID(BinaryReader r)
    {
        char[] b = r.ReadChars(4);
        StringBuilder sb = new StringBuilder(5);
        sb.Append(b, 0, 4);
        return sb.ToString();
    }

    // Split into pieces
    //     For each unattached voxel
    //          Create a piece
    //           Recursively:
    //               Randomly choose if neighbors are attached
    //               Reduce chance of attaching
    //               Recurse attached pieces
    void SplitIntoPieces()
    {
        foreach (Voxel v in Voxels.Values)
        {
            // If piece is not attached yet
            if (v.Piece == null && v.Y < 60f)
            {
                // Start a new piece by recursively attaching
                VoxPiece vp = new VoxPiece();

                //int key = Key(v.X, v.Y, v.Z);
                vp.Key = Key(v);
                RecursiveAttach(vp, v, 1f);

                VoxPieces[vp.Key] = vp;
            }
        }

        Debug.LogWarning(">> Split into " + VoxPieces.Count + " pieces");

    }

    Dictionary<int, VoxPiece> VoxPieces = new Dictionary<int, VoxPiece>();

    void RecursiveAttach(VoxPiece vp, Voxel v, float chance)
    {
        v.Piece = vp;
        vp.Voxels.Add(v);

        // Try find piece in each direction
        RandomlyAttach(vp, v.X - 1, v.Y, v.Z, chance);
        RandomlyAttach(vp, v.X + 1, v.Y, v.Z, chance);
        RandomlyAttach(vp, v.X, v.Y - 1, v.Z, chance);
        RandomlyAttach(vp, v.X, v.Y + 1, v.Z, chance);
        RandomlyAttach(vp, v.X, v.Y, v.Z - 1, chance);
        RandomlyAttach(vp, v.X, v.Y, v.Z + 1, chance);
    }

    void RandomlyAttach(VoxPiece vp, int x, int y, int z, float chance)
    {
        if (Random.value <= chance)
        {
            Voxel child = TryGetVoxel(x, y, z);
            if (child != null && child.Piece == null)
                RecursiveAttach(vp, child, chance * 0.5f);
        }
    }

    Voxel TryGetVoxel(int x, int y, int z)
    {
        int key = Key(x, y, z);
        Voxel v = null;
        Voxels.TryGetValue(key, out v);
        return v;
    }

    // Build Geo per piece
    //     Just outside edges (not if neighbor on that face)
    //     Set UVs to color
    void BuildPieces()
    {
        // For each voxel in the pieces
        int max = 100000;
        int num = 0;
        foreach (VoxPiece vp in VoxPieces.Values)
        {
            vp.Mat = num * 17;
            BuildPiece(vp);
            num++;
            if (num >= max)
                break;
        }

        Debug.LogWarning(">> Built " + num + " pieces");
    }

    List<Vector3> Verts = new List<Vector3>();
    List<Vector3> Norms = new List<Vector3>();
    List<Vector4> Tangs = new List<Vector4>();
    List<Vector2> UVs = new List<Vector2>();
    List<int>     Indices = new List<int>();
    int Vert = 0;
    Vox BuildPiece(VoxPiece vp)
    {
        GameObject go = Instantiate(VoxPrefab.gameObject);
        go.SetActive(true);
        Vox vox = go.GetComponent<Vox>();
        Voxel firstVoxel = vp.Voxels[0];
        Vector3 pos = new Vector3(firstVoxel.X, firstVoxel.Y, firstVoxel.Z);
        go.transform.position = pos;

        MeshFilter filter = go.GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;

        Verts.Clear();
        Norms.Clear();
        Tangs.Clear();
        UVs.Clear();
        Indices.Clear();
        Vert = 0;

        // For each voxel in the pieces
        foreach (Voxel v in vp.Voxels)
        {
            // For each face on the voxel
            AddFace(vp, v, pos, Vector3.left);
            AddFace(vp, v, pos, -Vector3.left);
            AddFace(vp, v, pos, Vector3.up);
            AddFace(vp, v, pos, -Vector3.up);
            AddFace(vp, v, pos, Vector3.forward);
            AddFace(vp, v, pos, -Vector3.forward);
        }

        mesh.Clear();
        mesh.SetVertices(Verts);
        mesh.SetNormals(Norms);
        mesh.SetTangents(Tangs);
        mesh.SetUVs(0, UVs);
        mesh.SetIndices(Indices.ToArray(), MeshTopology.Triangles, 0);

        return vox;
    }

    void AddFace(VoxPiece vp, Voxel v, Vector3 pos, Vector3 n)
    {
        // If no neighbor on that face is part of the same piece
        Voxel neighbor = TryGetVoxel(v.X + (int)n.x, v.Y + (int)n.y, v.Z + (int)n.z);
        if (neighbor == null || neighbor.Piece != vp)
        {
            // Add quad for that face with color UV
            AddVoxelFace(v, pos, n, vp.Mat);
        }
    }

    void AddVoxelFace(Voxel v, Vector3 pos, Vector3 n, int mat)
    {
        //mat = v.C;
        float fmat = ((float)mat) / 255f;
        Vector2 uv = new Vector2(fmat, 0);
        //Vector2 uv = new Vector2(mat, mat);
        // x,y,z is the center of the voxel
        float r = 0.5f;
        Vector3 h = n * r;
        Vector3 c = new Vector3(v.X,v.Y,v.Z) - pos;
        Vector3 tl = c;
        if (h.x != 0f)
        {
            Verts.Add(c + new Vector3(h.x, -r, +r));
            Verts.Add(c + new Vector3(h.x, +r, +r));
            Verts.Add(c + new Vector3(h.x, +r, -r));
            Verts.Add(c + new Vector3(h.x, -r, -r));
            Tangs.Add(Vector3.forward);
            Tangs.Add(Vector3.forward);
            Tangs.Add(Vector3.forward);
            Tangs.Add(Vector3.forward);
        }
        else if (n.y != 0f)
        {
            Verts.Add(c + new Vector3(+r, h.y, +r));
            Verts.Add(c + new Vector3(-r, h.y, +r));
            Verts.Add(c + new Vector3(-r, h.y, -r));
            Verts.Add(c + new Vector3(+r, h.y, -r));
            Tangs.Add(Vector3.left);
            Tangs.Add(Vector3.left);
            Tangs.Add(Vector3.left);
            Tangs.Add(Vector3.left);
        }
        else if (n.z != 0f)
        {
            Verts.Add(c + new Vector3(-r, +r, h.z));
            Verts.Add(c + new Vector3(+r, +r, h.z));
            Verts.Add(c + new Vector3(+r, -r, h.z));
            Verts.Add(c + new Vector3(-r, -r, h.z));
            Tangs.Add(Vector3.up);
            Tangs.Add(Vector3.up);
            Tangs.Add(Vector3.up);
            Tangs.Add(Vector3.up);
        }

        Norms.Add(n);
        Norms.Add(n);
        Norms.Add(n);
        Norms.Add(n);
        UVs.Add(uv);
        UVs.Add(uv);
        UVs.Add(uv);
        UVs.Add(uv);

        if (n.x + n.y + n.z < -0.1f)
        {
            Indices.Add(Vert);
            Indices.Add(Vert + 1);
            Indices.Add(Vert + 3);

            Indices.Add(Vert + 1);
            Indices.Add(Vert + 2);
            Indices.Add(Vert + 3);
        }
        else
        {
            Indices.Add(Vert);
            Indices.Add(Vert + 3);
            Indices.Add(Vert + 1);

            Indices.Add(Vert + 1);
            Indices.Add(Vert + 3);
            Indices.Add(Vert + 2);
        }

        Vert += 4;
    }

}