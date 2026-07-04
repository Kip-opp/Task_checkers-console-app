import { useState, useEffect } from "react";
import { getTodos, addTodo, completeTodo, deleteTodo } from "./api";

export default function TodoList({ token }) {
  const [todos, setTodos] = useState([]);
  const [newTitle, setNewTitle] = useState("");

  async function loadTodos() {
    const data = await getTodos(token);
    setTodos(data);
}

useEffect(() => {
  loadTodos();
}, []);

async function handleAdd(e) {
  e.preventDefault();
  if (!newTitle.trim()) return;
  await addTodo(token, newTitle);
  setNewTitle("");
  loadTodos();
}

async function handleComplete(id) {
  await completeTodo(token, id);
  loadTodos();
}

async function handleDelete(id) {
  await deleteTodo(token, id);
  loadTodos();
}

return (
  <div>
    <h2>Your Todos</h2>
    <form onSubmit={handleAdd}>
      <input
        placeholder="New task..."
        value={newTitle}
        onChange={(e) => setNewTitle(e.target.value)}
      />
      <button type="submit">Add</button>
    </form>

    <ul>
      {todos.map((todo) => (
        <li key={todo.id} style={{ textDecoration: todo.isDone ? "line-through" : "none" }}>
          {todo.title}
          {!todo.isDone && (
            <button onClick={() => handleComplete(todo.id)}>Done</button>
          )}
          <button onClick={() => handleDelete(todo.id)}>Delete</button>
        </li>
      ))}
    </ul>
  </div>
 );
}
