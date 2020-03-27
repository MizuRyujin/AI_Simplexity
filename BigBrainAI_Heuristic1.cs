using System;
using ColorShapeLinks.Common;

public class BigBrainHeuristic1 : IBigBrainHeuristic
{
    public string Name { get => "ShapeHeuristic"; }
    public float WinScore => float.PositiveInfinity;

    public float Evaluate(Board board, PColor player)
    {
        return Heuristic(board, player) - Heuristic(board, player.Other());
    }

    // Heuristic function
    public float Heuristic(Board board, PColor color)
    {
        // Distance between two points
        float Dist(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(
                Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        // Determine the center row
        float centerRow = board.rows / 2;
        float centerCol = board.cols / 2;

        // Maximum points a piece can be awarded when it's at the center
        float maxPoints = Dist(centerRow, centerCol, 0, 0);

        // Current heuristic value
        float h = 0;

        // Loop through the board looking for pieces
        for (int i = 0; i < board.rows; i++)
        {
            for (int j = 0; j < board.cols; j++)
            {
                // Get piece in current board position
                Piece? piece = board[i, j];

                // Is there any piece there?
                if (piece.HasValue)
                {
                    // If the piece is of our shape, increment the
                    // heuristic inversely to the distance from the center
                    if (piece.Value.shape == color.Shape())
                    {
                        h += maxPoints - Dist(centerRow, centerCol, i, j);
                        break;
                    }

                    // Otherwise decrement the heuristic value using the
                    // same criteria
                    else
                    {
                        h -= maxPoints - Dist(centerRow, centerCol, i, j);
                    }

                    // If the piece is of our color, increment the
                    // heuristic inversely to the distance from the center
                    if (piece.Value.color == color)
                    {
                        h += maxPoints - Dist(centerRow, centerCol, i, j);
                        break;
                    }
                    // Otherwise decrement the heuristic value using the
                    // same criteria
                    else
                    {
                        h -= maxPoints - Dist(centerRow, centerCol, i, j);
                    }
                }
            }
        }
        // Return the final heuristic score for the given board
        return h;
    }
}
