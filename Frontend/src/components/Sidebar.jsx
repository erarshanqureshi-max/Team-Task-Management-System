import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const Sidebar = ({ isOpen, onClose }) => {
  const { user, isAdmin, isManager } = useAuth();

  const navItemClass = ({ isActive }) =>
    `nav-link d-flex align-items-center gap-3 px-3 py-2 rounded-2 mb-1 ${
      isActive
        ? 'bg-primary text-white fw-semibold shadow-sm'
        : 'text-secondary hover-bg-light'
    }`;

  return (
    <>
      {/* Mobile backdrop */}
      {isOpen && (
        <div
          className="modal-backdrop fade show d-md-none"
          onClick={onClose}
          style={{ zIndex: 1040 }}
        ></div>
      )}

      <aside
        className={`bg-white border-end position-fixed top-0 bottom-0 start-0 d-flex flex-column transition-all ${
          isOpen ? 'translate-middle-x-none' : 'd-none d-md-flex'
        }`}
        style={{
          width: '250px',
          zIndex: 1045,
          paddingTop: '65px',
          boxShadow: '0 0.125rem 0.25rem rgba(0, 0, 0, 0.075)',
        }}
      >
        <div className="p-3 flex-grow-1 overflow-auto">
          <div className="text-uppercase text-muted px-3 mb-2" style={{ fontSize: '0.7rem', letterSpacing: '1px' }}>
            Main Menu
          </div>
          <nav className="nav flex-column">
            <NavLink to="/dashboard" className={navItemClass} onClick={onClose}>
              <i className="bi bi-speedometer2 fs-5"></i>
              <span>Dashboard</span>
            </NavLink>

            <NavLink to="/tasks" end className={navItemClass} onClick={onClose}>
              <i className="bi bi-check2-square fs-5"></i>
              <span>Tasks</span>
            </NavLink>

            {(isAdmin || isManager) && (
              <NavLink to="/tasks/create" className={navItemClass} onClick={onClose}>
                <i className="bi bi-plus-circle fs-5"></i>
                <span>Create Task</span>
              </NavLink>
            )}

            <NavLink to="/teams" className={navItemClass} onClick={onClose}>
              <i className="bi bi-people fs-5"></i>
              <span>Teams</span>
            </NavLink>

            {(isAdmin || isManager) && (
              <NavLink to="/users" className={navItemClass} onClick={onClose}>
                <i className="bi bi-person-badge fs-5"></i>
                <span>Users</span>
              </NavLink>
            )}

            <NavLink to="/notifications" className={navItemClass} onClick={onClose}>
              <i className="bi bi-bell fs-5"></i>
              <span>Notifications</span>
            </NavLink>
          </nav>
        </div>

        {/* Sidebar Footer with Logged In User Preview */}
        <div className="p-3 border-top bg-light">
          <div className="d-flex align-items-center gap-2">
            <div
              className="rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-bold"
              style={{ width: '38px', height: '38px', fontSize: '0.9rem' }}
            >
              {user?.name ? user.name.charAt(0).toUpperCase() : 'U'}
            </div>
            <div className="overflow-hidden">
              <div className="fw-semibold text-truncate small">{user?.name}</div>
              <div className="text-muted text-truncate" style={{ fontSize: '0.75rem' }}>
                {user?.email}
              </div>
            </div>
          </div>
        </div>
      </aside>
    </>
  );
};
