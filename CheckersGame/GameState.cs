using Checkers.Core;

namespace CheckersGame;

public class GameState
{
    public Checkers.Core.GameState Current { get; private set; } = Checkers.Core.GameState.CreateInitialState(RuleSet.English);

    public void Reset(string variant = "english")
    {
        Current = Checkers.Core.GameState.CreateInitialState(RuleSet.FromId(variant));
    }

    public void Apply(Move move)
    {
        Current = CheckersRules.ApplyMove(Current, move);
    }
}