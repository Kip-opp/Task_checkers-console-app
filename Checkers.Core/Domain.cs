namespace Checkers.Core;

public enum Player { Black, Red }
public enum PieceKind { Man, King }
public enum GameStatus { InProgress, BlackWon, RedWon, Draw }
public enum DrawReason { None, ThreefoldRepetition, NoProgress }

public readonly record struct Position(int Row, int Column)
{
    public bool IsOn(int size) => Row >= 0 && Row < size && Column >= 0 && Column < size;
    public override string ToString() => $"{Row + 1},{Column + 1}";
}

public sealed record Piece(Player Player, PieceKind Kind);

public sealed record RuleSet(
    string Id,
    string Name,
    int BoardSize,
    int StartingPieces,
    bool MenCaptureBackward,
    bool FlyingKings,
    bool MaximumCaptureRequired,
    bool PromotionEndsCapture,
    bool PromotionAtTurnEnd,
    string DrawPolicy)
{
    public static RuleSet English { get; } = new("english", "English/American Checkers", 8, 12, false, false, false, true, false, "Threefold repetition and 40-move no-progress rule.");
    public static RuleSet International { get; } = new("international", "International Draughts", 10, 20, true, true, true, false, true, "Threefold repetition; advanced FMJD draw counters are deferred.");
    public static RuleSet FromId(string id) => id.Equals(International.Id, StringComparison.OrdinalIgnoreCase) ? International : English;
}

public sealed record Move(Position From, IReadOnlyList<Position> Path, IReadOnlyList<Position> Captures, bool Promotes = false)
{
    public Position To => Path[^1];
}

public sealed record GameState(
    RuleSet Rules,
    IReadOnlyDictionary<Position, Piece> Board,
    Player CurrentPlayer,
    IReadOnlyList<Move> History,
    IReadOnlyDictionary<string, int> PositionCounts,
    GameStatus Status = GameStatus.InProgress,
    DrawReason DrawReason = DrawReason.None,
    Position? ForcedPiece = null)
{
    public static GameState CreateInitialState(RuleSet rules)
    {
        var board = new Dictionary<Position, Piece>();
        var rows = rules.StartingPieces / (rules.BoardSize / 2);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < rules.BoardSize; column++)
                if (IsPlayable(new Position(row, column))) board[new(row, column)] = new Piece(Player.Black, PieceKind.Man);
        for (var row = rules.BoardSize - rows; row < rules.BoardSize; row++)
            for (var column = 0; column < rules.BoardSize; column++)
                if (IsPlayable(new Position(row, column))) board[new(row, column)] = new Piece(Player.Red, PieceKind.Man);

        var state = new GameState(rules, board, Player.Red, Array.Empty<Move>(), new Dictionary<string, int>());
        return state with { PositionCounts = new Dictionary<string, int> { [GetPositionKey(state)] = 1 } };
    }

    public static bool IsPlayable(Position position) => (position.Row + position.Column) % 2 == 1;
    public static string GetPositionKey(GameState state) => string.Join(";", state.Board.OrderBy(x => x.Key.Row).ThenBy(x => x.Key.Column).Select(x => $"{x.Key.Row},{x.Key.Column},{x.Value.Player},{x.Value.Kind}")) + $"|{state.CurrentPlayer}|{state.Rules.Id}";
}

public static class CheckersRules
{
    private static readonly (int Row, int Column)[] Directions = [(-1, -1), (-1, 1), (1, -1), (1, 1)];
    private const int MaxCaptureDepth = 50;

    public static IReadOnlyList<Move> GetLegalMoves(GameState state, Player player)
    {
        if (state.Status != GameStatus.InProgress || player != state.CurrentPlayer) return Array.Empty<Move>();
        if (state.ForcedPiece is Position forced)
            return GetCaptures(state, forced).Where(m => m.Captures.Count > 0).ToArray();

        var captures = state.Board.Where(x => x.Value.Player == player).SelectMany(x => GetCaptures(state, x.Key)).ToArray();
        if (captures.Length > 0)
            return state.Rules.MaximumCaptureRequired ? captures.Where(m => m.Captures.Count == captures.Max(x => x.Captures.Count)).ToArray() : captures;

        return state.Board.Where(x => x.Value.Player == player).SelectMany(x => GetOrdinaryMoves(state, x.Key)).ToArray();
    }

