import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { notificationApi } from '../api/notificationApi';
import { Toast } from '../components/Toast';

export const NotificationsPage = () => {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState({ message: '', type: 'success' });

  const fetchNotifications = async () => {
    setLoading(true);
    try {
      const res = await notificationApi.getAll();
      if (res.success && res.data) {
        setNotifications(res.data);
      }
    } catch (err) {
      setToast({ message: 'Failed to load notifications.', type: 'error' });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, []);

  const handleMarkAsRead = async (id) => {
    try {
      await notificationApi.markAsRead(id);
      setNotifications((prev) =>
        prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
      );
    } catch (err) {
      setToast({ message: 'Failed to mark notification as read.', type: 'error' });
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await notificationApi.markAllAsRead();
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setToast({ message: 'All notifications marked as read.', type: 'success' });
    } catch (err) {
      setToast({ message: 'Failed to mark all as read.', type: 'error' });
    }
  };

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const getIconForType = (type) => {
    switch (type) {
      case 'TaskAssigned':
        return <i className="bi bi-person-check fs-4 text-primary"></i>;
      case 'TaskStatusUpdated':
        return <i className="bi bi-arrow-repeat fs-4 text-warning"></i>;
      default:
        return <i className="bi bi-info-circle fs-4 text-info"></i>;
    }
  };

  return (
    <div className="container-fluid p-0" style={{ maxWidth: '900px' }}>
      <Toast
        message={toast.message}
        type={toast.type}
        onClose={() => setToast({ message: '', type: 'success' })}
      />

      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-4">
        <div>
          <h3 className="fw-bold mb-1">Notifications</h3>
          <p className="text-muted small mb-0">
            Updates on task assignments, status changes, and team activities
          </p>
        </div>
        {unreadCount > 0 && (
          <button
            type="button"
            className="btn btn-outline-primary btn-sm d-flex align-items-center gap-1"
            onClick={handleMarkAllAsRead}
          >
            <i className="bi bi-check2-all"></i>
            <span>Mark All as Read</span>
          </button>
        )}
      </div>

      <div className="card bg-white border-0 shadow-sm">
        <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
          <span className="fw-semibold small">
            Unread Notifications: <span className="badge bg-primary">{unreadCount}</span>
          </span>
        </div>
        <div className="card-body p-0">
          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading notifications...</span>
              </div>
            </div>
          ) : notifications.length === 0 ? (
            <div className="text-center py-5 text-muted">
              <i className="bi bi-bell-slash fs-1 d-block mb-2"></i>
              <p className="mb-0">You have no notifications at this time.</p>
            </div>
          ) : (
            <div className="list-group list-group-flush">
              {notifications.map((n) => (
                <div
                  key={n.id}
                  className={`list-group-item p-3 d-flex align-items-start gap-3 transition-all ${
                    !n.isRead ? 'bg-light border-start border-primary border-4' : ''
                  }`}
                >
                  <div className="mt-1">{getIconForType(n.type)}</div>
                  <div className="flex-grow-1">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span className="badge bg-secondary bg-opacity-10 text-secondary" style={{ fontSize: '0.7rem' }}>
                        {n.type === 'TaskAssigned' ? 'Assignment' : n.type === 'TaskStatusUpdated' ? 'Status Update' : 'Notice'}
                      </span>
                      <span className="text-muted small" style={{ fontSize: '0.75rem' }}>
                        {new Date(n.createdAt).toLocaleString()}
                      </span>
                    </div>

                    <p className="mb-1 text-dark small">{n.message}</p>

                    <div className="d-flex align-items-center gap-2 mt-2">
                      {n.taskId && (
                        <Link to={`/tasks/${n.taskId}`} className="btn btn-sm btn-link p-0 text-decoration-none small">
                          <i className="bi bi-box-arrow-up-right me-1"></i>
                          View Task #{n.taskId}
                        </Link>
                      )}

                      {!n.isRead && (
                        <button
                          type="button"
                          className="btn btn-sm btn-outline-secondary py-0 px-2 ms-auto"
                          style={{ fontSize: '0.75rem' }}
                          onClick={() => handleMarkAsRead(n.id)}
                        >
                          Mark as Read
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
