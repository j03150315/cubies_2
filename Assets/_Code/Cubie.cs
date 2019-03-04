using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

public class Cubie : MonoBehaviour
{
    public List<Piece> Pieces = new List<Piece>();
    public Transform Base;

    public Dictionary<int, Voxel> Voxels = new Dictionary<int, Voxel>();
    public Dictionary<int, Vox> Voxs = new Dictionary<int, Vox>();
    public Color32[] Palette = null;

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

    public void SetupPrefabCubie()
    {
        Piece[] all = GetComponentsInChildren<Piece>();
        foreach (Piece piece in all)
        {
            Pieces.Add(piece);
        }
        Pieces[0].SetPlaced();

        Base = transform.Find("Base");
    }

    public void BuildFromVoxFile(string name)
    {
        ReadMagicaVoxel(name);
        SplitIntoPieces();

        BuildPieces(name);

        // Set the start piece
        Piece first = Pieces[0];
        Base = first.transform;
        first.SetPlaced();

        //Explode();
    }

    public void Explode()
    {
        foreach (Piece piece in Pieces)
        {
            if (piece.transform != Base)
                ExplodePiece(piece);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
            Explode();
    }

    // Read MagicaVoxel
    //      Read as RAW Voxel data
    bool ReadMagicaVoxel(string name)
    {
        string path = @"Assets\StreamingAssets\" + name + @".vox";
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

            while (modelCount > 0 || Palette == null)
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
                    case "RGBA":
                        Palette = new Color32[256];
                        for (int i = 0; i <= 254; i++)
                        {
                            byte R = r.ReadByte();
                            byte G = r.ReadByte();
                            byte B = r.ReadByte();
                            byte A = r.ReadByte();
                            Palette[i + 1] = new Color32(R,G,B,A);
                        }
                        break;
                    default:
                        // Skip chunks we don't care about
                        r.ReadBytes(numBytes);
                        break;
                }
            }
        }

        Debug.LogWarning(">> Read " + Voxels.Count + " voxels");

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

        // Find small pieces
        int minSize = App.Inst.MinPieceVoxels;
        List<VoxPiece> smallPieces = new List<VoxPiece>();
        foreach (var pair in VoxPieces)
        {
            VoxPiece piece = pair.Value;
            if (piece.Voxels.Count < minSize)
                smallPieces.Add(piece);
        }

        // Remove small pieces from the list
        foreach (VoxPiece piece in smallPieces)
        {
            VoxPieces.Remove(piece.Key);
        }

        // Combine small piece with existing large piece
        foreach (VoxPiece piece in smallPieces)
        {
            AttachToNeigbor(piece);
        }

        Debug.LogWarning(">> Split into " + VoxPieces.Count + " pieces");
    }

    void AttachToNeigbor(VoxPiece vp)
    {
        foreach (Voxel v in vp.Voxels)
        {
            // Try find piece in each direction
            if (AttachToNeigbor(vp, v.X - 1, v.Y, v.Z))
                return;
            if (AttachToNeigbor(vp, v.X + 1, v.Y, v.Z))
                return;
            if (AttachToNeigbor(vp, v.X, v.Y - 1, v.Z))
                return;
            if (AttachToNeigbor(vp, v.X, v.Y + 1, v.Z))
                return;
            if (AttachToNeigbor(vp, v.X, v.Y, v.Z - 1))
                return;
            if (AttachToNeigbor(vp, v.X, v.Y, v.Z + 1))
                return;
        }

    }

    bool AttachToNeigbor(VoxPiece piece, int x, int y, int z)
    {
        Voxel neighbor = TryGetVoxel(x, y, z);
        if (neighbor != null && neighbor.Piece != piece)
        {
            Attach(piece, neighbor.Piece);
            return true;
        }

        return false;
    }

    void Attach(VoxPiece piece, VoxPiece neigbor)
    {
        foreach (Voxel v in piece.Voxels)
        {
            v.Piece = neigbor;
            neigbor.Voxels.Add(v);
        }
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
                RecursiveAttach(vp, child, chance * 0.9f);
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
    void BuildPieces(string name)
    {
        // Read the texture
        //string path = @"Assets\StreamingAssets\" + name + @".png";
        //byte[] imgData = File.ReadAllBytes(path);
        //Texture2D tex = new Texture2D(256, 1);
        //tex.LoadImage(imgData);

        byte[] imgData = new byte[4 * 256];
        Texture2D tex = new Texture2D(256, 1);
        tex.SetPixels32(Palette);
        tex.Apply();

        // Create a new material
        Renderer prefabRend = App.Inst.VoxPrefab.GetComponentInChildren<Renderer>();
        Material mat = new Material(prefabRend.sharedMaterial);
        mat.mainTexture = tex;

        // For each voxel in the pieces
        int max = 100000;
        int num = 0;
        foreach (VoxPiece vp in VoxPieces.Values)
        {
            vp.Mat = num * 17;
            Piece piece = BuildPiece(vp, mat);
            Pieces.Add(piece);
            piece.transform.parent = transform;
            num++;
            if (num >= max)
                break;
        }

        Debug.LogWarning(">> Built " + num + " pieces");
    }

    List<Vector3> Verts = new List<Vector3>();
    List<Vector3> Norms = new List<Vector3>();
    List<Vector4> Tangs = new List<Vector4>();
    List<Color> Colors = new List<Color>();
    List<Vector2> UVs = new List<Vector2>();
    List<int> Indices = new List<int>();
    int Vert = 0;
    Piece BuildPiece(VoxPiece vp, Material mat)
    {
        GameObject go = Instantiate(App.Inst.VoxPrefab.gameObject);
        go.SetActive(true);
        Piece piece = go.GetComponent<Piece>();
        Voxel firstVoxel = vp.Voxels[0];
        float scale = App.Inst.CubieScale;
        Vector3 pos = new Vector3(firstVoxel.X, firstVoxel.Y, firstVoxel.Z);
        go.transform.position = pos * scale;
        piece.SnapPos = go.transform.localPosition;

        MeshFilter filter = go.GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;

        // Set the texture
        Renderer rend = go.GetComponent<Renderer>();
        rend.material = mat;

        Verts.Clear();
        Norms.Clear();
        Tangs.Clear();
        Colors.Clear();
        UVs.Clear();
        Indices.Clear();
        Vert = 0;

        // For each voxel in the pieces
        foreach (Voxel v in vp.Voxels)
        {
            // For each face on the voxel
            AddFace(vp, v, pos, scale, Vector3.left);
            AddFace(vp, v, pos, scale, -Vector3.left);
            AddFace(vp, v, pos, scale, Vector3.up);
            AddFace(vp, v, pos, scale, -Vector3.up);
            AddFace(vp, v, pos, scale, Vector3.forward);
            AddFace(vp, v, pos, scale, -Vector3.forward);
        }

        mesh.Clear();
        mesh.SetVertices(Verts);
        mesh.SetNormals(Norms);
        mesh.SetTangents(Tangs);
        mesh.SetColors(Colors);
        mesh.SetUVs(0, UVs);
        mesh.SetIndices(Indices.ToArray(), MeshTopology.Triangles, 0);

        // Rebuild the physics mesh
        MeshCollider col = go.GetComponent<MeshCollider>();
        col.sharedMesh = mesh;

        Rigidbody body = piece.GetComponent<Rigidbody>();
        body.isKinematic = true;

        BakeAppertureLighting(piece);

        return piece;
    }

    void ExplodePiece(Piece piece)
    {
        piece.Placed = false;
        piece.transform.parent = null;
        // Apply some force to move it apart
        Rigidbody body = piece.GetComponent<Rigidbody>();
        body.isKinematic = false;
        float maxF = 0.1f;
        float maxP = 0.1f;
        Vector3 force = new Vector3(Random.Range(-maxF, maxF), Random.Range(-maxF, maxF), Random.Range(-maxF, maxF));
        Vector3 fpos = new Vector3(Random.Range(-maxP, maxP), Random.Range(-maxP, maxP), Random.Range(-maxP, maxP));
        body.AddForceAtPosition(force, fpos, ForceMode.Impulse);

    }

    void AddFace(VoxPiece vp, Voxel v, Vector3 pos, float scale, Vector3 n)
    {
        // If no neighbor on that face is part of the same piece
        Voxel neighbor = TryGetVoxel(v.X + (int)n.x, v.Y + (int)n.y, v.Z + (int)n.z);
        if (neighbor == null || neighbor.Piece != vp)
        {
            // Get the voxel color
            int mat = v.C;

            // If there is a neighbor on another piece then this is an interior piece
            if (neighbor != null)
                mat = 1;

            // Add quad for that face with color UV
            AddVoxelFace(v, pos, scale, n, mat);
        }
    }

    void AddVoxelFace(Voxel v, Vector3 pos, float scale, Vector3 n, int mat)
    {
        // Set the color of the voxel by adjusting the uv
        float fmat = ((float)mat) / 255f;
        Vector2 uv = new Vector2(fmat, 0);
        // x,y,z is the center of the voxel
        float r = 0.5f;
        Vector3 h = n * r;
        Vector3 c = (new Vector3(v.X, v.Y, v.Z) - pos) * scale;
        if (h.x != 0f)
        {
            Verts.Add(c + new Vector3(h.x, -r, +r) * scale);
            Verts.Add(c + new Vector3(h.x, +r, +r) * scale);
            Verts.Add(c + new Vector3(h.x, +r, -r) * scale);
            Verts.Add(c + new Vector3(h.x, -r, -r) * scale);
            Tangs.Add(Vector3.forward);
            Tangs.Add(Vector3.forward);
            Tangs.Add(Vector3.forward);
            Tangs.Add(Vector3.forward);
        }
        else if (n.y != 0f)
        {
            Verts.Add(c + new Vector3(+r, h.y, +r) * scale);
            Verts.Add(c + new Vector3(-r, h.y, +r) * scale);
            Verts.Add(c + new Vector3(-r, h.y, -r) * scale);
            Verts.Add(c + new Vector3(+r, h.y, -r) * scale);
            Tangs.Add(Vector3.left);
            Tangs.Add(Vector3.left);
            Tangs.Add(Vector3.left);
            Tangs.Add(Vector3.left);
        }
        else if (n.z != 0f)
        {
            Verts.Add(c + new Vector3(-r, +r, h.z) * scale);
            Verts.Add(c + new Vector3(+r, +r, h.z) * scale);
            Verts.Add(c + new Vector3(+r, -r, h.z) * scale);
            Verts.Add(c + new Vector3(-r, -r, h.z) * scale);
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

        Colors.Add(new Color(1, 0, 0, 1));
        Colors.Add(new Color(0, 1, 0, 1));
        Colors.Add(new Color(0, 0, 1, 1));
        Colors.Add(new Color(1, 0, 1, 1));

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

    #region In game voxel building
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
    public void SaveVoxels()
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

    public bool LoadVoxels()
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

    void BakeAppertureLighting(Piece piece)
    {
        int numVerts = Verts.Count;
        Collider collider = piece.GetComponent<Collider>();

        for (int i=0; i<numVerts; i++)
        {
            Vector4 ap = GetAppertureLight(collider, Verts[i], Norms[i]);
            Colors[i] = new Color(ap.x,ap.y,ap.z,ap.w);
        }

        MeshFilter filter = piece.GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.SetColors(Colors);
    }

    Vector4 GetAppertureLight(Collider collider, Vector3 pos, Vector3 normal)
    {
        int numRays = 128;
        Vector3[] rays = FibonacciSphere(numRays);
        Ray ray = new Ray();
        RaycastHit hit = new RaycastHit();
        ray.origin = pos;
        float maxDist = 10f;

        float visible = 1f;
        Vector3 bentNormal = normal * 0.0001f;
        foreach (Vector3 raydir in rays)
        {
            ray.direction = raydir;
            if (!collider.Raycast(ray, out hit, maxDist))
            {
                bentNormal += raydir;
                visible += 1f;
            }
        }
        bentNormal = bentNormal.normalized;
        Vector4 apperture = new Vector4(bentNormal.x, bentNormal.y, bentNormal.z, visible / numRays); 
        return apperture;
    }


    public class SpherePoint
    {
        float lat, lon;
        public SpherePoint(float lat, float lon)
        {
            this.lat = lat;
            this.lon = lon;
        }
    };

    // https://www.openprocessing.org/sketch/41142
    SpherePoint[] FibonacciSphere2(int samples = 2)
    {
        float phi = (Mathf.Sqrt(5) + 1) / 2 - 1; // golden ratio
        float ga = phi * 2 * Mathf.PI;           // golden angle
        SpherePoint[] pts = new SpherePoint[samples];
        for (int i = 1; i <= samples; ++i)
        {
            float lon = ga * i;
            lon /= 2 * Mathf.PI; lon -= Mathf.Floor(lon); lon *= 2 * Mathf.PI;
            if (lon > Mathf.PI) lon -= 2 * Mathf.PI;

            // Convert dome height (which is proportional to surface area) to latitude
            float lat = Mathf.Asin(-1 + 2 * i / (float)samples);

            pts[i] = new SpherePoint(lat, lon);
        }

        return pts;
    }

    Vector3[] FibonacciSphere(int samples = 1, bool randomize = true)
    {
        float rnd = 1f;
        if (randomize)
            rnd = Random.value * samples;

        Vector3[] points = new Vector3[samples];
        float offset = 2f / samples;
        float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < samples; i++)
        {
            float y = ((i * offset) - 1) + (offset / 2);
            //float r = Mathf.Sqrt(1f - Mathf.Pow(y, 2));
            float r = Mathf.Sqrt(1f - y*y);

            float phi = ((i + rnd) % samples) * increment;

            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }
    #endregion
}
