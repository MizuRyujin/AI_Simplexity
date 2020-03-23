using System;
using System.Threading;
using System.Collections.Generic;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;

/// <summary>
/// BigBrain thinker class. Uses NegaMAx method
/// </summary>
public class BigBrainAIThinker : AbstractThinker
{
    // AI Variables
    /// <summary>
    /// Max depth for thinking plays
    /// </summary>
    private byte maxDepth = default;
    /// <summary>
    /// Number of board states evaluated
    /// </summary>
    private int numEvals = default;
    /// <summary>
    /// Value used for maximisation
    /// </summary>
    private byte alpha = default;
    /// <summary>
    /// Value used for minimisation
    /// </summary>
    private byte beta = default;
    /// <summary>
    /// Heuristic for AI behaviour
    /// </summary>
    private IBigBrainHeuristic selectedHeuristic = default;

    private ICollection<IBigBrainHeuristic> availableHeuristics = default;

    /// <summary>
    /// Method to setup values and heuristics
    /// </summary>
    /// <param name="str"></param>
    public override void Setup(string str)
    {
        if (!byte.TryParse(str, out maxDepth))
        {
            maxDepth = 2;
        }

        SetupHeuristic(str);
    }

    /// <summary>
    /// Assigns wanted Heuristic to be used 
    /// </summary>
    /// <param name="str"> Accepts param with name of wanted heuristic </param>
    private void SetupHeuristic(string str)
    {
        availableHeuristics = new List<IBigBrainHeuristic>();

        availableHeuristics.Add(new BigBrainHeuristic1());

        foreach (IBigBrainHeuristic heuristic in availableHeuristics)
        {
            if (str.Contains(heuristic.Name))
            {
                selectedHeuristic = heuristic;
            }
        }

        // Assigns default heuristic
        if (selectedHeuristic == default)
        {
            selectedHeuristic = new BigBrainHeuristic1();
        }
    }

    // The ToString() method, optional override
    public override string ToString()
    {
        return base.ToString() + "D" + maxDepth;
    }

    // The Think() method (mandatory override) is invoked by the game engine
    public override FutureMove Think(Board board, CancellationToken ct)
    {
        numEvals = 0;

        // Invoke minimax, starting with zero depth
        // (FutureMove move, float score) decision =
        //     Minimax(board, ct, board.Turn, board.Turn, 0);

        (FutureMove move, float score) decision = ABNegaMAx(
            board, ct, board.Turn, board.Turn, 0, byte.MinValue, byte.MaxValue);

        OnThinkingInfo($"Heuristic: {selectedHeuristic.Name} Depth: {maxDepth} "
        + $"numEvals: {numEvals}");

        // Return best move
        return decision.move;
    }

