using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    public Voxel(int x, int y, int z, int c)
    {
        X = x;
        Y = y;
        Z = z;
        C = c;
    }

    public int X;
    public int Y;
    public int Z;
    public int C;

    public VoxPiece Piece = null;
}
