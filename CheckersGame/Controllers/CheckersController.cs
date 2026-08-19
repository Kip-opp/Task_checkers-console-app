using Microsoft.AspNetCore.Mvc;
using Checkers.Core;

namespace CheckersGame.Controllers;

public record MoveRequest(int FromRow, int FromCol, int ToRow, int ToCol);

[ApiController]
[Route("api/[controller]")]
public class CheckersController : ControllerBase
{
    private readonly GameState _state;

    public CheckersController(GameState state)
    {
        _state = state;
    }

    [HttpGet("board")]
    public IActionResult GetBoard()
    {
        return Ok(ToResponse(_state.Current));
    }

    [HttpPost("moves")]
    public IActionResult GetMoves(int row, int col)
    {
        var moves = CheckersRules.GetLegalMovesForPiece(_state.Current, new Position(row, col));
        return Ok(moves.Select(move => new { row = move.To.Row, column = move.To.Column, captures = move.Captures }));
    }

    [HttpPost("move")]
    public IActionResult MakeMove(MoveRequest request)
    {
        var legal = CheckersRules.GetLegalMoves(_state.Current, _state.Current.CurrentPlayer)
            .FirstOrDefault(move => move.From == new Position(request.FromRow, request.FromCol) && move.To == new Position(request.ToRow, request.ToCol));
        if (legal is null) return BadRequest(new { error = "Invalid move." });
        _state.Apply(legal);
        return Ok(ToResponse(_state.Current));

    }

    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _state.Reset();
        return Ok(ToResponse(_state.Current));
    }

    private static object ToResponse(Checkers.Core.GameState state) => new
    {
        board = Enumerable.Range(0, state.Rules.BoardSize).Select(row => Enumerable.Range(0, state.Rules.BoardSize).Select(column =>
            state.Board.TryGetValue(new Position(row, column), out var piece) ? new { player = piece.Player.ToString().ToLowerInvariant(), king = piece.Kind == PieceKind.King } : null).ToArray()).ToArray(),
        turn = state.CurrentPlayer.ToString().ToLowerInvariant(),
        variant = state.Rules.Id,
        status = state.Status.ToString(),
        rules = state.Rules
    };
}
        



    