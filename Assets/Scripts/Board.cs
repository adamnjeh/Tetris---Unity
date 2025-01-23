using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {  get; private set; } // reference to tilemap of main board 
    public Piece ActivePiece { get; private set; } // our moving piece
    public TetrominoData[] tetrominoes; // all possible pieces
    public Vector3Int StartingPosition; // where our piece shall be spawned
    public RectInt bounds { get; private set; } // bounding the board to check for out of bounds pieces

    private void Awake()
    {
        bounds = new RectInt(new Vector2Int(-5, -10), new Vector2Int(10, 20));
        tilemap = GetComponentInChildren<Tilemap>();
        ActivePiece = GetComponent<Piece>();

        for(int i = 0; i < tetrominoes.Length; i++) // set all the pieces ready
        {
            tetrominoes[i].Initialize();
        }  
        
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length); // randomize the spawned piece
        TetrominoData data = tetrominoes[random];
        ActivePiece.Initialize(this, StartingPosition, data); // initilize/reset the active piece with the new data

        if (!IsValid(StartingPosition, ActivePiece.Cells)) // if there is no valid space for spawning the piece
        {
            tilemap.ClearAllTiles(); // clear the board and reset the game
        }
        Set(); // else set the piece in spawning position
        ActivePiece.ghost.SetGhost(); // and activate its ghost
    }

    public void Set() // draw the tiles in the positions of cells
    {

        for (int i = 0; i < ActivePiece.Cells.Length; i++) // for each cell relative position
        {
            Vector3Int position = ActivePiece.Cells[i] + ActivePiece.Position; // we calculate the actual postion in the board
            tilemap.SetTile(position, ActivePiece.Data.tile); // and draw the tile
        }

    }

    public void Clear() // clear the tiles on the positions of cells
    {
        for(int i = 0; i < ActivePiece.Cells.Length; i++) // for each cell relative position
        {
            Vector3Int position = ActivePiece.Cells[i] + ActivePiece.Position; // we calculate the actual cell position
            tilemap.SetTile(position, null); // and we remove the tile
        }

    }

    public bool IsValid(Vector3Int position, Vector3Int[] cells) // check if that position is valid to be placed in
    {
        for(int i = 0; i < cells.Length; i++) // for each cell relative position
        {
            Vector3Int newPosition = cells[i] + position; // we calculate the actual cell position

            if (tilemap.HasTile(newPosition)) // if that position has a tile
            {
                return false; // not valid
            }

            if (!bounds.Contains((Vector2Int)newPosition)){ // if that position out of bound
                return false; // not valid
            }
        }
        return true; // everything is good so valid
    }

    public void ClearLines() // clear full lines
    {
        for(int row = bounds.yMin;  row < bounds.yMax; row++) // we go from the very buttom to the very max of our bound
        {
            if (IsLineFull(row)) // if that row/line is full
            {
                ClearCurrentLine(row); // we clear it
                row--; // we decrease the row by one to check if the line that was full is full again
            }
        }
    }

    bool IsLineFull(int row) // check if a row is full
    {
        for(int col = bounds.xMin;  col < bounds.xMax; col++) // we go from left to right
        {
            if(!tilemap.HasTile(new Vector3Int(col, row, 0))) // if there is a position that has no tile
            {
                return false; // then line not full
            }
        }
        return true; // else it is a full line
    }

    void ClearCurrentLine(int currentRow) // clear the line and make all the lines above it fall by one line
    {
        for (int row = currentRow; row < bounds.yMax; row++) // for each row from the current one going up
        {
            for(int col = bounds.xMin; col < bounds.xMax; col++) // for each column in that row 
            {
                tilemap.SetTile(new Vector3Int(col, row, 0), tilemap.GetTile(new Vector3Int(col, row + 1, 0))); // we set its tile to the one just above it
            }
        }
    }

}
