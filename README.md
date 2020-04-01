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
It's going through every possible play in his own perpective in 
a restrict number of turns (depth) assigning a score to both 
the AI and the opponent's plays. The score is determined by 
conditions (defined in the heuristics) and it's always
maximized due to the nature of Negamax, the AI picks the 
"branch" with the best score and makes a move.
Alpha-Beta pruning "cuts" the "branches" found that can't 
contain the best results so it ignores them in order to reduce 
number of actions/searches made by the AI.

### Heuristic

Our AIThinker is prepared to accept multiple heuristics, so we
can test different solutions to our problems against each other,
and so that in the future we can switch heuristics mid-game
depending on the state of the board.

Our "default" heuristic (...)


## References

- Initial AI based on what Professor Nuno Fachada provided and
ABNegamax based on work done in class for Tick Tack Toe.

- AI for Games, Third Edition (2006)