import axiosClient from './axiosClient';

export const dashboardApi = {
  getAdminStats: async () => {
    const response = await axiosClient.get('/dashboard/admin');
    return response.data;
  },
  getManagerStats: async () => {
    const response = await axiosClient.get('/dashboard/manager');
    return response.data;
  },
  getUserStats: async () => {
    const response = await axiosClient.get('/dashboard/user');
    return response.data;
  },
};
