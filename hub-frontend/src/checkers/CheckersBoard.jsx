import { useEffect, useReducer } from "react";
import { applyMove, chooseComputerMove, createGame, legalMoves, RULES, snapshot } from "./checkersApi";

function reducer(game, action) {
  if (action.type === "reset") return createGame(action.variant || game.rules.id, action.mode || game.mode);
  if (action.type === "undo" && game.past.length) {
    const previous = game.past.at(-1);
    return { ...previous, past: game.past.slice(0, -1), future: [snapshot(game), ...game.future].slice(0, 40) };
  }
  if (action.type === "redo" && game.future.length) {
    const next = game.future[0];
    return { ...next, past: [...game.past, snapshot(game)].slice(-40), future: game.future.slice(1) };
  }
  if (action.type === "computerMove") {
    if (game.mode !== "computer" || game.turn !== "black" || game.status !== "playing") return game;
    const computerMove = chooseComputerMove(game);
    return computerMove ? { ...applyMove(game, computerMove), past: game.past, future: game.future } : game;
  }
  if (action.type !== "cell" || game.status !== "playing") return game;
  if (game.mode === "computer" && game.turn === "black") return game;
  const [row, col] = action.position; const piece = game.board[row][col]; const moves = legalMoves(game);
  if (piece?.player === game.turn) return { ...game, selected: [row, col] };
  const move = moves.find((candidate) => candidate.from.join() === game.selected?.join() && candidate.path.at(-1).join() === [row, col].join());
  if (!move) return game;
  const afterHumanMove = applyMove(game, move);
  return { ...afterHumanMove, past: [...game.past, snapshot(game)].slice(-40), future: [] };
}

const playerName = (player) => player === "red" ? "Red" : "Black";
export default function CheckersBoard() {
  const [game, dispatch] = useReducer(reducer, "english", createGame);
  useEffect(() => {
    if (game.mode !== "computer" || game.turn !== "black" || game.status !== "playing") return undefined;
    const timer = window.setTimeout(() => dispatch({ type: "computerMove" }), 700);
    return () => window.clearTimeout(timer);
  }, [game.mode, game.turn, game.status, game.history.length]);
  const moves = legalMoves(game); const targets = new Set(moves.filter((move) => game.selected && move.from[0] === game.selected[0] && move.from[1] === game.selected[1]).map((move) => move.path.at(-1).join()));
  const status = game.status === "playing" ? `${game.mode === "computer" && game.turn === "black" ? "Computer is thinking" : `${playerName(game.turn)} to move`}${moves.some((move) => move.captures.length) ? " · capture required" : ""}` : `${game.status === "draw" ? "Draw" : `${game.status === "red-won" ? "Red" : "Black"} wins`} · start a new game to play again`;
  const rules = game.rules;
  return <div className="game-workspace">
    <header className="game-header"><div><p className="eyebrow">{game.mode === "computer" ? "Solo match" : "Two-player local match"}</p><h1>{rules.name}</h1><p className="lede">{game.mode === "computer" ? "You play Red. The computer plays Black." : "Red moves first. Select a piece, then choose a highlighted destination."}</p></div><div className="game-actions"><label className="select-label" htmlFor="mode">Game mode<select id="mode" value={game.mode} onChange={(event) => dispatch({ type: "reset", mode: event.target.value })}><option value="local">Two players</option><option value="computer">Play against computer</option></select></label><label className="select-label" htmlFor="variant">Variant<select id="variant" value={rules.id} onChange={(event) => dispatch({ type: "reset", variant: event.target.value })}>{Object.values(RULES).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label><button className="button button--primary" onClick={() => dispatch({ type: "reset" })}>New game</button><div className="history-controls"><button className="button button--small" disabled={!game.past.length} onClick={() => dispatch({ type: "undo" })}>Undo</button><button className="button button--small" disabled={!game.future.length} onClick={() => dispatch({ type: "redo" })}>Redo</button></div></div></header>
    <div className="game-layout"><section className="board-panel" aria-labelledby="board-heading"><h2 id="board-heading" className="sr-only">{rules.name} board</h2><div className="status-banner" role="status" aria-live="polite">{status}</div><div className="board" style={{ "--board-size": rules.size }}>{game.board.map((row, rowIndex) => row.map((piece, colIndex) => { const key = `${rowIndex}-${colIndex}`; const selected = game.selected?.join() === [rowIndex, colIndex].join(); const target = targets.has(key); return <button key={key} className={`square ${(rowIndex + colIndex) % 2 ? "square--dark" : "square--light"} ${selected ? "square--selected" : ""} ${target ? "square--target" : ""}`} aria-label={piece ? `${playerName(piece.player)} ${piece.king ? "king" : "man"} at row ${rowIndex + 1} column ${colIndex + 1}${target ? "; legal move target" : ""}` : `Empty square at row ${rowIndex + 1} column ${colIndex + 1}${target ? "; legal move target" : ""}`} aria-pressed={selected} onClick={() => dispatch({ type: "cell", position: [rowIndex, colIndex] })}>{piece && <span className={`piece piece--${piece.player}`}>{piece.king ? "K" : ""}</span>}{target && <span className="target-dot" aria-hidden="true" />}</button>; }))}</div><p className="board-note">Dark squares are playable. A ring marks legal destinations; captures are announced in the status.</p></section>
      <aside className="rules-panel"><details open><summary>How {rules.id === "english" ? "English checkers" : "International draughts"} works</summary><dl><div><dt>Board</dt><dd>{rules.size} × {rules.size}, {rules.startingPieces ?? rules.rows * rules.size / 2} pieces per side</dd></div><div><dt>Men</dt><dd>Move one diagonal square forward; captures are {rules.backwardCapture ? "forward or backward" : "forward only"}.</dd></div><div><dt>Captures</dt><dd>Compulsory. {rules.maximumCapture ? "The path capturing the most pieces is required." : "Any available capture path is legal."}</dd></div><div><dt>Kings</dt><dd>{rules.flyingKing ? "Flying kings travel across open diagonals." : "Kings move one diagonal square in either direction."}</dd></div><div><dt>Promotion</dt><dd>{rules.promotionEndsCapture ? "Promotion during a capture ends the turn." : "Promotion is applied after the entire capture sequence."}</dd></div><div><dt>Draws</dt><dd>{rules.draw}</dd></div></dl></details><section className="history"><h2>Move history</h2>{game.history.length ? <ol>{game.history.slice(-12).map((move, index) => <li key={`${index}-${move.from.join()}`}>{index % 2 ? "Black" : "Red"}: {move.from.join(",")} → {move.path.at(-1).join(",")}{move.captures.length ? ` · ${move.captures.length} captured` : ""}</li>)}</ol> : <p className="muted">Moves will appear here as the match develops.</p>}</section></aside></div>
  </div>;
}
