using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;

public interface IHeuristic
{
    // Get a board evaluation from the perspective of the given player
    float Evaluate(Board board);

    // Maximum score for a win
    float WinScore { get; }
}
