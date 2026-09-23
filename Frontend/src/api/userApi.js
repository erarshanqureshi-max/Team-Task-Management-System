import axiosClient from './axiosClient';

export const userApi = {
  getAll: async () => {
    const response = await axiosClient.get('/users');
    return response.data;
  },
  getById: async (id) => {
    const response = await axiosClient.get(`/users/${id}`);
    return response.data;
  },
};
