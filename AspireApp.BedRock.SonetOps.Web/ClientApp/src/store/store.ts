import { configureStore } from '@reduxjs/toolkit';
import fishReducer from './fishSlice';
import cartReducer from './cartSlice';

export const store = configureStore({
  reducer: {
    fish: fishReducer,
    cart: cartReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;