import { NavLink } from "react-router-dom";

export default function Sidebar() {
    const linkStyle = ({ isActive }) => ({
        display: "block",
        padding: "10px 16px",
        textDecoration: "none",
        color: isActive ? "white" : "black",
        background: isActive ? "#2b6cb0" : "transparent",
        borderRadius: "6px",
        marginBottom: "6px",
    });

    return (
        <nav style={{ width: "200px", padding: "16px", background: "#1a1a2e", height: "100vh" }}>
            <h2 style={{ color: "#fff", fontSize: "18px", marginBottom: "20px" }}>My Hub</h2>
            <NavLink to="/todo" style={linkStyle}>
                Tasks
            </NavLink>
            <NavLink to="/checkers" style={linkStyle}>
                Checkers
            </NavLink>
        </nav>

    );
}