import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Sidebar from "./Sidebar";
import TodoPage from "./pages/TodoPage";
import CheckersPage from "./pages/CheckersPage";

function App() {
  return (
    <Router>
      <div style={{ display: "flex" }}>
        <Sidebar />
        <div style={{ flex: 1 }}>
          <Routes>
            <Route path="/" element={<Navigate to="/todo" replace />} />
            <Route path="/checkers" element={<CheckersPage />} />
            <Route path="/todo" element={<TodoPage />} />
          </Routes>
        </div>
      </div>
    </Router>
  );
}

export default App;