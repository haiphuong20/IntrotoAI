using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Sprite pieceImage;
    AlphaBeta ab = new AlphaBeta();
    private bool _kingDead = false;
    float timer = 0;
    Board _board;
	void Start ()
    {
        _board = Board.Instance;
        _board.SetupBoard();
	}

	void Update ()
    {
        if (_kingDead)
        {
            SceneManager.LoadScene(0);
        }
        if (!playerTurn && timer < 1)
        {
            timer += Time.deltaTime;
        }
        else if (!playerTurn && timer >= 1)
        {
            Move move = ab.GetMove();
            _DoAIMove(move);
            timer = 0;
        }
	}

    public bool playerTurn = true;

    void _DoAIMove(Move move)
    {
        Tile firstPosition = move.firstPosition;
        Tile secondPosition = move.secondPosition;

        if (secondPosition.CurrentPiece && secondPosition.CurrentPiece.Type == Piece.pieceType.KING)
        {
            SwapPieces(move);
            _kingDead = true;
        }
        else
        {
            SwapPieces(move);
        }
    }

    public void SwapPieces(Move move)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Highlight");
        foreach (GameObject o in objects)
        {
            Destroy(o);
        }

        Tile firstTile = move.firstPosition;
        Tile secondTile = move.secondPosition;

        firstTile.CurrentPiece.MovePiece(new Vector3(-move.secondPosition.Position.x, 0, move.secondPosition.Position.y));

        if (secondTile.CurrentPiece != null)
        {
            if (secondTile.CurrentPiece.Type == Piece.pieceType.KING)
                _kingDead = true;
            Destroy(secondTile.CurrentPiece.gameObject);
        }

        // NOTE: This is extremely stupid and will definitely mess the AI up
        else
        {
            switch (firstTile.CurrentPiece.Type)
            {
                case Piece.pieceType.KING:
                    {
                        // The king only moves 2 squares when castling
                        if (Mathf.Abs(secondTile.Position.x - firstTile.Position.x) == 2)
                        {
                            Vector2 direction = Mathf.Sign(secondTile.Position.x - firstTile.Position.x) * Vector2.right;
                            Tile rook = _board.GetTileFromBoard(new Vector2(direction.x > 0 ? 7 : 0, firstTile.Position.y));

                            Move m = new Move();
                            m.firstPosition = rook;
                            m.pieceMoved = rook.CurrentPiece;
                            m.secondPosition = _board.GetTileFromBoard(secondTile.Position - direction);
                            SwapPieces(m);
                        }
                    }
                    break;

                case Piece.pieceType.PAWN:
                    {
                        // The pawn can only promote when it reaches the first or last row
                        if (secondTile.Position.y == 0 || secondTile.Position.y == 7)
                        {
                            move.pieceMoved.Type = Piece.pieceType.QUEEN;
                            move.pieceMoved.gameObject.GetComponent<SpriteRenderer>().sprite = pieceImage;
                        }
                    }
                    break;
            }
        }


        secondTile.CurrentPiece = move.pieceMoved;
        firstTile.CurrentPiece = null;
        secondTile.CurrentPiece.position = secondTile.Position;
        secondTile.CurrentPiece.HasMoved = true;

        playerTurn = !playerTurn;
    }
}
