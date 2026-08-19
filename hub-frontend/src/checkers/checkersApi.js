export const RULES = {
  english: { id: "english", name: "English/American Checkers", size: 8, rows: 3, backwardCapture: false, flyingKing: false, maximumCapture: false, promotionEndsCapture: true, draw: "Threefold repetition and a documented no-progress policy." },
  international: { id: "international", name: "International Draughts", size: 10, rows: 4, backwardCapture: true, flyingKing: true, maximumCapture: true, promotionEndsCapture: false, draw: "Threefold repetition; advanced FMJD draw counters are deferred." },
};
const directions = [[-1, -1], [-1, 1], [1, -1], [1, 1]];
const playable = (row, col) => (row + col) % 2 === 1;
const inside = (size, row, col) => row >= 0 && row < size && col >= 0 && col < size;
const cloneBoard = (board) => board.map((row) => row.map((piece) => piece ? { ...piece } : null));

export function createGame(variant = "english", mode = "local") {
  const rules = RULES[variant] || RULES.english; const board = Array.from({ length: rules.size }, () => Array(rules.size).fill(null));
  for (let row = 0; row < rules.rows; row++) for (let col = 0; col < rules.size; col++) if (playable(row, col)) board[row][col] = { player: "black", king: false };
  for (let row = rules.size - rules.rows; row < rules.size; row++) for (let col = 0; col < rules.size; col++) if (playable(row, col)) board[row][col] = { player: "red", king: false };
  return { rules, mode, board, turn: "red", selected: null, status: "playing", history: [], lastMove: null, past: [], future: [] };
}
function capturesFor(game, from, piece, board = game.board, used = [], path = [], captures = [], depth = 0, origin = from) {
  if (depth > 50) return []; const moves = [];
  for (const [dr, dc] of directions) {
    const er = from[0] + dr, ec = from[1] + dc, enemy = board[er]?.[ec];
    if (!inside(game.rules.size, er, ec) || !enemy || enemy.player === piece.player || used.some(([r, c]) => r === er && c === ec)) continue;
    if (!piece.king && !game.rules.backwardCapture && dr !== (piece.player === "red" ? -1 : 1)) continue;
    const landings = [];
    if (piece.king && game.rules.flyingKing) { let r = er + dr, c = ec + dc; while (inside(game.rules.size, r, c) && playable(r, c) && !board[r][c]) { landings.push([r, c]); r += dr; c += dc; } }
    else landings.push([er + dr, ec + dc]);
    for (const landing of landings) {
      const [lr, lc] = landing; if (!inside(game.rules.size, lr, lc) || !playable(lr, lc) || board[lr][lc]) continue;
      const nextPath = [...path, landing], nextCaptures = [...captures, [er, ec]], nextUsed = [...used, [er, ec]];
      const promotes = !piece.king && ((piece.player === "red" && lr === 0) || (piece.player === "black" && lr === game.rules.size - 1));
      if (promotes && game.rules.promotionEndsCapture) moves.push({ from: origin, path: nextPath, captures: nextCaptures, promotes: true });
      else { const nextPiece = promotes && game.rules.id === "international" ? { ...piece, king: true } : piece; const more = capturesFor(game, landing, nextPiece, board, nextUsed, nextPath, nextCaptures, depth + 1, origin); moves.push(...(more.length ? more : [{ from: origin, path: nextPath, captures: nextCaptures, promotes } ])); }
    }
  }
  return moves;
}
export function legalMoves(game) {
  if (game.status !== "playing") return []; const pieces = [];
  game.board.forEach((row, r) => row.forEach((piece, c) => { if (piece?.player === game.turn && (!game.selected || (game.selected[0] === r && game.selected[1] === c))) pieces.push({ piece, from: [r, c] }); }));
  const captures = pieces.flatMap(({ piece, from }) => capturesFor(game, from, piece));
  if (captures.length) { const max = Math.max(...captures.map((move) => move.captures.length)); return game.rules.maximumCapture ? captures.filter((move) => move.captures.length === max) : captures; }
  return pieces.flatMap(({ piece, from }) => directions.flatMap(([dr, dc]) => { if (!piece.king && dr !== (piece.player === "red" ? -1 : 1)) return []; const landing = [from[0] + dr, from[1] + dc]; if (inside(game.rules.size, ...landing) && playable(...landing) && !game.board[landing[0]][landing[1]]) return [{ from, path: [landing], captures: [], promotes: landing[0] === (piece.player === "red" ? 0 : game.rules.size - 1) }]; if (piece.king && game.rules.flyingKing) { const result = []; let r = landing[0], c = landing[1]; while (inside(game.rules.size, r, c) && playable(r, c) && !game.board[r][c]) { result.push({ from, path: [[r, c]], captures: [], promotes: false }); r += dr; c += dc; } return result; } return []; }));
}
export function chooseComputerMove(game) {
  const moves = legalMoves(game);
  return moves.slice().sort((left, right) => {
    const leftKey = `${left.from[0]},${left.from[1]},${left.path.at(-1)[0]},${left.path.at(-1)[1]}`;
    const rightKey = `${right.from[0]},${right.from[1]},${right.path.at(-1)[0]},${right.path.at(-1)[1]}`;
    return leftKey.localeCompare(rightKey);
  })[0] || null;
}
  export function snapshot(game) {
    return { ...game, past: [], future: [] };
  }
export function applyMove(game, move) {
  const board = cloneBoard(game.board); const piece = { ...board[move.from[0]][move.from[1]] }; board[move.from[0]][move.from[1]] = null;
  move.captures.forEach(([r, c]) => { board[r][c] = null; }); if (move.promotes) piece.king = true; board[move.path.at(-1)[0]][move.path.at(-1)[1]] = piece;
  const turn = game.turn === "red" ? "black" : "red"; const next = { ...game, board, turn, selected: null, lastMove: move, history: [...game.history, move].slice(-80) };
  const opponentHasPieces = board.some((row) => row.some((item) => item?.player === turn)); return { ...next, status: !opponentHasPieces || legalMoves(next).length === 0 ? (game.turn === "red" ? "red-won" : "black-won") : "playing" };
}
