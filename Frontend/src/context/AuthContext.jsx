import React, { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../api/authApi';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem('task_mgmt_token');
      const storedUser = localStorage.getItem('task_mgmt_user');

      if (storedToken && storedUser) {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      }
    } catch (err) {
      console.error('Failed to restore session from local storage', err);
      localStorage.removeItem('task_mgmt_token');
      localStorage.removeItem('task_mgmt_user');
    } finally {
      setLoading(false);
    }
  }, []);

  const login = async (email, password) => {
    const res = await authApi.login({ email, password });
    if (res.success && res.data) {
      const authData = res.data;
      const userInfo = {
        id: authData.id,
        name: authData.name,
        email: authData.email,
        role: authData.role,
        expiresAt: authData.expiresAt,
      };

      setToken(authData.token);
      setUser(userInfo);

      localStorage.setItem('task_mgmt_token', authData.token);
      localStorage.setItem('task_mgmt_user', JSON.stringify(userInfo));

      return userInfo;
    }
    throw new Error(res.message || 'Login failed');
  };

  const register = async (userData) => {
    const res = await authApi.register(userData);
    if (res.success && res.data) {
      const authData = res.data;
      const userInfo = {
        id: authData.id,
        name: authData.name,
        email: authData.email,
        role: authData.role,
        expiresAt: authData.expiresAt,
      };

      setToken(authData.token);
      setUser(userInfo);

      localStorage.setItem('task_mgmt_token', authData.token);
      localStorage.setItem('task_mgmt_user', JSON.stringify(userInfo));

      return userInfo;
    }
    throw new Error(res.message || 'Registration failed');
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('task_mgmt_token');
    localStorage.removeItem('task_mgmt_user');
  };

  const hasRole = (roles) => {
    if (!user || !user.role) return false;
    if (Array.isArray(roles)) {
      return roles.some((r) => r.toLowerCase() === user.role.toLowerCase());
    }
    return user.role.toLowerCase() === roles.toLowerCase();
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        login,
        register,
        logout,
        isAuthenticated: !!token && !!user,
        isAdmin: user?.role?.toLowerCase() === 'admin',
        isManager: user?.role?.toLowerCase() === 'manager',
        isUser: user?.role?.toLowerCase() === 'user',
        hasRole,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
