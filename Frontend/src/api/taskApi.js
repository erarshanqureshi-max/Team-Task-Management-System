import axiosClient from './axiosClient';

export const taskApi = {
  getTasks: async (params = {}) => {
    const response = await axiosClient.get('/tasks', { params });
    return response.data;
  },
  getTaskById: async (id) => {
    const response = await axiosClient.get(`/tasks/${id}`);
    return response.data;
  },
  createTask: async (taskData) => {
    const response = await axiosClient.post('/tasks', taskData);
    return response.data;
  },
  updateTask: async (id, taskData) => {
    const response = await axiosClient.put(`/tasks/${id}`, taskData);
    return response.data;
  },
  deleteTask: async (id) => {
    const response = await axiosClient.delete(`/tasks/${id}`);
    return response.data;
  },
  updateStatus: async (id, status) => {
    const response = await axiosClient.patch(`/tasks/${id}/status`, { status });
    return response.data;
  },
  assignTask: async (id, assignedToUserId) => {
    const response = await axiosClient.patch(`/tasks/${id}/assign`, { assignedToUserId });
    return response.data;
  },
};
