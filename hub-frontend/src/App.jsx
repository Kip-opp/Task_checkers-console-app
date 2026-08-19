import { useEffect, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Sidebar from "./Sidebar";
import TodoPage from "./pages/TodoPage";
import CheckersPage from "./pages/CheckersPage";

function Shell() {
  const [compact, setCompact] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  useEffect(() => {
    if (!drawerOpen) return undefined;
    const close = (event) => { if (event.key === "Escape") setDrawerOpen(false); };
    document.addEventListener("keydown", close);
    document.body.classList.add("drawer-is-open");
    return () => { document.removeEventListener("keydown", close); document.body.classList.remove("drawer-is-open"); };
  }, [drawerOpen]);
  return (
      <div className="app-shell">
        <Sidebar open={drawerOpen} onClose={() => setDrawerOpen(false)} compact={compact} onToggle={() => setCompact((value) => !value)} />
        <main className="main-content">
          <button className="mobile-menu" type="button" onClick={() => setDrawerOpen(true)} aria-label="Open navigation">☰</button>
          <Routes>
            <Route path="/" element={<Navigate to="/todo" replace />} />
            <Route path="/checkers" element={<CheckersPage />} />
            <Route path="/todo" element={<TodoPage />} />
          </Routes>
        </main>
      </div>
  );
}

function App() { return <BrowserRouter><Shell /></BrowserRouter>; }

export default App;