import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';

import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { DashboardPage } from './pages/DashboardPage';
import { TaskListPage } from './pages/TaskListPage';
import { TaskCreatePage } from './pages/TaskCreatePage';
import { TaskDetailPage } from './pages/TaskDetailPage';
import { TaskEditPage } from './pages/TaskEditPage';
import { TeamsPage } from './pages/TeamsPage';
import { TeamDetailPage } from './pages/TeamDetailPage';
import { UsersPage } from './pages/UsersPage';
import { NotificationsPage } from './pages/NotificationsPage';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Protected Routes inside Layout */}
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            
            {/* Tasks Routes */}
            <Route path="tasks" element={<TaskListPage />} />
            <Route
              path="tasks/create"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                  <TaskCreatePage />
                </ProtectedRoute>
              }
            />
            <Route path="tasks/:id" element={<TaskDetailPage />} />
            <Route
              path="tasks/:id/edit"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                  <TaskEditPage />
                </ProtectedRoute>
              }
            />

            {/* Teams Routes */}
            <Route path="teams" element={<TeamsPage />} />
            <Route path="teams/:id" element={<TeamDetailPage />} />

            {/* Users Directory */}
            <Route
              path="users"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                  <UsersPage />
                </ProtectedRoute>
              }
            />

            {/* Notifications */}
            <Route path="notifications" element={<NotificationsPage />} />
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
