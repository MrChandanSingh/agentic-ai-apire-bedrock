import axios from 'axios';
import { Cart, CartItem } from '../types/cart';

const API_BASE_URL = '/api';

export const cartService = {
  getCart: async (): Promise<Cart> => {
    const response = await axios.get(`${API_BASE_URL}/cart`);
    return response.data;
  },

  addItem: async (item: CartItem): Promise<Cart> => {
    const response = await axios.post(`${API_BASE_URL}/cart/items`, item);
    return response.data;
  },

  updateItem: async (itemId: number, item: CartItem): Promise<Cart> => {
    const response = await axios.put(`${API_BASE_URL}/cart/items/${itemId}`, item);
    return response.data;
  },

  removeItem: async (itemId: number): Promise<Cart> => {
    const response = await axios.delete(`${API_BASE_URL}/cart/items/${itemId}`);
    return response.data;
  },

  clearCart: async (): Promise<void> => {
    await axios.post(`${API_BASE_URL}/cart/clear`);
  }
};