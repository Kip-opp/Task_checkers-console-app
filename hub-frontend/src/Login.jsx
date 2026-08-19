import { useState } from "react";
import { login, register } from "./api";

export default function Login({ onLogin }) {
  const [isRegistering, setIsRegistering] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setInfo("");

    try {
      if (isRegistering) {
        await register(username, password);
        setInfo("Account created! You can now log in.");
        setIsRegistering(false);
      } else {
        const data = await login(username, password);
        onLogin(data.token, username.trim());
      }
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="auth-panel" onSubmit={handleSubmit}>
      <div><p className="eyebrow">Welcome back</p><h2>{isRegistering ? "Create your account" : "Sign in to Notes"}</h2><p className="muted">Your notes are private to your account.</p></div>
      <input
        className="field"
        aria-label="Username"
        autoComplete="username"
        placeholder="Username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />
      <input
        className="field"
        aria-label="Password"
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      <button className="button button--primary" type="submit">{isRegistering ? "Create account" : "Log in"}</button>

      {error && <p className="form-message form-message--error" role="alert">{error}</p>}
      {info && <p className="form-message form-message--success" role="status">{info}</p>}

      <p>
        {isRegistering ? "Already have an account?" : "Need an account?"}{" "}
        <button type="button" onClick={() => setIsRegistering(!isRegistering)}>
          {isRegistering ? "Log in instead" : "Create an account"}
        </button>
      </p>
    </form>
  );
}
