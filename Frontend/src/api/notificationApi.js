import axiosClient from './axiosClient';

export const notificationApi = {
  getAll: async () => {
    const response = await axiosClient.get('/notifications');
    return response.data;
  },
  markAsRead: async (id) => {
    const response = await axiosClient.patch(`/notifications/${id}/read`);
    return response.data;
  },
  markAllAsRead: async () => {
    const response = await axiosClient.patch('/notifications/read-all');
    return response.data;
  },
};