    #region Minimax
    // Minimax implementation
    private (FutureMove move, float score) Minimax(
        Board board, CancellationToken ct,
        PColor player, PColor turn, int depth)
    {
        numEvals++;

        // Move to return and its heuristic value
        (FutureMove move, float score) selectedMove;

        // Current board state
        Winner winner;

        // If a cancellation request was made...
        if (ct.IsCancellationRequested)
        {
            // ...set a "no move" and skip the remaining part of the algorithm
            selectedMove = (FutureMove.NoMove, float.NaN);
        }
        // Otherwise, if it's a final board, return the appropriate evaluation
        else if ((winner = board.CheckWinner()) != Winner.None)
        {
            if (winner.ToPColor() == player)
            {
                // AI player wins, return highest possible score
                selectedMove = (FutureMove.NoMove, float.PositiveInfinity);
            }
            else if (winner.ToPColor() == player.Other())
            {
                // Opponent wins, return lowest possible score
                selectedMove = (FutureMove.NoMove, float.NegativeInfinity);
            }
            else
            {
                // A draw, return zero
                selectedMove = (FutureMove.NoMove, 0f);
            }
        }
        // If we're at maximum depth and don't have a final board, use
        // the heuristic
        else if (depth == maxDepth)
        {
            selectedMove = (FutureMove.NoMove,
                selectedHeuristic.Heuristic(board, player));
        }
        else // Board not final and depth not at max...
        {
            //...so let's test all possible moves and recursively call Minimax()
            // for each one of them, maximizing or minimizing depending on who's
            // turn it is

            // Initialize the selected move...
            selectedMove = turn == player
                // ...with negative infinity if it's the AI's turn and we're
                // maximizing (so anything except defeat will be better than this)
                ? (FutureMove.NoMove, float.NegativeInfinity)
                // ...or with positive infinity if it's the opponent's turn and we're
                // minimizing (so anything except victory will be worse than this)
                : (FutureMove.NoMove, float.PositiveInfinity);

            // Test each column
            for (int i = 0; i < Cols; i++)
            {
                // Skip full columns
                if (board.IsColumnFull(i)) continue;

                // Test shapes
                for (int j = 0; j < 2; j++)
                {
                    // Get current shape
                    PShape shape = (PShape)j;

                    // Use this variable to keep the current board's score
                    float eval;

                    // Skip unavailable shapes
                    if (board.PieceCount(turn, shape) == 0) continue;

                    // Test move, call minimax and undo move
                    board.DoMove(shape, i);
                    eval = Minimax(board, ct, player, turn.Other(), depth + 1).score;
                    board.UndoMove();

                    // If we're maximizing, is this the best move so far?
                    if (turn == player
                        && eval >= selectedMove.score)
                    {
                        // If so, keep it
                        selectedMove = (new FutureMove(i, shape), eval);
                    }
                    // Otherwise, if we're minimizing, is this the worst move so far?
                    else if (turn == player.Other()
                        && eval <= selectedMove.score)
                    {
                        // If so, keep it
                        selectedMove = (new FutureMove(i, shape), eval);
                    }
                }
            }
        }

        // Return movement and its heuristic value
        return selectedMove;
    }
    #endregion
    
    
    // Implementation of ABNegamax
    private (FutureMove move, float score) ABNegaMAx(
        Board board, CancellationToken ct,
        PColor player, PColor turn, int depth, byte alpha, byte beta)
    {
        numEvals++;
        
        // Move to return and its heuristic value
        (FutureMove move, float score) selectedMove;

        // Current board state
        Winner winner;

        // If a cancellation request was made...
        if (ct.IsCancellationRequested)
        {
            // ...set a "no move" and skip the remaining part of the algorithm
            selectedMove = (FutureMove.NoMove, float.NaN);
        }
        // Otherwise, if it's a final board, return the appropriate evaluation
        else if ((winner = board.CheckWinner()) != Winner.None)
        {
            if (winner.ToPColor() == player)
            {
                // AI player wins, return highest possible score
                selectedMove = (FutureMove.NoMove, float.PositiveInfinity);
            }
            else if (winner.ToPColor() == player.Other())
            {
                // Opponent wins, return lowest possible score
                selectedMove = (FutureMove.NoMove, float.NegativeInfinity);
            }
            else
            {
                // A draw, return zero
                selectedMove = (FutureMove.NoMove, 0f);
            }
        }
        // If we're at maximum depth and don't have a final board, use
        // the heuristic
        else if (depth == maxDepth)
        {
            selectedMove = (FutureMove.NoMove,
                selectedHeuristic.Heuristic(board, player));
        }
        else // Board not final and depth not at max...
        {
            //...so let's test all possible moves and recursively call ABNegamax()
            // for each one of them, maximizing depending on who's
            // turn it is

            // Initialize the selected move...
            selectedMove = turn == player
                // ...with negative infinity if it's the AI's turn and we're
                // maximizing (so anything except defeat will be better than this)
                ? (FutureMove.NoMove, float.NegativeInfinity)
                // ...or with positive infinity if it's the opponent's turn and we're
                // minimizing (so anything except victory will be worse than this)
                : (FutureMove.NoMove, float.PositiveInfinity);

            // Test each column
            for (int i = 0; i < Cols; i++)
            {
                // Skip full columns
                if (board.IsColumnFull(i)) continue;

                // Test shapes
                for (int j = 0; j < 2; j++)
                {
                    // Get current shape
                    PShape shape = (PShape)j;

                    // Use this variable to keep the current board's score
                    float eval;

                    // Skip unavailable shapes
                    if (board.PieceCount(turn, shape) == 0) continue;

                    // Test move, call minimax and undo move
                    board.DoMove(shape, i);

                    eval =- ABNegaMAx(
                        board, ct, player, turn.Other(), 
                        depth + 1, (byte)-alpha, (byte)-beta).score;

                    board.UndoMove();

                    // If we're maximizing, is this the best move so far?
                    if (turn == player
                        && eval >= selectedMove.score)
                    {
                        alpha = (byte) eval;

                        // If so, keep it
                        selectedMove = (new FutureMove(i, shape), eval);

                        if (alpha >= beta)
                        {
                            return selectedMove;
                        }
                    }
                }
            }
        }

        return selectedMove;
    }
}
