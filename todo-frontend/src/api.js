const API_BASE = "http://localhost:5086/api";  // todo Api port

export async function login(username, password) {
  const res = await fetch(`${API_BASE}/auth/login`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ username, password }),
});

 if (!res.ok) throw new Error("Login failed");
 return res.json(); // { token: "..." }
}
export async function register(username, password) {
    const res = await fetch(`${API_BASE}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
    });
    if (!res.ok) {
        const message = await res.text();
        throw new Error(message || "Registration failed");
    }
    return res.text();
}
export async function getTodos(token) {
  const res = await fetch(`${API_BASE}/todos`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error("Failed to fetch todos");
  return res.json();
}

export async function addTodo(token, title) {
  const res = await fetch(`${API_BASE}/todos`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ title }),
  });
  if (!res.ok) throw new Error("Failed to add todo");
  return res.json();
}

export async function completeTodo(token, id) {
  const res = await fetch(`${API_BASE}/todos/${id}/complete`, {
    method: "PUT",
    headers: { Authorization: `Bearer ${token}` },
   });
  if (!res.ok) throw new Error("Failed to complete todo");
}

export async function deleteTodo(token, id) {
  const res = await fetch(`${API_BASE}/todos/${id}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error("Failed to delete todo");
}
