import { useState } from "react";
import Login from "../Login";
import TodoList from "../TodoList";

export default function TodoPage() {
    const [token, setToken] = useState(null);
        const [username, setUsername] = useState("");

    return (
                <div className="page page--notes">
                    <header className="page-header"><div><p className="eyebrow">Personal workspace</p><h1>Notes</h1><p className="lede">Keep the small commitments visible and moving.</p></div>{token && <button className="button button--quiet" onClick={() => { setToken(null); setUsername(""); }}>Log out</button>}</header>
                        {!token ? <Login onLogin={(nextToken, nextUsername) => { setToken(nextToken); setUsername(nextUsername); }} /> : <TodoList token={token} username={username} />}
        </div>
    );
}