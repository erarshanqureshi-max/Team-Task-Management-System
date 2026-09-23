import React, { useState, useEffect } from 'react';
import { userApi } from '../api/userApi';
import { Toast } from '../components/Toast';

export const UsersPage = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [toast, setToast] = useState({ message: '', type: 'success' });

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const res = await userApi.getAll();
        if (res.success && res.data) {
          setUsers(res.data);
        }
      } catch (err) {
        setToast({ message: 'Failed to load user directory.', type: 'error' });
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, []);

  const getRoleBadge = (role) => {
    switch (role?.toLowerCase()) {
      case 'admin':
        return <span className="badge bg-danger">Admin</span>;
      case 'manager':
        return <span className="badge bg-warning text-dark">Manager</span>;
      default:
        return <span className="badge bg-info text-dark">User</span>;
    }
  };

  const filteredUsers = users.filter((u) => {
    const matchesSearch =
      u.name.toLowerCase().includes(search.toLowerCase()) ||
      u.email.toLowerCase().includes(search.toLowerCase());
    const matchesRole = roleFilter ? u.role.toLowerCase() === roleFilter.toLowerCase() : true;
    return matchesSearch && matchesRole;
  });

  return (
    <div className="container-fluid p-0">
      <Toast
        message={toast.message}
        type={toast.type}
        onClose={() => setToast({ message: '', type: 'success' })}
      />

      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-4">
        <div>
          <h3 className="fw-bold mb-1">User Directory</h3>
          <p className="text-muted small mb-0">Organization members, roles, and assigned teams</p>
        </div>
      </div>

      {/* Filter Bar */}
      <div className="card bg-white border-0 shadow-sm mb-4">
        <div className="card-body p-3">
          <div className="row g-2">
            <div className="col-12 col-md-6">
              <div className="input-group input-group-sm">
                <span className="input-group-text bg-white">
                  <i className="bi bi-search text-muted"></i>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Search user by name or email..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
            </div>
            <div className="col-6 col-md-3">
              <select
                className="form-select form-select-sm"
                value={roleFilter}
                onChange={(e) => setRoleFilter(e.target.value)}
              >
                <option value="">All Roles</option>
                <option value="Admin">Admin</option>
                <option value="Manager">Manager</option>
                <option value="User">User</option>
              </select>
            </div>
            <div className="col-6 col-md-3">
              <button
                type="button"
                className="btn btn-sm btn-outline-secondary w-100"
                onClick={() => {
                  setSearch('');
                  setRoleFilter('');
                }}
              >
                Reset
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Users Table */}
      <div className="card bg-white border-0 shadow-sm">
        <div className="card-body p-0">
          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading users...</span>
              </div>
            </div>
          ) : filteredUsers.length === 0 ? (
            <div className="text-center py-5 text-muted small">No users found.</div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>User</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Assigned Team</th>
                    <th>Joined</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredUsers.map((u) => (
                    <tr key={u.id}>
                      <td>
                        <div className="d-flex align-items-center gap-2">
                          <div
                            className="rounded-circle bg-primary bg-opacity-10 text-primary d-flex align-items-center justify-content-center fw-bold"
                            style={{ width: '34px', height: '34px', fontSize: '0.85rem' }}
                          >
                            {u.name.charAt(0).toUpperCase()}
                          </div>
                          <span className="fw-semibold text-dark">{u.name}</span>
                        </div>
                      </td>
                      <td className="small text-muted">{u.email}</td>
                      <td>{getRoleBadge(u.role)}</td>
                      <td className="small">
                        {u.teamName ? (
                          <span className="badge bg-light text-dark border">
                            <i className="bi bi-people me-1"></i>
                            {u.teamName}
                          </span>
                        ) : (
                          <span className="text-muted fst-italic">None</span>
                        )}
                      </td>
                      <td className="small text-muted">
                        {new Date(u.createdAt).toLocaleDateString()}
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
