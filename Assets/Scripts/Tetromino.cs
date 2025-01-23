using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino // type of the piece
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z
}

[System.Serializable]
public struct TetrominoData // data that defines the piece
{
    public Tetromino tetromino; // type of it
    public Tile tile; // its tile
    public Vector2Int[] Cells {  get; private set; }  // relative positions of its cells (tiles) 
    public Vector2Int[,] WallKicks { get; private set; } // wallkicks translations 
    public void Initialize() // setting cells and wallkicks based on tetromino type
    {
        Cells = Data.Cells[tetromino];
        WallKicks = Data.WallKicks[tetromino];
    }
}