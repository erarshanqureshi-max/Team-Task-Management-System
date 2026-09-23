import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { teamApi } from '../api/teamApi';
import { userApi } from '../api/userApi';
import { Toast } from '../components/Toast';

export const TeamsPage = () => {
  const { user, isAdmin, isManager } = useAuth();
  const [teams, setTeams] = useState([]);
  const [managers, setManagers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [toast, setToast] = useState({ message: '', type: 'success' });

  // New Team Form state
  const [teamForm, setTeamForm] = useState({
    name: '',
    description: '',
    managerId: '',
  });
  const [submitting, setSubmitting] = useState(false);

  const fetchTeams = async () => {
    setLoading(true);
    try {
      const res = await teamApi.getAll();
      if (res.success && res.data) {
        setTeams(res.data);
      }
    } catch (err) {
      setToast({ message: 'Failed to load teams.', type: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const fetchPotentialManagers = async () => {
    try {
      const res = await userApi.getAll();
      if (res.success && res.data) {
        // filter managers or admins
        const eligible = res.data.filter((u) => u.role === 'Manager' || u.role === 'Admin');
        setManagers(eligible.length > 0 ? eligible : res.data);
        if (eligible.length > 0) {
          setTeamForm((prev) => ({ ...prev, managerId: eligible[0].id }));
        }
      }
    } catch (err) {
      // Ignore
    }
  };

  useEffect(() => {
    fetchTeams();
    if (isAdmin) {
      fetchPotentialManagers();
    }
  }, [isAdmin]);

  const handleCreateTeam = async (e) => {
    e.preventDefault();
    if (!teamForm.name.trim()) return;

    setSubmitting(true);
    try {
      const payload = {
        name: teamForm.name.trim(),
        description: teamForm.description.trim() || null,
        managerId: parseInt(teamForm.managerId, 10),
      };

      const res = await teamApi.create(payload);
      if (res.success && res.data) {
        setTeams((prev) => [...prev, res.data]);
        setShowModal(false);
        setTeamForm({ name: '', description: '', managerId: managers[0]?.id || '' });
        setToast({ message: 'Team created successfully.', type: 'success' });
      }
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to create team.',
        type: 'error',
      });
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteTeam = async (teamId, teamName) => {
    if (!window.confirm(`Are you sure you want to delete team "${teamName}"? This will delete associated tasks.`)) {
      return;
    }

    try {
      await teamApi.delete(teamId);
      setTeams((prev) => prev.filter((t) => t.id !== teamId));
      setToast({ message: 'Team deleted successfully.', type: 'success' });
    } catch (err) {
      setToast({
        message: err.response?.data?.message || 'Failed to delete team.',
        type: 'error',
      });
    }
  };

  return (
    <div className="container-fluid p-0">
      <Toast
        message={toast.message}
        type={toast.type}
        onClose={() => setToast({ message: '', type: 'success' })}
      />

      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-4">
        <div>
          <h3 className="fw-bold mb-1">Teams</h3>
          <p className="text-muted small mb-0">
            {isAdmin ? 'Manage all organizational teams and assignments' : 'Teams and workspaces you belong to'}
          </p>
        </div>
        {isAdmin && (
          <button
            className="btn btn-primary d-flex align-items-center gap-2"
            onClick={() => setShowModal(true)}
          >
            <i className="bi bi-plus-lg"></i>
            <span>Create Team</span>
          </button>
        )}
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading teams...</span>
          </div>
        </div>
      ) : teams.length === 0 ? (
        <div className="card bg-white border-0 shadow-sm p-5 text-center text-muted">
          <i className="bi bi-people fs-1 d-block mb-2"></i>
          <p className="mb-0">No teams found.</p>
        </div>
      ) : (
        <div className="row g-3">
          {teams.map((t) => (
            <div key={t.id} className="col-12 col-md-6 col-xl-4">
              <div className="card bg-white border-0 shadow-sm h-100 stat-card">
                <div className="card-body p-4 d-flex flex-column">
                  <div className="d-flex justify-content-between align-items-start mb-2">
                    <h5 className="fw-bold text-dark mb-0">{t.name}</h5>
                    <span className="badge bg-primary bg-opacity-10 text-primary px-2 py-1">
                      {t.memberCount} Members
                    </span>
                  </div>

                  <p className="text-muted small flex-grow-1 mb-3" style={{ minHeight: '40px' }}>
                    {t.description || 'No description provided.'}
                  </p>

                  <div className="border-top pt-3 mt-auto">
                    <div className="d-flex align-items-center justify-content-between small text-muted mb-3">
                      <div>
                        <i className="bi bi-person-badge me-1"></i>
                        <span>Manager: </span>
                        <strong className="text-dark">{t.managerName}</strong>
                      </div>
                      <div>
                        <i className="bi bi-kanban me-1"></i>
                        <span>Tasks: </span>
                        <strong className="text-dark">{t.taskCount}</strong>
                      </div>
                    </div>

                    <div className="d-flex gap-2 justify-content-between">
                      <Link to={`/teams/${t.id}`} className="btn btn-sm btn-outline-primary flex-grow-1">
                        View Team & Members
                      </Link>
                      {isAdmin && (
                        <button
                          type="button"
                          className="btn btn-sm btn-outline-danger"
                          title="Delete Team"
                          onClick={() => handleDeleteTeam(t.id, t.name)}
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create Team Modal */}
      {showModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content border-0 shadow">
              <div className="modal-header border-bottom">
                <h5 className="modal-title fw-bold">Create New Team</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowModal(false)}
                ></button>
              </div>
              <form onSubmit={handleCreateTeam}>
                <div className="modal-body p-4">
                  <div className="mb-3">
                    <label className="form-label small fw-semibold">Team Name *</label>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="e.g. Frontend Engineering"
                      value={teamForm.name}
                      onChange={(e) => setTeamForm({ ...teamForm, name: e.target.value })}
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label small fw-semibold">Description</label>
                    <textarea
                      rows="3"
                      className="form-control"
                      placeholder="Objectives and scope of this team..."
                      value={teamForm.description}
                      onChange={(e) => setTeamForm({ ...teamForm, description: e.target.value })}
                    ></textarea>
                  </div>

                  <div className="mb-3">
                    <label className="form-label small fw-semibold">Designated Manager *</label>
                    <select
                      className="form-select"
                      value={teamForm.managerId}
                      onChange={(e) => setTeamForm({ ...teamForm, managerId: e.target.value })}
                      required
                    >
                      {managers.map((m) => (
                        <option key={m.id} value={m.id}>
                          {m.name} ({m.role}) - {m.email}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="modal-footer border-top">
                  <button
                    type="button"
                    className="btn btn-outline-secondary"
                    onClick={() => setShowModal(false)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? 'Creating...' : 'Create Team'}
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
