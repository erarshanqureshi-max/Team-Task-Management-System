import axiosClient from './axiosClient';

export const teamApi = {
  getAll: async () => {
    const response = await axiosClient.get('/teams');
    return response.data;
  },
  getById: async (id) => {
    const response = await axiosClient.get(`/teams/${id}`);
    return response.data;
  },
  create: async (data) => {
    const response = await axiosClient.post('/teams', data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await axiosClient.put(`/teams/${id}`, data);
    return response.data;
  },
  delete: async (id) => {
    const response = await axiosClient.delete(`/teams/${id}`);
    return response.data;
  },
  getMembers: async (id) => {
    const response = await axiosClient.get(`/teams/${id}/members`);
    return response.data;
  },
  addMember: async (id, userId) => {
    const response = await axiosClient.post(`/teams/${id}/members`, { userId });
    return response.data;
  },
  removeMember: async (id, userId) => {
    const response = await axiosClient.delete(`/teams/${id}/members/${userId}`);
    return response.data;
  },
};
