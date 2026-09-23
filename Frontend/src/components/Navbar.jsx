import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { notificationApi } from '../api/notificationApi';

export const Navbar = ({ onToggleSidebar }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    let isMounted = true;
    const fetchUnreadCount = async () => {
      try {
        const res = await notificationApi.getAll();
        if (isMounted && res.success && res.data) {
          const unread = res.data.filter((n) => !n.isRead).length;
          setUnreadCount(unread);
        }
      } catch (err) {
        // Silently catch in navbar poll
      }
    };

    fetchUnreadCount();
    const interval = setInterval(fetchUnreadCount, 20000);
    return () => {
      isMounted = false;
      clearInterval(interval);
    };
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const getRoleBadgeColor = (role) => {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'bg-danger';
      case 'manager':
        return 'bg-warning text-dark';
      default:
        return 'bg-info text-dark';
    }
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-light bg-white border-bottom sticky-top px-3 py-2 shadow-sm">
      <div className="container-fluid">
        <button
          className="btn btn-outline-secondary d-md-none me-2"
          type="button"
          onClick={onToggleSidebar}
          aria-label="Toggle navigation"
        >
          <i className="bi bi-list fs-5"></i>
        </button>

        <Link className="navbar-brand fw-bold text-primary d-flex align-items-center gap-2" to="/dashboard">
          <i className="bi bi-kanban fs-4"></i>
          <span>TaskFlow</span>
        </Link>

        <div className="ms-auto d-flex align-items-center gap-3">
          {/* Notifications Bell */}
          <Link
            to="/notifications"
            className="btn btn-light position-relative p-2 rounded-circle"
            title="Notifications"
          >
            <i className="bi bi-bell fs-5 text-secondary"></i>
            {unreadCount > 0 && (
              <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style={{ fontSize: '0.65rem' }}>
                {unreadCount > 99 ? '99+' : unreadCount}
                <span className="visually-hidden">unread notifications</span>
              </span>
            )}
          </Link>

          {/* User Info & Role Badge */}
          <div className="d-flex align-items-center gap-2 border-start ps-3">
            <div className="text-end d-none d-sm-block">
              <div className="fw-semibold text-dark small">{user?.name}</div>
              <span className={`badge ${getRoleBadgeColor(user?.role)} py-0 px-2`} style={{ fontSize: '0.7rem' }}>
                {user?.role}
              </span>
            </div>
            <button
              onClick={handleLogout}
              className="btn btn-outline-danger btn-sm d-flex align-items-center gap-1"
              title="Logout"
            >
              <i className="bi bi-box-arrow-right"></i>
              <span className="d-none d-md-inline">Logout</span>
            </button>
          </div>
        </div>
      </div>
    </nav>
  );
};
