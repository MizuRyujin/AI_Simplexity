﻿using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;

public interface IBigBrainHeuristic
{
    // Get a board evaluation from the perspective of the given player
    float Evaluate(Board board, PColor player);

    // Maximum score for a win
    float WinScore { get; }

    string Name { get; }

    float Heuristic(Board board, PColor color);
}
