using System;
using System.Threading;
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
    private IHeuristic selectedHeuristic = default;

    /// <summary>
    /// Method to setup values and heuristics
    /// </summary>
    /// <param name="str"></param>
    public override void Setup(string str)
    {
        alpha = byte.MinValue;
        beta = byte.MaxValue;
        // TODO: INSERT HEURISTIC
        if (!byte.TryParse(str, out maxDepth))
        {
            maxDepth = 2;
        }
    }

    public override FutureMove Think(Board board, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    private (PShape shape, int col) ABNegaMAx(CancellationToken ct,
            PShape pShape, byte depth)
    {
        numEvals++;

        if (ct.IsCancellationRequested)
        {
            return (pShape, 0);
        }


    }
}
