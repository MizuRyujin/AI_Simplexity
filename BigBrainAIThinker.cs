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
    /// Selected heuristic for AI behaviour
    /// </summary>
    private IBigBrainHeuristic selectedHeuristic = default;

    /// <summary>
    /// Collection of heuristics
    /// </summary>
    private ICollection<IBigBrainHeuristic> availableHeuristics = default;

    /// <summary>
    /// Method to setup values and heuristics
    /// </summary>
    /// <param name="str"> String to be splitted and used to check for 
    /// AI parameters such as heuristic and max depth value. </param>
    public override void Setup(string str)
    {
        // TODO: INSERT STRING SPLIT() HERE
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

        // Assigns default heuristic, in case the string 
        if (selectedHeuristic == null)
        {
            selectedHeuristic = new BigBrainAI_Heuristic1();
        }
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

        // Call ABNegamax()
        (FutureMove move, float score) decision = ABNegaMAx(
            board, ct, 0, float.NegativeInfinity, float.PositiveInfinity);

        // Debug information
        OnThinkingInfo(
            $"Heuristic: {selectedHeuristic.Name}" +
            $" Color: {board.Turn} " +
            $"numEvals: {numEvals}");

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
    private (FutureMove move, float score) ABNegaMAx(
        Board board, CancellationToken ct, int depth, float alpha, float beta)
    {
        numEvals++;
        
        (FutureMove move, float score) selectedMove;

        Winner winner;

        if (ct.IsCancellationRequested)
        {
            selectedMove = (FutureMove.NoMove, float.NaN);
        }

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

        else if (depth == maxDepth)
        {
            selectedMove = (FutureMove.NoMove,
                selectedHeuristic.Evaluate(board, board.Turn));
        }
        else
        {
            selectedMove = (FutureMove.NoMove, float.NegativeInfinity);

            for (int i = 0; i < Cols; i++)
            {
                if (board.IsColumnFull(i)) continue;

                for (int j = 0; j < 2; j++)
                {
                    PShape shape = (PShape)j;

                    float eval;

                    if (board.PieceCount(board.Turn, shape) == 0) continue;

                    board.DoMove(shape, i);

                    eval = -ABNegaMAx(
                        board, ct,
                        depth + 1, -beta, -alpha).score;

                    board.UndoMove();

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
