const API_BASE = (import.meta.env.VITE_API_BASE_URL || "http://localhost:5086/api").replace(/\/$/, "");

async function request(path, options = {}) {
  const controller = new AbortController();
  const timeout = window.setTimeout(() => controller.abort(), options.timeout ?? 10000);
  const signal = options.signal;
  const onAbort = () => controller.abort();
  signal?.addEventListener("abort", onAbort, { once: true });
  try {
    const response = await fetch(`${API_BASE}${path}`, {
      ...options,
      signal: controller.signal,
      headers: { ...(options.body ? { "Content-Type": "application/json" } : {}), ...(options.headers || {}) },
    });
    const text = await response.text();
    let body = null;
    try { body = text ? JSON.parse(text) : null; } catch { body = text; }
    if (!response.ok) throw new Error(body?.error || body?.title || (typeof body === "string" ? body : `Request failed (${response.status})`));
    return body;
  } catch (error) {
    if (error.name === "AbortError") throw new Error("Request cancelled or timed out.", { cause: error });
    throw new Error(error.message || "Request failed.", { cause: error });
  } finally {
    window.clearTimeout(timeout);
    signal?.removeEventListener("abort", onAbort);
  }
}

export const login = (username, password, signal) => request("/auth/login", { method: "POST", body: JSON.stringify({ username, password }), signal });
export const register = (username, password, signal) => request("/auth/register", { method: "POST", body: JSON.stringify({ username, password }), signal });
export const getTodos = (token, signal) => request("/todos", { headers: { Authorization: `Bearer ${token}` }, signal });
export const addTodo = (token, title, signal) => request("/todos", { method: "POST", headers: { Authorization: `Bearer ${token}` }, body: JSON.stringify({ title }), signal });
export const completeTodo = (token, id, signal) => request(`/todos/${id}/complete`, { method: "PUT", headers: { Authorization: `Bearer ${token}` }, signal });
export const deleteTodo = (token, id, signal) => request(`/todos/${id}`, { method: "DELETE", headers: { Authorization: `Bearer ${token}` }, signal });
