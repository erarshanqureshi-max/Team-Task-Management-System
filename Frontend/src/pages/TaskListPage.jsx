import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { taskApi } from '../api/taskApi';
import { StatusBadge } from '../components/StatusBadge';
import { PriorityBadge } from '../components/PriorityBadge';
import { Toast } from '../components/Toast';

export const TaskListPage = () => {
  const { user, isAdmin, isManager } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState({ message: '', type: 'success' });

  // Filters state
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [deadlineFilter, setDeadlineFilter] = useState('');

  const fetchTasks = async () => {
    setLoading(true);
    try {
      const params = {};
      if (search.trim()) params.search = search.trim();
      if (statusFilter) params.status = statusFilter;
      if (priorityFilter) params.priority = priorityFilter;
      if (deadlineFilter) params.deadline = deadlineFilter;

      const res = await taskApi.getTasks(params);
      if (res.success && res.data) {
        setTasks(res.data);
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to load tasks.',
        type: 'error',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTasks();
  }, [statusFilter, priorityFilter, deadlineFilter]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    fetchTasks();
  };

  const handleResetFilters = () => {
    setSearch('');
    setStatusFilter('');
    setPriorityFilter('');
    setDeadlineFilter('');
    setTimeout(fetchTasks, 0);
  };

  const handleStatusChange = async (taskId, newStatus) => {
    try {
      await taskApi.updateStatus(taskId, newStatus);
      setToast({ message: `Task status updated to ${newStatus}`, type: 'success' });
      fetchTasks();
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to update status.',
        type: 'error',
      });
    }
  };

  const handleDeleteTask = async (taskId, title) => {
    if (!window.confirm(`Are you sure you want to delete task "${title}"?`)) {
      return;
    }

    try {
      await taskApi.deleteTask(taskId);
      setToast({ message: 'Task deleted successfully.', type: 'success' });
      setTasks((prev) => prev.filter((t) => t.id !== taskId));
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to delete task.',
        type: 'error',
      });
    }
  };

  const isOverdue = (deadline, status) => {
    if (!deadline || status?.toLowerCase() === 'done') return false;
    return new Date(deadline) < new Date();
  };

  return (
    <div className="container-fluid p-0">
      <Toast
        message={toast.message}
        type={toast.type}
        onClose={() => setToast({ message: '', type: 'success' })}
      />

      {/* Page Header */}
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-4">
        <div>
          <h3 className="fw-bold mb-1">Tasks</h3>
          <p className="text-muted small mb-0">
            {isAdmin && 'All organization tasks across all teams'}
            {isManager && 'Tasks managed within your team'}
            {!isAdmin && !isManager && 'Tasks assigned specifically to you'}
          </p>
        </div>
        {(isAdmin || isManager) && (
          <Link to="/tasks/create" className="btn btn-primary d-flex align-items-center gap-2">
            <i className="bi bi-plus-lg"></i>
            <span>Create Task</span>
          </Link>
        )}
      </div>

      {/* Filter and Search Bar */}
      <div className="card bg-white border-0 shadow-sm mb-4">
        <div className="card-body p-3">
          <form onSubmit={handleSearchSubmit} className="row g-2 align-items-end">
            {/* Title Search */}
            <div className="col-12 col-md-4">
              <label className="form-label small fw-semibold text-muted mb-1">Search Title</label>
              <div className="input-group input-group-sm">
                <input
                  type="text"
                  className="form-control"
                  placeholder="Search by keyword..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
                <button type="submit" className="btn btn-outline-secondary">
                  <i className="bi bi-search"></i>
                </button>
              </div>
            </div>

            {/* Status Filter */}
            <div className="col-6 col-md-2">
              <label className="form-label small fw-semibold text-muted mb-1">Status</label>
              <select
                className="form-select form-select-sm"
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="">All Statuses</option>
                <option value="To Do">To Do</option>
                <option value="In Progress">In Progress</option>
                <option value="Done">Done</option>
              </select>
            </div>

            {/* Priority Filter */}
            <div className="col-6 col-md-2">
              <label className="form-label small fw-semibold text-muted mb-1">Priority</label>
              <select
                className="form-select form-select-sm"
                value={priorityFilter}
                onChange={(e) => setPriorityFilter(e.target.value)}
              >
                <option value="">All Priorities</option>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </div>

            {/* Deadline Filter */}
            <div className="col-6 col-md-2">
              <label className="form-label small fw-semibold text-muted mb-1">Due By</label>
              <input
                type="date"
                className="form-control form-select-sm"
                value={deadlineFilter}
                onChange={(e) => setDeadlineFilter(e.target.value)}
              />
            </div>

            {/* Reset Filter Button */}
            <div className="col-6 col-md-2 d-flex">
              <button
                type="button"
                className="btn btn-sm btn-outline-secondary w-100"
                onClick={handleResetFilters}
              >
                <i className="bi bi-arrow-counterclockwise me-1"></i>
                Reset
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Task List Table */}
      <div className="card bg-white border-0 shadow-sm">
        <div className="card-body p-0">
          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading tasks...</span>
              </div>
            </div>
          ) : tasks.length === 0 ? (
            <div className="text-center py-5 text-muted">
              <i className="bi bi-clipboard-x fs-1 d-block mb-2"></i>
              <p className="mb-0">No tasks match your criteria.</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>Task</th>
                    <th>Team</th>
                    <th>Assignee</th>
                    <th>Priority</th>
                    <th>Status</th>
                    <th>Deadline</th>
                    <th>Comments</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {tasks.map((task) => (
                    <tr key={task.id}>
                      <td style={{ minWidth: '220px' }}>
                        <Link
                          to={`/tasks/${task.id}`}
                          className="fw-semibold text-dark text-decoration-none d-block"
                        >
                          {task.title}
                        </Link>
                        {task.description && (
                          <div className="text-muted small text-truncate" style={{ maxWidth: '280px' }}>
                            {task.description}
                          </div>
                        )}
                      </td>
                      <td className="small text-muted">{task.teamName}</td>
                      <td>
                        {task.assignedToUserName ? (
                          <span className="badge bg-light text-dark border">
                            <i className="bi bi-person me-1"></i>
                            {task.assignedToUserName}
                          </span>
                        ) : (
                          <span className="text-muted small fst-italic">Unassigned</span>
                        )}
                      </td>
                      <td>
                        <PriorityBadge priority={task.priority} />
                      </td>
                      <td>
                        {/* Status dropdown for quick status update */}
                        <select
                          className="form-select form-select-sm py-0 ps-2 pe-4"
                          style={{ width: '130px', fontSize: '0.8rem' }}
                          value={task.status}
                          onChange={(e) => handleStatusChange(task.id, e.target.value)}
                        >
                          <option value="To Do">To Do</option>
                          <option value="In Progress">In Progress</option>
                          <option value="Done">Done</option>
                        </select>
                      </td>
                      <td className="small">
                        {task.deadline ? (
                          <span
                            className={
                              isOverdue(task.deadline, task.status)
                                ? 'text-danger fw-bold d-flex align-items-center gap-1'
                                : 'text-muted'
                            }
                          >
                            {isOverdue(task.deadline, task.status) && (
                              <i className="bi bi-exclamation-circle-fill"></i>
                            )}
                            {new Date(task.deadline).toLocaleDateString()}
                          </span>
                        ) : (
                          <span className="text-muted">-</span>
                        )}
                      </td>
                      <td>
                        <span className="badge bg-light text-secondary border">
                          <i className="bi bi-chat-left-text me-1"></i>
                          {task.commentsCount}
                        </span>
                      </td>
                      <td className="text-end">
                        <div className="btn-group btn-group-sm">
                          <Link
                            to={`/tasks/${task.id}`}
                            className="btn btn-outline-secondary"
                            title="View Details"
                          >
                            <i className="bi bi-eye"></i>
                          </Link>
                          {(isAdmin || isManager) && (
                            <>
                              <Link
                                to={`/tasks/${task.id}/edit`}
                                className="btn btn-outline-primary"
                                title="Edit Task"
                              >
                                <i className="bi bi-pencil"></i>
                              </Link>
                              <button
                                type="button"
                                className="btn btn-outline-danger"
                                title="Delete Task"
                                onClick={() => handleDeleteTask(task.id, task.title)}
                              >
                                <i className="bi bi-trash"></i>
                              </button>
                            </>
                          )}
                        </div>
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
