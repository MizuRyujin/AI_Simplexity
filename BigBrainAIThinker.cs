using System;
using System.Threading;
using System.Diagnostics;
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
    /// Selected heuristic for AI behaviour
    /// </summary>
    private IBigBrainHeuristic selectedHeuristic = default;

    /// <summary>
    /// Collection of heuristics
    /// </summary>
    private ICollection<IBigBrainHeuristic> availableHeuristics = default;

    // Create a dictionary or table
    private SortedDictionary<float, FutureMove> selectedMoves = default;

    private Stopwatch timer = default;

    private byte dictResetCounter = default;

    /// <summary>
    /// Method to setup values and heuristics
    /// </summary>
    /// <param name="str"> String to be splitted and used to check for 
    /// AI parameters such as heuristic and max depth value. </param>
    public override void Setup(string str)
    {
        string[] strs = str.Split(',');

        // Instantiate collection of heuristics
        availableHeuristics = new List<IBigBrainHeuristic>();

        // Add heuristics to collection
        availableHeuristics.Add(new BigBrainAI_Heuristic1());

        // Accepts a chopped up sentence to find depth and heuristics
        for (int i = 0; i < strs.Length; i++)
        {
            byte.TryParse(strs[i], out maxDepth);

            foreach (IBigBrainHeuristic heuristic in availableHeuristics)
            {
                if (strs[i].Contains(heuristic.Name))
                {
                    selectedHeuristic = heuristic;
                }
            }
        }

        // Assigns default heuristic, in case the string 
        if (selectedHeuristic == null)
        {
            selectedHeuristic = new BigBrainAI_Heuristic1();
        }

        // Assigns maxDepth
        if (maxDepth == default)
        {
            maxDepth = 2;
        }

        selectedMoves = new SortedDictionary<float, FutureMove>();

        timer = new Stopwatch();
    }

    /// <summary>
    /// The ToString() method, optional override
    /// </summary>
    /// <returns> AI name plus it's max depth for evaluating possible moves.
    /// </returns>
    public override string ToString()
    {
        return base.ToString() + "D" + maxDepth;
    }

    // 
    /// <summary>
    /// The Think() method (mandatory override) is invoked by the game engine
    /// </summary>
    /// <param name="board"> Game board. </param>
    /// <param name="ct">Cancelation token. </param>
    /// <returns></returns>
    public override FutureMove Think(Board board, CancellationToken ct)
    {
        // Reset number of evaluation on each Think() call, used for debug
        numEvals = 0;

        // SortedSet<bestMoves>.Clear();
        // if (dictResetCounter >= 3)
        // {
        //     selectedMoves.Clear();
        // }

        timer.Restart();

        // Call ABNegamax() (Change to ABNegaScout in case dictionary works!)
        (FutureMove move, float score) decision = ABNegaScout(
            board, ct, 0, float.NegativeInfinity, float.PositiveInfinity);

        // Debug information
        OnThinkingInfo(
            $"Max Depth: {maxDepth} " +
            $"DictCount: {selectedMoves.Count} " +
            $"numEvals: {numEvals}");

        dictResetCounter++;

        // Return best move
        return decision.move;
    }

    /// <summary>
    /// Negamax with alpha and beta pruning method
    /// </summary>
    /// <param name="board"> Board state at the time of method call. </param>
    /// <param name="ct"> Cancelation Token needed to check if think
    /// time is up. </param>
    /// <param name="depth"> Current search depth. </param>
    /// <param name="alpha"> Alpha value to be used in pruning. </param>
    /// <param name="beta"> Beta value to be used in pruning. </param>
    /// <returns> A desired move and a score for that movement. </returns>
    private (FutureMove move, float score) ABNegaMax(
        Board board, CancellationToken ct, int depth, float alpha, float beta)
    {
        // TODO: TRY ACTUALLY IMPLEMENT A MAX FUNCTION BETWEEN ALPHA AND A
        // BEST SCORE VARIABLE
        numEvals++;

        (FutureMove move, float score) bestMove;

        float bestScore = float.NegativeInfinity;

        Winner winner;

        // To check if we're done recursing...
        // ... first we check if thinking time is up...
        if (ct.IsCancellationRequested)
        {
            bestMove = (FutureMove.NoMove, float.NaN);
        }

        // ... if not, we check for winning board states...
        else if ((winner = board.CheckWinner()) != Winner.None)
        {
            // ... if it was a draw...
            if ((winner = board.CheckWinner()) == Winner.Draw)
            {
                bestMove = (FutureMove.NoMove, 0f);
            }

            // ... if the AI won...
            if (winner.ToPColor() == board.Turn)
            {
                bestMove = (FutureMove.NoMove, selectedHeuristic.WinScore);
            }

            // ... if it was the opponent winning...
            else
            {
                bestMove = (FutureMove.NoMove, -selectedHeuristic.WinScore);
            }
        }

        // ... lastly we check if we reached max thinking depth
        else if (depth == maxDepth)
        {
            bestMove = (FutureMove.NoMove,
               selectedHeuristic.Evaluate(board, board.Turn));
        }

        // If not done recursing, bubble up values from below
        else
        {
            // Initialize selected move with worst option
            bestMove = (FutureMove.NoMove, bestScore);

            // Go through each move
            for (int i = 0; i < Cols; i++)
            {
                if (board.IsColumnFull(i)) continue;

                for (int j = 0; j < 2; j++)
                {
                    PShape shape = (PShape)j;

                    float curScore;

                    if (board.PieceCount(board.Turn, shape) == 0) continue;

                    board.DoMove(shape, i);

                    // Recurse
                    curScore = -ABNegaMax(
                        board, ct,
                        depth + 1, -beta, -Math.Max(alpha, bestScore)).score;

                    board.UndoMove();

                    // Update the best score (alpha value)

                    if (curScore > bestMove.score)
                    {
                        bestScore = curScore;

                        // If so, keep it
                        bestMove = (new FutureMove(i, shape), curScore);

                        if (!selectedMoves.ContainsKey(curScore))
                        {
                            selectedMoves.Add(
                                curScore, new FutureMove(i, shape));
                        }

                        // If we're outside the bounds, 
                        // prune by exiting immediately
                        if (bestScore >= beta)
                        {
                            return bestMove;
                        }
                    }

                    else if (selectedMoves.ContainsKey(bestScore))
                    {
                        bestMove = (selectedMoves[bestScore], bestScore);
                    }
                }
            }
        }
        return bestMove;
    }


    private (FutureMove move, float score) ABNegaScout(
        Board board, CancellationToken ct, int depth, float alpha, float beta)
    {
        // TODO: TRY ACTUALLY IMPLEMENT A MAX FUNCTION BETWEEN ALPHA AND A
        // BEST SCORE VARIABLE
        numEvals++;

        (FutureMove move, float score) bestMove;

        float bestScore = float.NegativeInfinity;

        Winner winner;

        // To check if we're done recursing...
        // ... first we check if thinking time is up...
        if (ct.IsCancellationRequested)
        {
            bestMove = (FutureMove.NoMove, float.NaN);
        }

        // ... if not, we check for winning board states...
        else if ((winner = board.CheckWinner()) != Winner.None)
        {
            // ... if it was a draw...
            if ((winner = board.CheckWinner()) == Winner.Draw)
            {
                bestMove = (FutureMove.NoMove, 0f);
            }

            // ... if the AI won...
            if (winner.ToPColor() == board.Turn)
            {
                bestMove = (FutureMove.NoMove, selectedHeuristic.WinScore);
            }

            // ... if it was the opponent winning...
            else
            {
                bestMove = (FutureMove.NoMove, -selectedHeuristic.WinScore);
            }
        }

        // ... lastly we check if we reached max thinking depth
        else if (depth == maxDepth)
        {
            bestMove = (FutureMove.NoMove, selectedHeuristic.Evaluate(
                board, board.Turn));
        }

        // If not done recursing, bubble up values from below
        else
        {
            // Initialize best move with worst option
            bestMove = (FutureMove.NoMove, bestScore);

            // Keep track of the Test window value
            float adaptiveBeta = beta;

            // Go through each move
            for (int i = 0; i < Cols; i++)
            {
                if (board.IsColumnFull(i)) continue;

                for (int j = 0; j < 2; j++)
                {
                    PShape shape = (PShape)j;

                    float curScore;

                    if (board.PieceCount(board.Turn, shape) == 0) continue;

                    board.DoMove(shape, i);

                    // Recurse
                    curScore = -ABNegaMax(
                        board, ct,
                        depth + 1, -adaptiveBeta,
                        -Math.Max(alpha, bestScore)).score;

                    board.UndoMove();

                    // Update the best score (alpha value).

                    if (curScore > bestMove.score)
                    {
                        if (adaptiveBeta == beta || depth >= maxDepth - 2)
                        {
                            bestScore = curScore;
                            // If so, keep it
                            bestMove = (new FutureMove(i, shape), curScore);

                            if (!selectedMoves.ContainsKey(curScore))
                            {
                                selectedMoves.Add(
                                    bestScore, new FutureMove(i, shape));
                            }
                        }
                        else
                        {
                            float negaCurScore = ABNegaScout(
                                board, ct, depth, -beta,
                                -bestScore).score;

                            bestScore = -negaCurScore;
                        }

                        // If we're outside the bounds, 
                        // prune by exiting immediately
                        if (bestScore >= beta)
                        {
                            return bestMove;
                        }

                        adaptiveBeta = Math.Max(alpha, bestScore) + 1;

                    }

                    else if (selectedMoves.ContainsKey(bestScore))
                    {
                        bestMove = (selectedMoves[bestScore], bestScore);
                    }
                }
            }
        }
        return bestMove;
    }
}
