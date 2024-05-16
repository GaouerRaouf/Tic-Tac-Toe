using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState { none, cross, circle }

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
    int _aiLevel;


    [SerializeField]
    private bool _isPlayerTurn;

    [SerializeField]
    private int _gridSize = 3;

    [SerializeField]
    private TicTacToeState playerState = TicTacToeState.cross;
    [SerializeField]
    private TicTacToeState aiState = TicTacToeState.circle;

    [SerializeField]
    private GameObject _xPrefab;

    [SerializeField]
    private GameObject _oPrefab;

    public UnityEvent onGameStarted;

    private bool _gameEnded = false;

    // Call This event with the player number to denote the winner
    public WinnerEvent onPlayerWin;

    ClickTrigger[,] _triggers;

    private void Awake()
    {
        if (onPlayerWin == null)
        {
            onPlayerWin = new WinnerEvent();
        }
    }

    public void StartAI(int AILevel)
    {
        _aiLevel = AILevel;
        StartGame();
    }

    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
    {
        _triggers[myCoordX, myCoordY] = clickTrigger;
    }

    private void StartGame()
    {
        _triggers = new ClickTrigger[3, 3];

        // Initialize _triggers before invoking onGameStarted
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _triggers[i, j] = null;
            }
        }

        onGameStarted.Invoke();

        if (_aiLevel != 0)
        {
            StartCoroutine(AITurn());
        }
    }

    

    IEnumerator AITurn()
    {
        _isPlayerTurn = false;  // Indicate AI's turn

        // Call Minimax to get the best move
        int[,] result = Minimax(_triggers, true);  // AI is the maximizing player
        int bestRow = result[0, 1];
        int bestCol = result[0, 2];

        // Check if the corresponding ClickTrigger can be clicked
        if (_triggers[bestRow, bestCol].canClick)
        {
            // Make the move on the board state
            _triggers[bestRow, bestCol]._state = aiState;

            // Visualize the AI's move
            AiSelects(bestRow, bestCol);

            // Check for win or draw
            if (CheckForWinner(_triggers))
            {
                // Handle game end
                Debug.Log("AI wins!");
                _gameEnded = true;
            }
            else if (IsBoardFull())
            {
                // Handle draw
                Debug.Log("It's a draw!");
                _gameEnded = true;
            }
            else
            {
                _isPlayerTurn = true;  // Switch back to player's turn
            }
        }
        else
        {
            // Retry AI's turn if the selected ClickTrigger is not available
            yield return StartCoroutine(AITurn());
        }

        yield return null;
    }



    public void PlayerSelects(int coordX, int coordY)
    {
        SetVisual(coordX, coordY, playerState);
        _triggers[coordX, coordY]._state = playerState;
        // Check for win or draw after player's move
        if (CheckForWinner(_triggers))
        {
            // Handle game end
            Debug.Log("Player wins!");
            _gameEnded = true;
        }
        else if (IsBoardFull())
        {
            // Handle draw
            Debug.Log("It's a draw!");
            _gameEnded = true;
        }
        else
        {
            // Start AI's turn after player's move
            StartCoroutine(AITurn());
        }
    }


    public void AiSelects(int coordX, int coordY)
    {
        SetVisual(coordX, coordY, aiState);
        _triggers[coordX, coordY]._state = aiState;
    }

    private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
    {
        Instantiate(
            targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
            _triggers[coordX, coordY].transform.position,
            Quaternion.identity
        );
        _triggers[coordX, coordY].canClick = false;
    }


    int[,] Minimax(ClickTrigger[,] board, bool isMaximizingPlayer)
    {
        // Check for game-ending states
        if (CheckForWin(playerState)) return new int[,] { { 10, -1, -1 } };  // Win for maximizing player
        if (CheckForWin(aiState)) return new int[,] { { -10, -1, -1 } };      // Loss for maximizing player
        if (IsBoardFull()) return new int[,] { { 0, -1, -1 } };                 // Draw

        // Recursively explore possible moves
        int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;
        int bestMoveRow = -1, bestMoveCol = -1;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (board[row, col]._state == TicTacToeState.none && _triggers[row,col].canClick) // Empty cell
                {
                    // Make the move
                    board[row, col]._state = isMaximizingPlayer ? playerState : aiState;

                    // Recursive call for the opponent
                    int[,] result = Minimax(board, !isMaximizingPlayer);
                    int score = result[0, 0];

                    // Undo the move
                    board[row, col]._state = TicTacToeState.none;

                    // Update best move if needed
                    if (isMaximizingPlayer && score > bestScore)
                    {
                        bestScore = score;
                        bestMoveRow = row;
                        bestMoveCol = col;
                    }
                    else if (!isMaximizingPlayer && score < bestScore)
                    {
                        bestScore = score;
                        bestMoveRow = row;
                        bestMoveCol = col;
                    }
                }
            }
        }

        return new int[,] { { bestScore, bestMoveRow, bestMoveCol } };
    }


    public bool CheckForWin(TicTacToeState state)
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (_triggers[i, 0]._state == state && _triggers[i, 1]._state == state && _triggers[i, 2]._state == state)
            {
                return true;
            }
        }

        // Check columns
        for (int i = 0; i < 3; i++)
        {
            if (_triggers[0, i]._state == state && _triggers[1, i]._state == state && _triggers[2, i]._state == state)
            {
                return true;
            }
        }

        // Check diagonals
        if (_triggers[0, 0]._state == state && _triggers[1, 1]._state == state && _triggers[2, 2]._state == state)
        {
            return true;
        }

        if (_triggers[0, 2]._state == state && _triggers[1, 1]._state == state && _triggers[2, 0]._state == state)
        {
            return true;
        }

        return false;
    }



    bool CheckForWinner(ClickTrigger[,] board)
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0]._state != TicTacToeState.none && board[i, 0]._state == board[i, 1]._state && board[i, 1]._state == board[i, 2]._state)
            {
                onPlayerWin.Invoke(board[i, 0]._state == playerState ? 2 : 1);
                _gameEnded = true;  // Set game as ended
                return true;
            }
        }

        // Check columns
        for (int i = 0; i < 3; i++)
        {
            if (board[0, i]._state != TicTacToeState.none && board[0, i]._state == board[1, i]._state && board[1, i]._state == board[2, i]._state)
            {
                onPlayerWin.Invoke(board[0, i]._state == playerState ? 2 : 1);
                _gameEnded = true;  // Set game as ended
                return true;
            }

        }

        // Check diagonals
        if (board[0, 0]._state != TicTacToeState.none && board[0, 0]._state == board[1, 1]._state && board[1, 1]._state == board[2, 2]._state)
        {
            onPlayerWin.Invoke(board[0, 0]._state == playerState ? 2 : 1);
            _gameEnded = true;  // Set game as ended
            return true;
        }

        if (board[0, 2]._state != TicTacToeState.none && board[0, 2]._state == board[1, 1]._state && board[1, 1]._state == board[2, 0]._state)
        {
            onPlayerWin.Invoke(board[0, 2]._state == playerState ? 2 : 1);
            _gameEnded = true;  // Set game as ended
            return true;
        }

        _gameEnded = IsBoardFull();  // Check if the board is full (draw)

        return false;  // No winner yet
    }


    bool IsBoardFull()
    {
        // Check if the board is full (no empty cells)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (_triggers[i, j].canClick)
                {
                    return false;  // Found an empty cell
                }
            }
        }
        onPlayerWin.Invoke(-1);
        return true;  // Board is full
    }
}
