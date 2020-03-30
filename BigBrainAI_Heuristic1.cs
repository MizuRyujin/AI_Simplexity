using System;
using ColorShapeLinks.Common;

public class BigBrainAI_Heuristic1 : IBigBrainHeuristic
{
    public string Name { get => "ShapeHeuristic"; }

    public float WinScore => float.PositiveInfinity;

    public float Evaluate(Board board, PColor player)
    {
        //return Heuristic(board, player) - Heuristic(board, player.Other());
        return Heuristic(board, player);
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
        float maxPoints = Dist(centerRow, centerCol, 0, 0); // 4.61f

        // Current heuristic value
        float h = 0;

        // Loop through the board looking for pieces
        for (int i = 0; i < board.rows; i++)
        {
            for (int j = 0; j < board.cols; j++)
            {
                // Get piece in current board position
                Piece? piece = board[i, j];

                if (!piece.HasValue) continue;

                // Is there any piece there?
                if (piece.HasValue)
                {
                    // // If the piece is of our shape, increment the
                    // // heuristic inversely to the distance from the center
                    if (piece.Value.shape == color.Shape())
                    {
                        h += maxPoints - Dist(centerRow, centerCol, i, j);
                    }
                    else
                    {
                        h -= maxPoints - Dist(centerRow, centerCol, i, j);
                    }

                    if (color.FriendOf(piece.Value))
                    {
                        h += maxPoints - Dist(centerRow, centerCol, i, j);
                    }
                    else
                    {
                        h += maxPoints - Dist(centerRow, centerCol, i, j);
                    }

                    if (board.piecesInSequence == 3 &&
                        color.FriendOf(piece.Value))
                    {
                        h += maxPoints - Dist(centerRow, centerCol, i, j);
                    }
                    else
                    {
                        h -= maxPoints - Dist(centerRow, centerCol, i, j);
                    }

                    if (i > 0 && j > 0)
                    {
                        // Check if piece bellow
                        if (board[i - 1, j].HasValue && i <= 6)
                        {
                            if (!board[i - 1, j].Value.Is(color, color.Shape()))
                            {
                                h -= maxPoints - Dist(
                                    centerRow, centerCol, i, j);
                            }
                        }

                        // Check piece in collum before
                        if (board[i, j - 1].HasValue && j <= 7)
                        {
                            if (!board[i, j - 1].Value.Is(color, color.Shape()))
                            {
                                h -= maxPoints - Dist(
                                    centerRow, centerCol, i, j);
                            }
                        }

                        // Check piece in collum after
                        if (board[i, j + 1].HasValue && j <= 7)
                        {
                            if (!board[i, j + 1].Value.Is(color, color.Shape()))
                            {
                                h -= maxPoints - Dist(
                                    centerRow, centerCol, i, j);
                            }
                        }
                    }
                }
            }
        }
        // Return the final heuristic score for the given board
        return h;
    }
}
