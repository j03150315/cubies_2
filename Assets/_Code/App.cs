using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    public enum Layers
    {
        Default = 0,
        Piece = 10,
        Controller = 11,
    }
    public enum Mask
    {
        None        = 0,
        Default     = 1 << Layers.Default,
        Piece       = 1 << Layers.Piece,
        Controller  = 1 << Layers.Controller,
    }

    public static App Inst;

    public string StartVoxFile;
    public Cubie StartCubie;
    public Cubie CurrentCubie;
    public Vox VoxPrefab;
    public float SnapDistance = 0.1f;
    public float SnapAngle = 45f;
    public float CubieScale = 0.1f;
    public int MinPieceVoxels = 10;
    public Material HighlightMaterial;

    // Start is called before the first frame update
    void Awake()
    {
        Inst = this;
    }

    // Update is called once per frame
    void Start()
    {
        if (StartCubie != null)
        {
            StartCubie.SetupPrefabCubie();
            CurrentCubie = StartCubie;
        }
        else
        {
            CurrentCubie = BuildCubie(StartVoxFile);
        }
    }

    Cubie BuildCubie(string name)
    {
        // Create a parent cubie object
        GameObject go = new GameObject(name);
        Cubie cubie = go.AddComponent<Cubie>();
        cubie.BuildFromVoxFile(name);
        return cubie;
    }
}
