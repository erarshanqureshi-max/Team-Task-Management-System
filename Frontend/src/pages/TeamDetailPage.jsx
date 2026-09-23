import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { teamApi } from '../api/teamApi';
import { userApi } from '../api/userApi';
import { taskApi } from '../api/taskApi';
import { StatusBadge } from '../components/StatusBadge';
import { PriorityBadge } from '../components/PriorityBadge';
import { Toast } from '../components/Toast';

export const TeamDetailPage = () => {
  const { id } = useParams();
  const { user, isAdmin, isManager } = useAuth();

  const [team, setTeam] = useState(null);
  const [allUsers, setAllUsers] = useState([]);
  const [teamTasks, setTeamTasks] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [loading, setLoading] = useState(true);
  const [showAddMemberModal, setShowAddMemberModal] = useState(false);
  const [toast, setToast] = useState({ message: '', type: 'success' });

  const fetchTeamDetails = async () => {
    try {
      const res = await teamApi.getById(id);
      if (res.success && res.data) {
        setTeam(res.data);
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to load team details.',
        type: 'error',
      });
    } finally {
      setLoading(false);
    }
  };

  const fetchTasksForTeam = async () => {
    try {
      const res = await taskApi.getTasks();
      if (res.success && res.data) {
        setTeamTasks(res.data.filter((t) => t.teamId === parseInt(id, 10)));
      }
    } catch (err) {
      // Ignore
    }
  };

  const fetchUsersForAdding = async () => {
    try {
      const res = await userApi.getAll();
      if (res.success && res.data) {
        setAllUsers(res.data);
        if (res.data.length > 0) {
          setSelectedUserId(res.data[0].id);
        }
      }
    } catch (err) {
      // Ignore
    }
  };

  useEffect(() => {
    fetchTeamDetails();
    fetchTasksForTeam();
    if (isAdmin || isManager) {
      fetchUsersForAdding();
    }
  }, [id, isAdmin, isManager]);

  const canManageMembers = isAdmin || (isManager && team?.managerId === user?.id);

  const handleAddMember = async (e) => {
    e.preventDefault();
    if (!selectedUserId) return;

    try {
      await teamApi.addMember(id, parseInt(selectedUserId, 10));
      setToast({ message: 'Member added to team.', type: 'success' });
      setShowAddMemberModal(false);
      fetchTeamDetails();
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to add member.',
        type: 'error',
      });
    }
  };

  const handleRemoveMember = async (userId, memberName) => {
    if (!window.confirm(`Remove ${memberName} from this team?`)) return;

    try {
      await teamApi.removeMember(id, userId);
      setToast({ message: 'Member removed from team.', type: 'success' });
      fetchTeamDetails();
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to remove member.',
        type: 'error',
      });
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading team...</span>
        </div>
      </div>
    );
  }

  if (!team) {
    return (
      <div className="alert alert-warning" role="alert">
        Team not found or access denied.
      </div>
    );
  }

  // Filter out users who are already in this team
  const currentMemberIds = new Set(team.members?.map((m) => m.userId) || []);
  const availableUsers = allUsers.filter((u) => !currentMemberIds.has(u.id));

  return (
    <div className="container-fluid p-0">
      <Toast
        message={toast.message}
        type={toast.type}
        onClose={() => setToast({ message: '', type: 'success' })}
      />

      <div className="d-flex align-items-center gap-2 mb-4">
        <Link to="/teams" className="btn btn-outline-secondary btn-sm">
          <i className="bi bi-arrow-left"></i>
        </Link>
        <div>
          <h3 className="fw-bold mb-0">{team.name}</h3>
          <span className="text-muted small">Managed by {team.managerName}</span>
        </div>
      </div>

      <div className="row g-4 mb-4">
        {/* Team Details & Member List */}
        <div className="col-12 col-lg-5">
          <div className="card bg-white border-0 shadow-sm p-4 mb-4">
            <h6 className="fw-bold text-uppercase text-muted small mb-3 border-bottom pb-2">
              Team Information
            </h6>
            <div className="mb-3">
              <span className="text-muted small d-block">Description</span>
              <p className="text-dark small mt-1">{team.description || 'No description provided.'}</p>
            </div>
            <div className="mb-3">
              <span className="text-muted small d-block">Manager</span>
              <div className="fw-semibold text-dark small">{team.managerName} ({team.managerEmail})</div>
            </div>
            <div>
              <span className="text-muted small d-block">Formed On</span>
              <div className="small text-muted">{new Date(team.createdAt).toLocaleDateString()}</div>
            </div>
          </div>

          {/* Members Card */}
          <div className="card bg-white border-0 shadow-sm">
            <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
              <h6 className="mb-0 fw-bold">Team Members ({team.members?.length || 0})</h6>
              {canManageMembers && (
                <button
                  type="button"
                  className="btn btn-sm btn-outline-primary"
                  onClick={() => setShowAddMemberModal(true)}
                >
                  <i className="bi bi-person-plus me-1"></i> Add Member
                </button>
              )}
            </div>
            <div className="card-body p-0">
              <ul className="list-group list-group-flush">
                {team.members?.map((m) => (
                  <li key={m.userId} className="list-group-item d-flex justify-content-between align-items-center px-4 py-3">
                    <div className="d-flex align-items-center gap-2">
                      <div
                        className="rounded-circle bg-light border d-flex align-items-center justify-content-center fw-bold text-secondary"
                        style={{ width: '32px', height: '32px', fontSize: '0.8rem' }}
                      >
                        {m.name.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <div className="fw-semibold text-dark small">{m.name}</div>
                        <div className="text-muted" style={{ fontSize: '0.75rem' }}>{m.email}</div>
                      </div>
                    </div>

                    <div className="d-flex align-items-center gap-2">
                      <span className="badge bg-light text-secondary border" style={{ fontSize: '0.7rem' }}>
                        {m.role}
                      </span>
                      {canManageMembers && m.userId !== team.managerId && (
                        <button
                          type="button"
                          className="btn btn-sm btn-link text-danger p-0"
                          title="Remove from team"
                          onClick={() => handleRemoveMember(m.userId, m.name)}
                        >
                          <i className="bi bi-x-circle fs-6"></i>
                        </button>
                      )}
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>

        {/* Team Tasks */}
        <div className="col-12 col-lg-7">
          <div className="card bg-white border-0 shadow-sm">
            <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
              <h6 className="mb-0 fw-bold">Team Tasks ({teamTasks.length})</h6>
              {canManageMembers && (
                <Link to="/tasks/create" className="btn btn-sm btn-primary">
                  <i className="bi bi-plus-lg me-1"></i> New Task
                </Link>
              )}
            </div>
            <div className="card-body p-0">
              {teamTasks.length === 0 ? (
                <div className="text-center py-5 text-muted small">
                  No tasks assigned to this team yet.
                </div>
              ) : (
                <div className="table-responsive">
                  <table className="table table-hover align-middle mb-0">
                    <thead>
                      <tr>
                        <th>Title</th>
                        <th>Assignee</th>
                        <th>Priority</th>
                        <th>Status</th>
                        <th>Deadline</th>
                      </tr>
                    </thead>
                    <tbody>
                      {teamTasks.map((t) => (
                        <tr key={t.id}>
                          <td>
                            <Link to={`/tasks/${t.id}`} className="fw-semibold text-dark text-decoration-none">
                              {t.title}
                            </Link>
                          </td>
                          <td className="small">
                            {t.assignedToUserName || <span className="text-muted fst-italic">Unassigned</span>}
                          </td>
                          <td>
                            <PriorityBadge priority={t.priority} />
                          </td>
                          <td>
                            <StatusBadge status={t.status} />
                          </td>
                          <td className="small text-muted">
                            {t.deadline ? new Date(t.deadline).toLocaleDateString() : '-'}
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
      </div>

      {/* Add Member Modal */}
      {showAddMemberModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content border-0 shadow">
              <div className="modal-header border-bottom">
                <h5 className="modal-title fw-bold">Add Member to {team.name}</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowAddMemberModal(false)}
                ></button>
              </div>
              <form onSubmit={handleAddMember}>
                <div className="modal-body p-4">
                  {availableUsers.length === 0 ? (
                    <div className="alert alert-info py-2 small">
                      All existing users are already assigned to this team.
                    </div>
                  ) : (
                    <div>
                      <label className="form-label small fw-semibold">Select User</label>
                      <select
                        className="form-select"
                        value={selectedUserId}
                        onChange={(e) => setSelectedUserId(e.target.value)}
                        required
                      >
                        {availableUsers.map((u) => (
                          <option key={u.id} value={u.id}>
                            {u.name} ({u.email}) - Role: {u.role}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>
                <div className="modal-footer border-top">
                  <button
                    type="button"
                    className="btn btn-outline-secondary"
                    onClick={() => setShowAddMemberModal(false)}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={availableUsers.length === 0}
                  >
                    Add Member
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
