import { Fish } from './fish';

export interface CartItem {
  id: number;
  fishId: number;
  name: string;
  quantity: number;
  price: number;
  fish?: Fish;
}

export interface Cart {
  id: number;
  userId: string;
  items: CartItem[];
  totalAmount: number;
  createdAt: string;
  updatedAt: string;
}