namespace pa_lab3;
public class ChompAI
{
<<<<<<< HEAD
=======
    // Значення "нескінченності" для початкових меж альфа і бета
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
    private const int Infinity = 1000000;
    private Random _rnd = new Random();

    public Move GetBestMove(GameState state, Difficulty difficulty)
    {
        var possibleMoves = state.GetLegalMoves();
<<<<<<< HEAD
=======
        
        // Евристика: ніколи не ходити в (0,0), якщо є інші варіанти (це миттєвий програш)
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
        if (possibleMoves.Count > 1)
            possibleMoves.RemoveAll(m => m.Row == 0 && m.Col == 0);

        if (possibleMoves.Count == 0) return new Move(0, 0);

<<<<<<< HEAD
        if (difficulty == Difficulty.Easy)
            return possibleMoves[_rnd.Next(possibleMoves.Count)];

        int maxDepth = difficulty == Difficulty.Medium ? 4 : 8; 
=======
        // Легкий рівень: просто випадковий хід (алгоритм не використовується)
        if (difficulty == Difficulty.Easy)
            return possibleMoves[_rnd.Next(possibleMoves.Count)];

        // Вибір глибини:
        // Medium (2) - прораховує на 2 ходи вперед (хід гравця + відповідь AI)
        // Hard (8) - глибокий пошук
        int maxDepth = difficulty == Difficulty.Medium ? 2 : 8; 
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8

        Move bestMove = possibleMoves[0];
        int bestValue = -Infinity;

<<<<<<< HEAD
=======
        // Перебір всіх можливих ходів на першому рівні
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
        foreach (var move in possibleMoves)
        {
            var nextState = state.MakeMove(move);
            
<<<<<<< HEAD
=======
            // Викликаємо алгоритм Alpha-Beta
            // Ми зробили хід, тепер черга мінімізуючого гравця (User), тому передаємо false
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
            int value = AlphaBeta(nextState, maxDepth - 1, -Infinity, Infinity, false);

            if (value > bestValue)
            {
                bestValue = value;
                bestMove = move;
            }
        }
        return bestMove;
    }

<<<<<<< HEAD
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
=======
    /// <summary>
    /// Алгоритм Альфа-Бета відсікань (Alpha-Beta Pruning)
    /// </summary>
    private int AlphaBeta(GameState state, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        // 1. Термінальні стани (кінець гри або досягнуто ліміт глибини)
        if (state.IsGameOver()) 
        {
            // Якщо зараз хід Максимізатора (AI), значить попередній (Гравець) з'їв отруту -> AI виграв (+Infinity)
            // Якщо зараз хід Мінімізатора (Гравця), значить попередній (AI) з'їв отруту -> AI програв (-Infinity)
            return isMaximizingPlayer ? Infinity : -Infinity;
        }
        
        if (depth == 0) return 0; // Евристична оцінка (для цієї гри нейтральна 0, бо головне - перемога/поразка)

        var moves = state.GetLegalMoves();
        // Оптимізація: не розглядаємо хід у (0,0) як валідний варіант стратегії, якщо є вибір
        if (moves.Count > 1) moves.RemoveAll(m => m.Row == 0 && m.Col == 0);

        if (isMaximizingPlayer) // Хід AI (намагається максимізувати оцінку)
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
        {
            int maxEval = -Infinity;
            foreach (var move in moves)
            {
                var nextState = state.MakeMove(move);
                int eval = AlphaBeta(nextState, depth - 1, alpha, beta, false);
                
                maxEval = Math.Max(maxEval, eval);
<<<<<<< HEAD
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha) 
                    break;
            }
            return maxEval;
        }
        else
=======
                alpha = Math.Max(alpha, eval); // Оновлюємо нижню межу (Alpha)

                // --- ВІДСІКАННЯ ---
                if (beta <= alpha) 
                    break; // Beta-відсікання: Мінімізатор не дозволить досягти цього стану
            }
            return maxEval;
        }
        else // Хід Гравця (намагається мінімізувати оцінку для AI)
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
        {
            int minEval = Infinity;
            foreach (var move in moves)
            {
                var nextState = state.MakeMove(move);
                int eval = AlphaBeta(nextState, depth - 1, alpha, beta, true);
                
                minEval = Math.Min(minEval, eval);
<<<<<<< HEAD
                beta = Math.Min(beta, eval);

                if (beta <= alpha) 
                    break;
=======
                beta = Math.Min(beta, eval); // Оновлюємо верхню межу (Beta)

                // --- ВІДСІКАННЯ ---
                if (beta <= alpha) 
                    break; // Alpha-відсікання: Максимізатор вже знайшов кращий варіант в іншій гілці
>>>>>>> d6d157f322a54e995d215b72e5daf226e0cfb9e8
            }
            return minEval;
        }
    }
}