    public static IReadOnlyList<Move> GetLegalMovesForPiece(GameState state, Position piece) => GetLegalMoves(state, state.CurrentPlayer).Where(m => m.From == piece).ToArray();

    public static IReadOnlyList<Move> GetCaptureContinuations(GameState state, Position piece, IReadOnlyList<Position> path)
    {
        var moves = GetCaptures(state, piece);
        return moves.Where(m => m.Path.Count > path.Count && path.SequenceEqual(m.Path.Take(path.Count))).ToArray();
    }

    public static GameState ApplyMove(GameState state, Move move)
    {
        var legal = GetLegalMoves(state, state.CurrentPlayer).FirstOrDefault(candidate => candidate == move);
        if (legal is null) throw new InvalidOperationException("The move is not legal in the current position.");
        var board = new Dictionary<Position, Piece>(state.Board);
        var piece = board[move.From];
        board.Remove(move.From);
        foreach (var capture in move.Captures) board.Remove(capture);
        var promotes = piece.Kind == PieceKind.Man && ((piece.Player == Player.Red && move.To.Row == 0) || (piece.Player == Player.Black && move.To.Row == state.Rules.BoardSize - 1));
        if (promotes && (state.Rules.PromotionAtTurnEnd || state.Rules.PromotionEndsCapture)) piece = piece with { Kind = PieceKind.King };
        board[move.To] = piece;
        var nextPlayer = state.CurrentPlayer == Player.Red ? Player.Black : Player.Red;
        var next = state with { Board = board, CurrentPlayer = nextPlayer, History = state.History.Append(move).TakeLast(200).ToArray(), ForcedPiece = null };
        var key = GameState.GetPositionKey(next);
        var counts = new Dictionary<string, int>(state.PositionCounts) { [key] = state.PositionCounts.GetValueOrDefault(key) + 1 };
        var status = GetGameResult(next);
        return next with { PositionCounts = counts, Status = status, DrawReason = status == GameStatus.Draw ? DrawReason.ThreefoldRepetition : DrawReason.None };
    }

    public static GameStatus GetGameResult(GameState state)
    {
        if (state.PositionCounts.GetValueOrDefault(GameState.GetPositionKey(state)) >= 3) return GameStatus.Draw;
        var opponent = state.CurrentPlayer;
        if (state.Board.Values.All(piece => piece.Player != opponent)) return state.CurrentPlayer == Player.Red ? GameStatus.RedWon : GameStatus.BlackWon;
        if (GetLegalMovesWithoutResult(state).Count == 0) return state.CurrentPlayer == Player.Red ? GameStatus.RedWon : GameStatus.BlackWon;
        return GameStatus.InProgress;
    }

    private static IReadOnlyList<Move> GetLegalMovesWithoutResult(GameState state)
    {
        if (state.ForcedPiece is Position forced) return GetCaptures(state, forced);
        var captures = state.Board.Where(x => x.Value.Player == state.CurrentPlayer).SelectMany(x => GetCaptures(state, x.Key)).ToArray();
        if (captures.Length > 0) return state.Rules.MaximumCaptureRequired ? captures.Where(x => x.Captures.Count == captures.Max(y => y.Captures.Count)).ToArray() : captures;
        return state.Board.Where(x => x.Value.Player == state.CurrentPlayer).SelectMany(x => GetOrdinaryMoves(state, x.Key)).ToArray();
    }

