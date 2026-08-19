import { useCallback, useEffect, useState } from "react";
import { addTodo, completeTodo, deleteTodo, getTodos } from "./api";

export default function TodoList({ token, username }) {
  const [todos, setTodos] = useState([]);
  const [newTitle, setNewTitle] = useState("");
  const [filter, setFilter] = useState("open");
  const [loading, setLoading] = useState(true);
  const [pending, setPending] = useState(null);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");

  const loadTodos = useCallback(async (signal) => {
    setLoading(true); setError("");
    try { setTodos(await getTodos(token, signal)); } catch (err) { if (err.name !== "AbortError") setError(err.message); } finally { if (!signal?.aborted) setLoading(false); }
  }, [token]);
  useEffect(() => { const controller = new AbortController(); Promise.resolve().then(() => loadTodos(controller.signal)); return () => controller.abort(); }, [loadTodos]);

  async function handleAdd(event) {
    event.preventDefault(); const title = newTitle.trim();
    if (!title || title.length > 200 || pending) return;
    setPending("add"); setError(""); setNotice("");
    try { const created = await addTodo(token, title); setTodos((items) => [...items, created]); setNewTitle(""); setNotice("Note added."); } catch (err) { setError(err.message); } finally { setPending(null); }
  }
  async function handleComplete(id) { setPending(id); setError(""); try { await completeTodo(token, id); setTodos((items) => items.map((item) => item.id === id ? { ...item, isDone: true } : item)); setNotice("Note completed."); } catch (err) { setError(err.message); } finally { setPending(null); } }
  async function handleDelete(id) { setPending(id); setError(""); try { await deleteTodo(token, id); setTodos((items) => items.filter((item) => item.id !== id)); setNotice("Note deleted."); } catch (err) { setError(err.message); } finally { setPending(null); } }

  const visible = todos.filter((todo) => filter === "all" || (filter === "open" ? !todo.isDone : todo.isDone));
  return <section className="notes-workspace" aria-labelledby="notes-list-heading">
    <div className="notes-toolbar"><div><h2 id="notes-list-heading">{username ? `${username}'s notes` : "Your notes"}</h2><p className="muted">{todos.filter((todo) => !todo.isDone).length} open notes</p></div><div className="segmented" role="group" aria-label="Filter notes">{[["open", "Open"], ["done", "Completed"], ["all", "All"]].map(([value, label]) => <button key={value} className={filter === value ? "segment segment--active" : "segment"} aria-pressed={filter === value} onClick={() => setFilter(value)}>{label}</button>)}</div></div>
    <form className="add-note" onSubmit={handleAdd}><label className="sr-only" htmlFor="new-note">New note</label><input id="new-note" className="field" maxLength="200" value={newTitle} onChange={(event) => setNewTitle(event.target.value)} placeholder="What needs your attention?" /><button className="button button--primary" disabled={pending === "add" || !newTitle.trim()}>{pending === "add" ? "Adding..." : "Add note"}</button></form>
    {notice && <p className="form-message form-message--success" role="status">{notice}</p>}{error && <div className="error-row" role="alert"><span>{error}</span><button className="button button--quiet" onClick={() => loadTodos()}>Retry</button></div>}
    {loading ? <p className="empty-state">Loading your notes...</p> : visible.length === 0 ? <div className="empty-state"><strong>{filter === "open" ? "Nothing open." : "No notes here yet."}</strong><span>{filter === "open" ? "Add a note above to get started." : "Try another filter or add a new note."}</span></div> : <ul className="note-list">{visible.map((todo) => <li className={`note-item ${todo.isDone ? "note-item--done" : ""}`} key={todo.id}><span className="note-copy">{todo.title}</span><div className="note-actions">{!todo.isDone && <button className="button button--small" disabled={pending === todo.id} onClick={() => handleComplete(todo.id)}>Mark done</button>}<button className="button button--small button--danger" disabled={pending === todo.id} onClick={() => handleDelete(todo.id)}>Delete</button></div></li>)}</ul>}
+  </section>;
}
