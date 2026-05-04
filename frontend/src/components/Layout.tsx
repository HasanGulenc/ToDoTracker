import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function Layout() {
  const { auth, signOut } = useAuth();
  const navigate = useNavigate();

  function handleSignOut() {
    signOut();
    navigate('/login');
  }

  return (
    <div className="app-shell">
      <nav className="topnav">
        <span className="topnav-brand">TaskManager</span>
        <div className="topnav-links">
          <NavLink
            to="/tasks"
            className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
          >
            All Tasks
          </NavLink>
          <NavLink
            to="/due-today"
            className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
          >
            Due Today
          </NavLink>
        </div>
        <div className="topnav-right">
          <span className="topnav-email">{auth?.email}</span>
          <button className="btn-ghost" onClick={handleSignOut}>
            Sign Out
          </button>
        </div>
      </nav>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