    private static IEnumerable<Move> GetOrdinaryMoves(GameState state, Position from)
    {
        var piece = state.Board[from];
        foreach (var (dr, dc) in Directions)
        {
            if (piece.Kind == PieceKind.Man && dr != (piece.Player == Player.Red ? -1 : 1)) continue;
            var to = new Position(from.Row + dr, from.Column + dc);
            if (to.IsOn(state.Rules.BoardSize) && GameState.IsPlayable(to) && !state.Board.ContainsKey(to)) yield return new Move(from, [to], Array.Empty<Position>());
            if (piece.Kind == PieceKind.King && state.Rules.FlyingKings)
                foreach (var landing in ScanKingLandings(state, from, dr, dc)) yield return new Move(from, [landing], Array.Empty<Position>());
        }
    }

    private static IReadOnlyList<Move> GetCaptures(GameState state, Position from)
    {
        if (!state.Board.TryGetValue(from, out var piece)) return Array.Empty<Move>();
        var results = new List<Move>();
        SearchCaptures(state, from, piece, from, [], [], new HashSet<Position>(), results, 0);
        return results;
    }

    private static void SearchCaptures(GameState state, Position origin, Piece piece, Position current, IReadOnlyList<Position> path, IReadOnlyList<Position> captures, HashSet<Position> used, List<Move> results, int depth)
    {
        if (depth >= MaxCaptureDepth) throw new InvalidOperationException("Capture sequence exceeded the defensive depth limit.");
        var found = false;
        foreach (var (dr, dc) in Directions)
        {
            var enemy = new Position(current.Row + dr, current.Column + dc);
            if (!enemy.IsOn(state.Rules.BoardSize) || used.Contains(enemy) || !state.Board.TryGetValue(enemy, out var victim) || victim.Player == piece.Player) continue;
            if (piece.Kind == PieceKind.Man && !state.Rules.MenCaptureBackward && dr != (piece.Player == Player.Red ? -1 : 1)) continue;
            IEnumerable<Position> landings = piece.Kind == PieceKind.King && state.Rules.FlyingKings ? ScanKingLandingsAfterCapture(state, enemy, dr, dc, used) : [new Position(enemy.Row + dr, enemy.Column + dc)];
            foreach (var landing in landings)
            {
                if (!landing.IsOn(state.Rules.BoardSize) || !GameState.IsPlayable(landing) || state.Board.ContainsKey(landing)) continue;
                found = true;
                var nextPath = path.Append(landing).ToArray();
                var nextCaptures = captures.Append(enemy).ToArray();
                var nextUsed = new HashSet<Position>(used) { enemy };
                var promotes = piece.Kind == PieceKind.Man && ((piece.Player == Player.Red && landing.Row == 0) || (piece.Player == Player.Black && landing.Row == state.Rules.BoardSize - 1));
                if (promotes && state.Rules.PromotionEndsCapture) results.Add(new Move(origin, nextPath, nextCaptures, true));
                else SearchCaptures(state, origin, piece with { Kind = promotes && state.Rules.PromotionAtTurnEnd ? PieceKind.King : piece.Kind }, landing, nextPath, nextCaptures, nextUsed, results, depth + 1);
            }
        }
        if (!found && captures.Count > 0) results.Add(new Move(origin, path, captures));
    }

    private static IEnumerable<Position> ScanKingLandings(GameState state, Position from, int dr, int dc)
    {
        var row = from.Row + dr; var column = from.Column + dc;
        while (new Position(row, column).IsOn(state.Rules.BoardSize) && GameState.IsPlayable(new Position(row, column)) && !state.Board.ContainsKey(new Position(row, column))) { yield return new Position(row, column); row += dr; column += dc; }
    }

    private static IEnumerable<Position> ScanKingLandingsAfterCapture(GameState state, Position enemy, int dr, int dc, HashSet<Position> used)
    {
        var row = enemy.Row + dr; var column = enemy.Column + dc;
        while (new Position(row, column).IsOn(state.Rules.BoardSize) && GameState.IsPlayable(new Position(row, column)) && !state.Board.ContainsKey(new Position(row, column))) { yield return new Position(row, column); row += dr; column += dc; }
    }
}