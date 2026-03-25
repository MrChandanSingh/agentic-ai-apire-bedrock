import axios from 'axios';
import { Fish } from '../types/fish';

const API_BASE_URL = '/api';

export const fishService = {
  getAllFish: async (): Promise<Fish[]> => {
    const response = await axios.get(`${API_BASE_URL}/fish`);
    return response.data;
  },

  getFishById: async (id: number): Promise<Fish> => {
    const response = await axios.get(`${API_BASE_URL}/fish/${id}`);
    return response.data;
  },

  getFishBySpecies: async (species: string): Promise<Fish[]> => {
    const response = await axios.get(`${API_BASE_URL}/fish/species/${species}`);
    return response.data;
  }
};