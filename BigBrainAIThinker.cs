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

        availableHeuristics.Add(new BigBrainAI_Heuristic1());

        foreach (IBigBrainHeuristic heuristic in availableHeuristics)
        {
            if (str.Contains(heuristic.Name))
            {
                selectedHeuristic = heuristic;
            }
        }

        // Assigns default heuristic
        if (selectedHeuristic == null)
        {
            selectedHeuristic = new BigBrainAI_Heuristic1();
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

        // Invoke ABNegamax, starting with zero depth
        (FutureMove move, float score) decision = ABNegaMAx(
            board, ct, 0, float.NegativeInfinity, float.PositiveInfinity);

        OnThinkingInfo(
            $"Heuristic: {selectedHeuristic.Name}" +
            $" Color: {board.Turn} " +
            $"numEvals: {numEvals}");

        // Return best move
        return decision.move;
    }

    // Implementation of ABNegamax
    private (FutureMove move, float score) ABNegaMAx(
        Board board, CancellationToken ct, int depth, float alpha, float beta)
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
            if ((winner = board.CheckWinner()) == Winner.Draw)
            {
                selectedMove = (FutureMove.NoMove, 0f);
            }
            if (winner.ToPColor() == board.Turn)
            {
                selectedMove = (FutureMove.NoMove, selectedHeuristic.WinScore);
            }
            else
            {
                selectedMove = (FutureMove.NoMove, -selectedHeuristic.WinScore);
            }
        }
        // If we're at maximum depth and don't have a final board, use
        // the heuristic
        else if (depth == maxDepth)
        {
            selectedMove = (FutureMove.NoMove,
                selectedHeuristic.Evaluate(board, board.Turn));
        }
        else // Board not final and depth not at max...
        {
            // Initialize the selected move...
            selectedMove = (FutureMove.NoMove, float.NegativeInfinity);

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
                    if (board.PieceCount(board.Turn, shape) == 0) continue;

                    // Test move, call minimax and undo move
                    board.DoMove(shape, i);

                    eval = -ABNegaMAx(
                        board, ct,
                        depth + 1, -beta, -alpha).score;

                    board.UndoMove();

                    // If we're maximizing, is this the best move so far?
                    if (eval > selectedMove.score)
                    {
                        alpha = eval;

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
