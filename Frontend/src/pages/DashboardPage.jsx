import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { dashboardApi } from '../api/dashboardApi';
import { StatusBadge } from '../components/StatusBadge';
import { PriorityBadge } from '../components/PriorityBadge';

export const DashboardPage = () => {
  const { user, isAdmin, isManager } = useAuth();
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchDashboard = async () => {
      setLoading(true);
      setError('');
      try {
        let res;
        if (isAdmin) {
          res = await dashboardApi.getAdminStats();
        } else if (isManager) {
          res = await dashboardApi.getManagerStats();
        } else {
          res = await dashboardApi.getUserStats();
        }

        if (res.success && res.data) {
          setDashboardData(res.data);
        }
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to load dashboard statistics.');
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, [isAdmin, isManager]);

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert alert-danger" role="alert">
        <i className="bi bi-exclamation-triangle me-2"></i>
        {error}
      </div>
    );
  }

  const stats = dashboardData?.stats || {};
  const recentTasks = dashboardData?.recentTasks || [];

  return (
    <div className="container-fluid p-0">
      {/* Page Header */}
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-4">
        <div>
          <h3 className="fw-bold mb-1">
            {isAdmin && 'Organization Overview'}
            {isManager && `${dashboardData?.teamName || 'Team'} Dashboard`}
            {!isAdmin && !isManager && 'My Workspace'}
          </h3>
          <p className="text-muted small mb-0">
            Welcome back, <span className="fw-semibold text-dark">{user?.name}</span> ({user?.role})
          </p>
        </div>
        <div className="d-flex gap-2">
          {(isAdmin || isManager) && (
            <Link to="/tasks/create" className="btn btn-primary d-flex align-items-center gap-2">
              <i className="bi bi-plus-lg"></i>
              <span>Create Task</span>
            </Link>
          )}
          <Link to="/tasks" className="btn btn-outline-secondary d-flex align-items-center gap-2">
            <i className="bi bi-list-task"></i>
            <span>All Tasks</span>
          </Link>
        </div>
      </div>

      {/* Role specific banner */}
      {isManager && dashboardData?.teamName && (
        <div className="card bg-white border-0 shadow-sm mb-4">
          <div className="card-body d-flex align-items-center justify-content-between py-3">
            <div className="d-flex align-items-center gap-3">
              <div className="rounded p-2 bg-warning bg-opacity-10 text-warning">
                <i className="bi bi-people fs-4"></i>
              </div>
              <div>
                <h6 className="mb-0 fw-bold">{dashboardData.teamName}</h6>
                <span className="text-muted small">
                  Team Members: <strong>{dashboardData.teamMemberCount}</strong>
                </span>
              </div>
            </div>
            <Link to={`/teams/${dashboardData.teamId}`} className="btn btn-sm btn-outline-primary">
              Manage Team
            </Link>
          </div>
        </div>
      )}

      {isAdmin && (
        <div className="row g-3 mb-4">
          <div className="col-12 col-sm-6">
            <div className="card bg-white border-0 shadow-sm p-3 stat-card">
              <div className="d-flex align-items-center justify-content-between">
                <div>
                  <div className="text-muted small fw-semibold text-uppercase">Total Users</div>
                  <h3 className="fw-bold mb-0 text-dark mt-1">{dashboardData?.totalUsers ?? 0}</h3>
                </div>
                <div className="p-3 rounded-circle bg-primary bg-opacity-10 text-primary">
                  <i className="bi bi-people fs-3"></i>
                </div>
              </div>
            </div>
          </div>
          <div className="col-12 col-sm-6">
            <div className="card bg-white border-0 shadow-sm p-3 stat-card">
              <div className="d-flex align-items-center justify-content-between">
                <div>
                  <div className="text-muted small fw-semibold text-uppercase">Total Teams</div>
                  <h3 className="fw-bold mb-0 text-dark mt-1">{dashboardData?.totalTeams ?? 0}</h3>
                </div>
                <div className="p-3 rounded-circle bg-info bg-opacity-10 text-info">
                  <i className="bi bi-diagram-3 fs-3"></i>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Primary 6 Metrics Grid */}
      <div className="row g-3 mb-4">
        {/* Total Tasks */}
        <div className="col-12 col-sm-6 col-xl-2">
          <div className="card bg-white border-0 shadow-sm p-3 stat-card h-100">
            <div className="text-muted small fw-semibold text-uppercase">Total Tasks</div>
            <div className="d-flex align-items-baseline justify-content-between mt-2">
              <h2 className="fw-bold mb-0 text-dark">{stats.totalTasks ?? 0}</h2>
              <i className="bi bi-kanban fs-4 text-primary opacity-50"></i>
            </div>
          </div>
        </div>

        {/* To Do */}
        <div className="col-12 col-sm-6 col-xl-2">
          <div className="card bg-white border-0 shadow-sm p-3 stat-card h-100">
            <div className="text-muted small fw-semibold text-uppercase">To Do</div>
            <div className="d-flex align-items-baseline justify-content-between mt-2">
              <h2 className="fw-bold mb-0 text-secondary">{stats.toDoTasks ?? 0}</h2>
              <i className="bi bi-hourglass-top fs-4 text-secondary opacity-50"></i>
            </div>
          </div>
        </div>

        {/* In Progress */}
        <div className="col-12 col-sm-6 col-xl-2">
          <div className="card bg-white border-0 shadow-sm p-3 stat-card h-100">
            <div className="text-muted small fw-semibold text-uppercase">In Progress</div>
            <div className="d-flex align-items-baseline justify-content-between mt-2">
              <h2 className="fw-bold mb-0 text-primary">{stats.inProgressTasks ?? 0}</h2>
              <i className="bi bi-arrow-repeat fs-4 text-primary opacity-50"></i>
            </div>
          </div>
        </div>

        {/* Done */}
        <div className="col-12 col-sm-6 col-xl-2">
          <div className="card bg-white border-0 shadow-sm p-3 stat-card h-100">
            <div className="text-muted small fw-semibold text-uppercase">Done</div>
            <div className="d-flex align-items-baseline justify-content-between mt-2">
              <h2 className="fw-bold mb-0 text-success">{stats.doneTasks ?? 0}</h2>
              <i className="bi bi-check2-circle fs-4 text-success opacity-50"></i>
            </div>
          </div>
        </div>

        {/* Overdue */}
        <div className="col-12 col-sm-6 col-xl-2">
          <div className="card bg-white border-0 shadow-sm p-3 stat-card h-100 border-start border-danger border-4">
            <div className="text-muted small fw-semibold text-uppercase">Overdue</div>
            <div className="d-flex align-items-baseline justify-content-between mt-2">
              <h2 className="fw-bold mb-0 text-danger">{stats.overdueTasks ?? 0}</h2>
              <i className="bi bi-clock-history fs-4 text-danger opacity-50"></i>
            </div>
          </div>
        </div>

        {/* High Priority */}
        <div className="col-12 col-sm-6 col-xl-2">
          <div className="card bg-white border-0 shadow-sm p-3 stat-card h-100 border-start border-warning border-4">
            <div className="text-muted small fw-semibold text-uppercase">High Priority</div>
            <div className="d-flex align-items-baseline justify-content-between mt-2">
              <h2 className="fw-bold mb-0 text-warning">{stats.highPriorityTasks ?? 0}</h2>
              <i className="bi bi-fire fs-4 text-warning opacity-50"></i>
            </div>
          </div>
        </div>
      </div>

      {/* Recent Tasks Card */}
      <div className="card bg-white border-0 shadow-sm">
        <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
          <h5 className="mb-0 fw-bold">Recent Tasks</h5>
          <Link to="/tasks" className="btn btn-sm btn-link text-decoration-none">
            View All <i className="bi bi-arrow-right"></i>
          </Link>
        </div>
        <div className="card-body p-0">
          {recentTasks.length === 0 ? (
            <div className="text-center py-5 text-muted">
              <i className="bi bi-inbox fs-1 d-block mb-2"></i>
              <p className="mb-0">No tasks found.</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Team</th>
                    <th>Assignee</th>
                    <th>Priority</th>
                    <th>Status</th>
                    <th>Deadline</th>
                    <th className="text-end">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {recentTasks.map((t) => (
                    <tr key={t.id}>
                      <td>
                        <Link to={`/tasks/${t.id}`} className="fw-semibold text-dark text-decoration-none hover-primary">
                          {t.title}
                        </Link>
                      </td>
                      <td className="small text-muted">{t.teamName}</td>
                      <td className="small">
                        {t.assignedToUserName ? (
                          <span className="badge bg-light text-dark border">
                            <i className="bi bi-person me-1"></i>
                            {t.assignedToUserName}
                          </span>
                        ) : (
                          <span className="text-muted fst-italic">Unassigned</span>
                        )}
                      </td>
                      <td>
                        <PriorityBadge priority={t.priority} />
                      </td>
                      <td>
                        <StatusBadge status={t.status} />
                      </td>
                      <td className="small">
                        {t.deadline ? new Date(t.deadline).toLocaleDateString() : '-'}
                      </td>
                      <td className="text-end">
                        <Link to={`/tasks/${t.id}`} className="btn btn-sm btn-outline-secondary">
                          Details
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
