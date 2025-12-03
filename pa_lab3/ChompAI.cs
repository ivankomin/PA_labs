namespace pa_lab3;
public class ChompAI
{
    private const int Infinity = 1000000;
    private Random _rnd = new Random();

    public Move GetBestMove(GameState state, Difficulty difficulty)
    {
        var possibleMoves = state.GetLegalMoves();
        if (possibleMoves.Count > 1)
            possibleMoves.RemoveAll(m => m.Row == 0 && m.Col == 0);

        if (possibleMoves.Count == 0) return new Move(0, 0);

        if (difficulty == Difficulty.Easy)
            return possibleMoves[_rnd.Next(possibleMoves.Count)];

        int maxDepth = difficulty == Difficulty.Medium ? 4 : 8; 

        Move bestMove = possibleMoves[0];
        int bestValue = -Infinity;

        foreach (var move in possibleMoves)
        {
            var nextState = state.MakeMove(move);
            
            int value = AlphaBeta(nextState, maxDepth - 1, -Infinity, Infinity, false);

            if (value > bestValue)
            {
                bestValue = value;
                bestMove = move;
            }
        }
        return bestMove;
    }

    private int AlphaBeta(GameState state, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        if (state.IsGameOver()) 
        {
            return isMaximizingPlayer ? Infinity : -Infinity;
        }
        
        if (depth == 0) return 0;

        var moves = state.GetLegalMoves();

        if (moves.Count > 1) moves.RemoveAll(m => m.Row == 0 && m.Col == 0);

        if (isMaximizingPlayer)
        {
            int maxEval = -Infinity;
            foreach (var move in moves)
            {
                var nextState = state.MakeMove(move);
                int eval = AlphaBeta(nextState, depth - 1, alpha, beta, false);
                
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha) 
                    break;
            }
            return maxEval;
        }
        else
        {
            int minEval = Infinity;
            foreach (var move in moves)
            {
                var nextState = state.MakeMove(move);
                int eval = AlphaBeta(nextState, depth - 1, alpha, beta, true);
                
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                if (beta <= alpha) 
                    break;
            }
            return minEval;
        }
    }
}