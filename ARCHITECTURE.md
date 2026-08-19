# Task and Games Hub architecture

- `hub-frontend/src/main.jsx` is the React/Vite entry point; `App.jsx` owns routing and the shared layout.
- `src/pages` hosts route-level Notes and Checkers screens; `src/api.js` is the current API boundary and `src/checkers` is the game UI adapter.
- `TodoApi` contains authentication, JWT configuration, and todo controllers; `Shared` contains `User`, `TodoItem`, and `TodoDbContext`.
- `TodoApp` is the direct EF console client. `CheckersGame` is the existing web/console scaffold and now consumes `Checkers.Core`.
- Solution references: `TodoApi -> Shared`; `TodoApp -> Shared`; `CheckersGame -> Checkers.Core` after this change. `Shared` has EF Core/SQLite dependencies; `Checkers.Core` has no framework or persistence dependencies.
- `Checkers.Core` is the canonical immutable rules domain. The frontend is presentation-only and must consume the same move/state contract when a checkers endpoint is connected.

The International implementation supports 10x10 placement, backward man captures, flying kings, compulsory maximum capture, deferred end-of-turn promotion, and bounded capture search. Advanced FMJD draw counters remain deferred and are labeled in the rules metadata/UI.