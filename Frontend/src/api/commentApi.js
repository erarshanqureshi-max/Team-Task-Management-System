import axiosClient from './axiosClient';

export const commentApi = {
  getByTask: async (taskId) => {
    const response = await axiosClient.get(`/tasks/${taskId}/comments`);
    return response.data;
  },
  create: async (taskId, content) => {
    const response = await axiosClient.post(`/tasks/${taskId}/comments`, { content });
    return response.data;
  },
};
