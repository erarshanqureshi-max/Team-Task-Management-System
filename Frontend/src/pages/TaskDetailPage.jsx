import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { taskApi } from '../api/taskApi';
import { commentApi } from '../api/commentApi';
import { teamApi } from '../api/teamApi';
import { StatusBadge } from '../components/StatusBadge';
import { PriorityBadge } from '../components/PriorityBadge';
import { Toast } from '../components/Toast';

export const TaskDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, isAdmin, isManager } = useAuth();

  const [task, setTask] = useState(null);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [teamMembers, setTeamMembers] = useState([]);
  const [selectedAssignee, setSelectedAssignee] = useState('');
  const [loading, setLoading] = useState(true);
  const [submittingComment, setSubmittingComment] = useState(false);
  const [toast, setToast] = useState({ message: '', type: 'success' });

  const fetchTaskAndComments = async () => {
    try {
      const [taskRes, commentsRes] = await Promise.all([
        taskApi.getTaskById(id),
        commentApi.getByTask(id),
      ]);

      if (taskRes.success && taskRes.data) {
        setTask(taskRes.data);
        setSelectedAssignee(taskRes.data.assignedToUserId ? taskRes.data.assignedToUserId.toString() : '');

        // If admin/manager, fetch team members to allow quick reassignment
        if (isAdmin || isManager) {
          try {
            const membersRes = await teamApi.getMembers(taskRes.data.teamId);
            if (membersRes.success && membersRes.data) {
              setTeamMembers(membersRes.data);
            }
          } catch (e) {
            // Silently ignore
          }
        }
      }

      if (commentsRes.success && commentsRes.data) {
        setComments(commentsRes.data);
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to load task details.',
        type: 'error',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTaskAndComments();
  }, [id]);

  const handleStatusChange = async (newStatus) => {
    try {
      const res = await taskApi.updateStatus(id, newStatus);
      if (res.success && res.data) {
        setTask(res.data);
        setToast({ message: `Status updated to ${newStatus}`, type: 'success' });
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to update status.',
        type: 'error',
      });
    }
  };

  const handleAssignChange = async (e) => {
    const newUserId = e.target.value;
    setSelectedAssignee(newUserId);

    try {
      const payloadId = newUserId ? parseInt(newUserId, 10) : null;
      const res = await taskApi.assignTask(id, payloadId);
      if (res.success && res.data) {
        setTask(res.data);
        setToast({ message: 'Assignee updated successfully.', type: 'success' });
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to update assignment.',
        type: 'error',
      });
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim()) return;

    setSubmittingComment(true);
    try {
      const res = await commentApi.create(id, newComment.trim());
      if (res.success && res.data) {
        setComments((prev) => [...prev, res.data]);
        setNewComment('');
        setToast({ message: 'Comment posted.', type: 'success' });
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to add comment.',
        type: 'error',
      });
    } finally {
      setSubmittingComment(false);
    }
  };

  const handleDeleteTask = async () => {
    if (!window.confirm('Are you sure you want to delete this task?')) return;

    try {
      await taskApi.deleteTask(id);
      navigate('/tasks');
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to delete task.',
        type: 'error',
      });
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading task...</span>
        </div>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="alert alert-warning" role="alert">
        Task not found or access denied.
      </div>
    );
  }

  const isOverdue = task.deadline && task.status !== 'Done' && new Date(task.deadline) < new Date();

  return (
    <div className="container-fluid p-0">
      <Toast
        message={toast.message}
        type={toast.type}
        onClose={() => setToast({ message: '', type: 'success' })}
      />

      {/* Header and Back navigation */}
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-4">
        <div className="d-flex align-items-center gap-2">
          <Link to="/tasks" className="btn btn-outline-secondary btn-sm">
            <i className="bi bi-arrow-left"></i>
          </Link>
          <div>
            <h3 className="fw-bold mb-0">{task.title}</h3>
            <span className="text-muted small">Task #{task.id} &bull; Team: {task.teamName}</span>
          </div>
        </div>

        {(isAdmin || isManager) && (
          <div className="d-flex gap-2">
            <Link to={`/tasks/${id}/edit`} className="btn btn-outline-primary btn-sm d-flex align-items-center gap-1">
              <i className="bi bi-pencil"></i>
              <span>Edit</span>
            </Link>
            <button
              type="button"
              className="btn btn-outline-danger btn-sm d-flex align-items-center gap-1"
              onClick={handleDeleteTask}
            >
              <i className="bi bi-trash"></i>
              <span>Delete</span>
            </button>
          </div>
        )}
      </div>

      <div className="row g-4">
        {/* Main Content (Left / Middle) */}
        <div className="col-12 col-lg-8">
          {/* Details Card */}
          <div className="card bg-white border-0 shadow-sm mb-4">
            <div className="card-body p-4">
              <h6 className="fw-bold text-uppercase text-muted small mb-3">Description</h6>
              {task.description ? (
                <p className="text-dark" style={{ whiteSpace: 'pre-line', lineHeight: '1.6' }}>
                  {task.description}
                </p>
              ) : (
                <p className="text-muted fst-italic">No description provided for this task.</p>
              )}

              {/* Status Switcher Quick Bar */}
              <div className="mt-4 pt-3 border-top">
                <span className="fw-semibold small d-block mb-2 text-muted">Update Status:</span>
                <div className="btn-group" role="group">
                  <button
                    type="button"
                    className={`btn btn-sm ${task.status === 'To Do' ? 'btn-secondary' : 'btn-outline-secondary'}`}
                    onClick={() => handleStatusChange('To Do')}
                  >
                    To Do
                  </button>
                  <button
                    type="button"
                    className={`btn btn-sm ${task.status === 'In Progress' ? 'btn-primary' : 'btn-outline-primary'}`}
                    onClick={() => handleStatusChange('In Progress')}
                  >
                    In Progress
                  </button>
                  <button
                    type="button"
                    className={`btn btn-sm ${task.status === 'Done' ? 'btn-success' : 'btn-outline-success'}`}
                    onClick={() => handleStatusChange('Done')}
                  >
                    Done
                  </button>
                </div>
              </div>
            </div>
          </div>

          {/* Comments Card */}
          <div className="card bg-white border-0 shadow-sm">
            <div className="card-header bg-white border-bottom py-3">
              <h5 className="mb-0 fw-bold d-flex align-items-center gap-2">
                <i className="bi bi-chat-dots text-primary"></i>
                <span>Comments ({comments.length})</span>
              </h5>
            </div>
            <div className="card-body p-4">
              {/* Add Comment Form */}
              <form onSubmit={handleAddComment} className="mb-4">
                <div className="mb-2">
                  <textarea
                    rows="3"
                    className="form-control"
                    placeholder="Write a comment or status update..."
                    value={newComment}
                    onChange={(e) => setNewComment(e.target.value)}
                    required
                  ></textarea>
                </div>
                <div className="d-flex justify-content-end">
                  <button
                    type="submit"
                    className="btn btn-primary btn-sm d-flex align-items-center gap-1"
                    disabled={submittingComment || !newComment.trim()}
                  >
                    {submittingComment ? (
                      <span>Posting...</span>
                    ) : (
                      <>
                        <i className="bi bi-send"></i>
                        <span>Post Comment</span>
                      </>
                    )}
                  </button>
                </div>
              </form>

              {/* Comments Thread */}
              {comments.length === 0 ? (
                <div className="text-center py-4 text-muted small">
                  No comments yet. Start the conversation!
                </div>
              ) : (
                <div className="d-flex flex-column gap-3">
                  {comments.map((c) => (
                    <div key={c.id} className="p-3 bg-light rounded-3">
                      <div className="d-flex justify-content-between align-items-center mb-1">
                        <div className="fw-semibold text-dark small d-flex align-items-center gap-2">
                          <i className="bi bi-person-circle text-primary"></i>
                          <span>{c.userName}</span>
                          <span className="text-muted fw-normal" style={{ fontSize: '0.75rem' }}>
                            ({c.userEmail})
                          </span>
                        </div>
                        <span className="text-muted" style={{ fontSize: '0.75rem' }}>
                          {new Date(c.createdAt).toLocaleString()}
                        </span>
                      </div>
                      <div className="text-dark small mt-1" style={{ whiteSpace: 'pre-line' }}>
                        {c.content}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Sidebar Info (Right) */}
        <div className="col-12 col-lg-4">
          <div className="card bg-white border-0 shadow-sm p-3">
            <h6 className="fw-bold text-uppercase text-muted small mb-3 border-bottom pb-2">
              Task Meta
            </h6>

            <div className="mb-3">
              <span className="text-muted small d-block">Status</span>
              <div className="mt-1">
                <StatusBadge status={task.status} />
              </div>
            </div>

            <div className="mb-3">
              <span className="text-muted small d-block">Priority</span>
              <div className="mt-1">
                <PriorityBadge priority={task.priority} />
              </div>
            </div>

            <div className="mb-3">
              <span className="text-muted small d-block">Assignee</span>
              {(isAdmin || isManager) ? (
                <select
                  className="form-select form-select-sm mt-1"
                  value={selectedAssignee}
                  onChange={handleAssignChange}
                >
                  <option value="">-- Unassigned --</option>
                  {teamMembers.map((m) => (
                    <option key={m.userId} value={m.userId}>
                      {m.name}
                    </option>
                  ))}
                </select>
              ) : (
                <div className="fw-semibold small mt-1">
                  {task.assignedToUserName || 'Unassigned'}
                </div>
              )}
            </div>

            <div className="mb-3">
              <span className="text-muted small d-block">Team</span>
              <div className="fw-semibold small mt-1">
                <Link to={`/teams/${task.teamId}`} className="text-decoration-none">
                  {task.teamName}
                </Link>
              </div>
            </div>

            <div className="mb-3">
              <span className="text-muted small d-block">Deadline</span>
              <div className="fw-semibold small mt-1">
                {task.deadline ? (
                  <span className={isOverdue ? 'text-danger fw-bold' : 'text-dark'}>
                    {new Date(task.deadline).toLocaleDateString()}
                    {isOverdue && ' (Overdue)'}
                  </span>
                ) : (
                  <span className="text-muted">None</span>
                )}
              </div>
            </div>

            <div className="mb-3">
              <span className="text-muted small d-block">Created By</span>
              <div className="fw-semibold small mt-1">{task.createdByUserName}</div>
            </div>

            <div className="mb-3">
              <span className="text-muted small d-block">Created At</span>
              <div className="small text-muted mt-1">{new Date(task.createdAt).toLocaleString()}</div>
            </div>

            {task.updatedAt && (
              <div>
                <span className="text-muted small d-block">Last Updated</span>
                <div className="small text-muted mt-1">{new Date(task.updatedAt).toLocaleString()}</div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
