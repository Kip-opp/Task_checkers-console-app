import { useState } from "react";
import Login from "./Login";
import TodoList from "./TodoList";

function App() {
  const [token, setToken] = useState(null);

  return (
    <div>
      <h1>Todo App</h1>
      {token ? (
        <TodoList token={token} />
      ) : (
        <Login onLogin={setToken} />
      )}
    </div>
  );
}

export default App;