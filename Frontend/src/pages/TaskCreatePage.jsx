import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { taskApi } from '../api/taskApi';
import { teamApi } from '../api/teamApi';

export const TaskCreatePage = () => {
  const { user, isAdmin, isManager } = useAuth();
  const navigate = useNavigate();

  const [teams, setTeams] = useState([]);
  const [teamMembers, setTeamMembers] = useState([]);
  const [loadingTeams, setLoadingTeams] = useState(true);
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    teamId: '',
    assignedToUserId: '',
    priority: 'Medium',
    status: 'To Do',
    deadline: '',
  });

  // Load teams
  useEffect(() => {
    const fetchTeams = async () => {
      try {
        const res = await teamApi.getAll();
        if (res.success && res.data) {
          setTeams(res.data);
          if (res.data.length > 0) {
            const defaultTeamId = res.data[0].id;
            setFormData((prev) => ({ ...prev, teamId: defaultTeamId }));
            fetchMembersForTeam(defaultTeamId);
          }
        }
      } catch (err) {
        setError('Failed to load teams.');
      } finally {
        setLoadingTeams(false);
      }
    };

    fetchTeams();
  }, []);

  const fetchMembersForTeam = async (teamId) => {
    if (!teamId) {
      setTeamMembers([]);
      return;
    }
    setLoadingMembers(true);
    try {
      const res = await teamApi.getMembers(teamId);
      if (res.success && res.data) {
        setTeamMembers(res.data);
      }
    } catch (err) {
      setTeamMembers([]);
    } finally {
      setLoadingMembers(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));

    if (name === 'teamId') {
      fetchMembersForTeam(value);
      setFormData((prev) => ({ ...prev, assignedToUserId: '' }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!formData.title.trim()) {
      setError('Task title is required.');
      return;
    }

    if (!formData.teamId) {
      setError('A team must be selected.');
      return;
    }

    setSubmitting(true);

    try {
      const payload = {
        title: formData.title.trim(),
        description: formData.description.trim() || null,
        teamId: parseInt(formData.teamId, 10),
        assignedToUserId: formData.assignedToUserId ? parseInt(formData.assignedToUserId, 10) : null,
        priority: formData.priority,
        status: formData.status,
        deadline: formData.deadline ? new Date(formData.deadline).toISOString() : null,
      };

      const res = await taskApi.createTask(payload);
      if (res.success && res.data) {
        navigate(`/tasks/${res.data.id}`);
      }
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to create task.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingTeams) {
    return (
      <div className="d-flex justify-content-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading teams...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid p-0" style={{ maxWidth: '800px' }}>
      <div className="d-flex align-items-center gap-2 mb-4">
        <Link to="/tasks" className="btn btn-outline-secondary btn-sm">
          <i className="bi bi-arrow-left"></i>
        </Link>
        <h3 className="fw-bold mb-0">Create New Task</h3>
      </div>

      <div className="card bg-white border-0 shadow-sm">
        <div className="card-body p-4">
          {error && (
            <div className="alert alert-danger py-2 small" role="alert">
              <i className="bi bi-exclamation-triangle me-1"></i>
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            {/* Title */}
            <div className="mb-3">
              <label className="form-label fw-semibold small">Task Title *</label>
              <input
                type="text"
                name="title"
                className="form-control"
                placeholder="e.g. Implement User Authentication"
                value={formData.title}
                onChange={handleChange}
                required
              />
            </div>

            {/* Description */}
            <div className="mb-3">
              <label className="form-label fw-semibold small">Description</label>
              <textarea
                name="description"
                rows="4"
                className="form-control"
                placeholder="Provide detailed description of deliverables, acceptance criteria, or links..."
                value={formData.description}
                onChange={handleChange}
              ></textarea>
            </div>

            <div className="row g-3 mb-3">
              {/* Team selection */}
              <div className="col-12 col-md-6">
                <label className="form-label fw-semibold small">Team *</label>
                <select
                  name="teamId"
                  className="form-select"
                  value={formData.teamId}
                  onChange={handleChange}
                  required
                >
                  {teams.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name}
                    </option>
                  ))}
                </select>
                <div className="form-text small">Task will be scoped to this team.</div>
              </div>

              {/* Assignee selection */}
              <div className="col-12 col-md-6">
                <label className="form-label fw-semibold small">Assignee</label>
                <select
                  name="assignedToUserId"
                  className="form-select"
                  value={formData.assignedToUserId}
                  onChange={handleChange}
                  disabled={loadingMembers}
                >
                  <option value="">-- Unassigned --</option>
                  {teamMembers.map((m) => (
                    <option key={m.userId} value={m.userId}>
                      {m.name} ({m.email})
                    </option>
                  ))}
                </select>
                <div className="form-text small">
                  {loadingMembers ? 'Loading team members...' : 'Only members of the selected team can be assigned.'}
                </div>
              </div>
            </div>

            <div className="row g-3 mb-4">
              {/* Priority */}
              <div className="col-12 col-md-4">
                <label className="form-label fw-semibold small">Priority *</label>
                <select
                  name="priority"
                  className="form-select"
                  value={formData.priority}
                  onChange={handleChange}
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Urgent">Urgent</option>
                </select>
              </div>

              {/* Status */}
              <div className="col-12 col-md-4">
                <label className="form-label fw-semibold small">Initial Status *</label>
                <select
                  name="status"
                  className="form-select"
                  value={formData.status}
                  onChange={handleChange}
                >
                  <option value="To Do">To Do</option>
                  <option value="In Progress">In Progress</option>
                  <option value="Done">Done</option>
                </select>
              </div>

              {/* Deadline */}
              <div className="col-12 col-md-4">
                <label className="form-label fw-semibold small">Deadline</label>
                <input
                  type="date"
                  name="deadline"
                  className="form-control"
                  value={formData.deadline}
                  onChange={handleChange}
                />
              </div>
            </div>

            <div className="d-flex justify-content-end gap-2">
              <Link to="/tasks" className="btn btn-outline-secondary">
                Cancel
              </Link>
              <button
                type="submit"
                className="btn btn-primary d-flex align-items-center gap-2"
                disabled={submitting}
              >
                {submitting ? (
                  <>
                    <span className="spinner-border spinner-border-sm" role="status"></span>
                    <span>Creating...</span>
                  </>
                ) : (
                  <>
                    <i className="bi bi-check2"></i>
                    <span>Create Task</span>
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};
