using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tilemap tilemap {  get; private set; } // reference to ghost board
    public Vector3Int[] Cells {  get; private set; } // cells of the ghost piece

    public Tile tile; // ghost tile
    public Board board;

    Vector3Int position; // position of the ghost piece

    public void Initilize() // same process as the active piece
    {
        tilemap = GetComponentInChildren<Tilemap>();
        position = board.ActivePiece.Position;
        
        if(Cells == null)
            Cells = new Vector3Int[4];

        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i] = board.ActivePiece.Cells[i];
        }
    }

    public void SetGhost() // setting/updating ghost after every movement of active piece
    {
        Clear(); // we clear current ghost tiles
        Copy(); // we set ghost cells position to its active piece cells position
        Fall(); // we perform a fall to the possible buttom of the board
        Set(); // we draw ghost tiles
    }

    public void Clear()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            tilemap.SetTile(position + Cells[i], null);
        }
    }

    void Copy()
    {
        position = board.ActivePiece.Position;
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i] = board.ActivePiece.Cells[i];
        }
    }

    void Fall()
    {
        board.Clear(); // we clear so the ghost doesn't get confused with its piece cells
        for(int row = position.y; row >= board.bounds.yMin - 1; row--) // starting from current line moving down to the buttom
        {
            if (board.IsValid(position - Vector3Int.up, Cells)) // if it's a valid position
            {
                position.y = row; // we update ghost position
            }
            else
            {
                board.Set(); // else we stop (we got the needed ghost position) and draw back the active piece
                return;
            }
        }
        board.Set();
    }

    void Set()
    {
        for(int i = 0; i < Cells.Length; i++)
        {
            tilemap.SetTile(position + Cells[i], tile);
        }
    }
}
