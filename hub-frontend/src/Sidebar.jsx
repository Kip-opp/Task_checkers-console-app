import { NavLink } from "react-router-dom";

export default function Sidebar({ open, onClose, compact, onToggle }) {
  return (
    <>
      {open && <button className="drawer-backdrop" aria-label="Close navigation" onClick={onClose} />}
      <aside className={`sidebar ${compact ? "sidebar--compact" : ""} ${open ? "sidebar--open" : ""}`}>
        <div className="brand"><span className="brand-mark" aria-hidden="true">TH</span><span>Task &amp; Games</span></div>
        <nav aria-label="Primary navigation" className="nav-list">
          <NavLink to="/todo" onClick={onClose} className={({ isActive }) => `nav-link ${isActive ? "nav-link--active" : ""}`}><span aria-hidden="true">✦</span><span>Notes</span></NavLink>
          <NavLink to="/checkers" onClick={onClose} className={({ isActive }) => `nav-link ${isActive ? "nav-link--active" : ""}`}><span aria-hidden="true">◈</span><span>Checkers</span></NavLink>
        </nav>
        <button className="sidebar-toggle" type="button" onClick={onToggle} aria-label={compact ? "Expand navigation" : "Collapse navigation"} aria-pressed={compact}><span aria-hidden="true">{compact ? "→" : "←"}</span><span>{compact ? "Expand" : "Collapse"}</span></button>
      </aside>
    </>
  );
}
