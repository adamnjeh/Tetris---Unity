using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board Board { get; private set; } // reference to the board 
    public Vector3Int Position { get; private set; } // position of this piece
    public TetrominoData Data { get; private set; } // data attached to this piece
    public Vector3Int[] Cells { get; private set; } // cells positions of this piece
    public int RotationIndex {  get; private set; } // rotation index refering to current direction going from 0 as 0° to 3 as 270°

    public Ghost ghost; // reference to the ghost following this piece
    public float StepDelay = 1; // amount of time to perform a step down automatically
    public float LockDelay = .5f; // amount of time of inactivity before locking the piece on the board and spawning another

    float stepTime = 0; // counter for step delay
    float lockTime = 0; // counter for lock delay

    public void Initialize(Board board, Vector3Int position, TetrominoData data) // initializing/resetting the piece's variables
    {
        this.Board = board;
        this.Position = position;
        this.Data = data;

        if(Cells == null) // for the first time, we allocate place for the cells
        {
            Cells = new Vector3Int[data.Cells.Length];
        }

        for(int i = 0; i < Cells.Length; i++) // then we set each cell with the correspanding data
        {
            Cells[i] = (Vector3Int)data.Cells[i];
        }
        ghost.Initilize(); // then we initialize its ghost
        RotationIndex = 0; // and set the rotation to its default state
    }

    private void Update()
    {
        UpdateTimers(); // update counters
        PerformStep(); // perform step down if counter is up
        InputHandler(); // read the inputs and make actions
    }

    void UpdateTimers() 
    {
        stepTime += Time.deltaTime;
        lockTime += Time.deltaTime;
    }

    void PerformStep()
    {     
        if (stepTime >= StepDelay) // when step counter is up
        {
            stepTime = 0; /// reset it first
            TryMoving(Vector3Int.down); // perform a step down
            if(lockTime > LockDelay) // if teh piece inactive for some time
            {
                Lock(); // lock it and respawn another
            }
        }
    }

    void Lock()
    {
        ghost.Clear(); // when locked, we erase the ghost
        Board.ClearLines(); // we check for full lines to be cleared
        Board.SpawnPiece(); // and finally we respawn another piece
    }

    void InputHandler() // for each movement/rotation we give the corresponding translation/direction
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryMoving(Vector3Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryMoving(Vector3Int.right);
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryMoving(Vector3Int.down);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardFall();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(-1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }
    }

    bool TryMoving(Vector3Int translation, bool doClearAndSet = true) // try to move in the needed translation and tell if we moved or not
    {                                                                 // the default parameter doClearAndSet is made to not clear and set the piece when checking for wallkicks so we don't mess up existing tiles
        Vector3Int newPosition = Position + translation; // calculate the position we wish to go to
       
        if(doClearAndSet) 
            Board.Clear(); // clear everything otherwise IsValid will always return false

        bool isValid = Board.IsValid(newPosition, Cells); // check if it is a valid position
        
        if (isValid) // if it is
        {
            Position = newPosition; // we update the position
            lockTime = 0; // and reset the lock time since the piece got active
            ghost.SetGhost(); // after we perform the movement we reset the ghost position
        }
        
        if(doClearAndSet) Board.Set(); // we reset the piece tiles in the new positions
        return (isValid); // and return wether we moved or not
    }

    void HardFall() // moving piece instantly to the buttom
    {
        while (TryMoving(Vector3Int.down)) // while we can move down (and actually perform a move down)
        {
            continue; // we continue trying to move down
        }
        Lock(); //if we can't keep moving down, that means we're at the buttom so we lock the piece
    }

    void Rotate(int direction) // rotate the cells depending on the direction
    {
        Board.Clear(); // first, we clear the current tiles to make sure IsValid function works correctly
        int nextRotationIndex = (RotationIndex + direction + 4) % 4; // we calculate the next rotation index and make sure it stays in [0,3]
        ApplyRotation(direction); // we apply rotation 
        if (!CheckWallkicks(nextRotationIndex)) // we check if any of the wallkicks are possible. 
        {
            ApplyRotation(-direction); // if not, we re-apply the rotation in the opposite direction
        }
        else
        {
            RotationIndex = nextRotationIndex; // else, we update our rotation index
        }
        Board.Set(); // and set the tiles back in the new positions
    }

    int WallKickIndex(int nextIndex) // calculate the index from the table in this guide "https://tetris.fandom.com/wiki/SRS" based on the current rotation index and the next one
    {
        if (RotationIndex == 0)
            if (nextIndex == 1)
                return 0;
            else
                return 7;
        else if (RotationIndex == 1)
            if (nextIndex == 0)
                return 1;
            else
                return 2;
        else if (RotationIndex == 2)
            if (nextIndex == 1)
                return 3;
            else
                return 4;
        else if (nextIndex == 2)   
            return 5;
        else
            return 6;
    }

    bool CheckWallkicks(int nextIndex) // iterate through all the wallkicks translations and stop when one of them is valid
    {
        int wallKickIndex = WallKickIndex(nextIndex); // get the right index from the guide table
        for (int i = 0; i < Data.WallKicks.GetLength(1); i++) // for each wallkick translation
        {
            if (TryMoving((Vector3Int)Data.WallKicks[wallKickIndex, i], false)) // try to move and do not perform Clear/Set functions
                return true; // if we moved, return that the walkick is valid
        }
        return false; // else all wallkicks not possible and so rotation not possible
    }

    void ApplyRotation(int direction) // apply rotation by multiplying by rotation matrix
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3 cell = Cells[i];
            int x, y;

            switch (Data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O: // for O and I pieces, the center of rotation is not on natural number we need to shift it a little bit
                    cell.x -= .5f;
                    cell.y -= .5f;
                    x = Mathf.RoundToInt((global::Data.RotationMatrix[0] * cell.x * direction) + (global::Data.RotationMatrix[1] * cell.y * direction) + .5f);
                    y = Mathf.RoundToInt((global::Data.RotationMatrix[2] * cell.x * direction) + (global::Data.RotationMatrix[3] * cell.y * direction) + .5f);
                    break;
                default:
                    x = Mathf.RoundToInt((global::Data.RotationMatrix[0] * cell.x * direction) + (global::Data.RotationMatrix[1] * cell.y * direction));
                    y = Mathf.RoundToInt((global::Data.RotationMatrix[2] * cell.x * direction) + (global::Data.RotationMatrix[3] * cell.y * direction));
                    break;
            }

            Cells[i] = new Vector3Int(x, y);
        }
    }
}
