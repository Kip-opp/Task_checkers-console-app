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
        onLogin(data.token);
      }
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>{isRegistering ? "Register" : "Login"}</h2>
      <input
        placeholder="Username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />
      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      <button type="submit">{isRegistering ? "Register" : "Log in"}</button>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {info && <p style={{ color: "green" }}>{info}</p>}

      <p>
        {isRegistering ? "Already have an account?" : "Need an account?"}{" "}
        <button type="button" onClick={() => setIsRegistering(!isRegistering)}>
          {isRegistering ? "Log in instead" : "Register instead"}
        </button>
      </p>
    </form>
  );
}
