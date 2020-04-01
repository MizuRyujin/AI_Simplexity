# Artificial inteligence Simplexity - AI 1st Project 2019/2020

Source code is in [this](https://github.com/MizuRyujin/AI_Simplexity) public repository.

## Authors

### *[João Rebelo - a21805230](https://github.com/JBernardoRebelo)*

- Implement Minimax and base heuristic.
- Idea Guy.
- Optimization.
- Report.  

### *[Miguel Fernández - a21803644](https://github.com/MizuRyujin)*

- Idea Guy.
- Implement ABNegamax.
- AI optimization.

### Search Algorithm

Our search algorithm is Negamax with Alpha-Beta pruning.
It's going through every possible play in his own perspective  
in a restrict number of turns (depth) assigning a score to both
the AI and the opponent's plays.

The score is determined by
conditions (defined in the heuristics) and it's always
maximized due to the nature of Negamax, the AI picks the
"branch" with the best score and makes a move.

Alpha-Beta pruning "cuts" the "branches" found that can't
contain the best results, so it ignores them in order to reduce
the number of actions/searches made by the AI, it does this by
keeping the best score "saved" and not accepting a "branch"
with a lower score possibly giving a worse outcome through the
AI's perspective thus not going through unnecessary play
possibilities.

### Heuristic

Our AIThinker is prepared to accept multiple heuristics, so we
can test different solutions to our problems against each other,
and so that in the future we can switch heuristics mid-game
depending on the state of the board.

Our "default" heuristic has `if` statments that define what the 
best scores will be, for example:

```cs
if (color.FriendOf(piece.Value))
    h += maxPoints - Dist(centerRow, centerCol, i, j);
```

Here we are prioratizing color and adding score to the selected 
play.

```cs
if (board.piecesInSequence == 3 &&
                        color.FriendOf(piece.Value))
    h += maxPoints - Dist(centerRow, centerCol, i, j);
```

And in this example we are adding score if it finds a  
play with 3 pieces of the same color.

## References

- Initial AI based on what Professor Nuno Fachada provided and
ABNegamax based on work done in class for Tic Tac Toe.

- AI for Games, Third Edition (2006)
