using Checkers.Core;

namespace CheckersGame;

public class GameLogic
{
    public IReadOnlyList<Move> GetLegalMoves(GameState state)
    {
        return CheckersRules.GetLegalMoves(state.Current, state.Current.CurrentPlayer);
    }
}