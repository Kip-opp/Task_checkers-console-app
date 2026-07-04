import { useState } from "react";
import Login from "../Login";
import TodoList from "../TodoList";

export default function TodoPage() {
    const [token, setToken] = useState(null);

    return (
        <div style={{ padding: "24px" }}>
          <h1>Todo</h1>
            {!token ? <Login onLogin={setToken} /> : <TodoList token={token} />}
        </div>
    );
}