import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchCart, updateCartItem, removeFromCart, clearCart } from '../store/cartSlice';

export const Cart: React.FC = () => {
  const dispatch = useAppDispatch();
  const { cart, loading, error } = useAppSelector((state) => state.cart);

  useEffect(() => {
    dispatch(fetchCart());
  }, [dispatch]);

  const handleUpdateQuantity = (itemId: number, currentItem: any, newQuantity: number) => {
    if (newQuantity > 0) {
      dispatch(updateCartItem({
        itemId,
        item: { ...currentItem, quantity: newQuantity }
      }));
    }
  };

  const handleRemoveItem = (itemId: number) => {
    dispatch(removeFromCart(itemId));
  };

  const handleClearCart = () => {
    dispatch(clearCart());
  };

  if (loading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!cart || cart.items.length === 0) {
    return <div className="text-center py-8">Your cart is empty</div>;
  }

  return (
    <div className="max-w-2xl mx-auto">
      <h2 className="text-2xl font-bold mb-4">Shopping Cart</h2>
      <div className="divide-y">
        {cart.items.map((item) => (
          <div key={item.id} className="py-4 flex items-center justify-between">
            <div className="flex items-center space-x-4">
              {item.fish && (
                <img 
                  src={item.fish.imageUrl} 
                  alt={item.name} 
                  className="w-16 h-16 object-cover rounded"
                />
              )}
              <div>
                <h3 className="font-medium">{item.name}</h3>
                <p className="text-gray-500">${item.price.toFixed(2)} each</p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => handleUpdateQuantity(item.id, item, item.quantity - 1)}
                  className="px-2 py-1 border rounded"
                >
                  -
                </button>
                <span>{item.quantity}</span>
                <button
                  onClick={() => handleUpdateQuantity(item.id, item, item.quantity + 1)}
                  className="px-2 py-1 border rounded"
                >
                  +
                </button>
              </div>
              <button
                onClick={() => handleRemoveItem(item.id)}
                className="text-red-600 hover:text-red-800"
              >
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>
      <div className="mt-8 space-y-4">
        <div className="flex justify-between text-xl font-semibold">
          <span>Total:</span>
          <span>${cart.totalAmount.toFixed(2)}</span>
        </div>
        <div className="flex justify-end space-x-4">
          <button
            onClick={handleClearCart}
            className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Clear Cart
          </button>
          <button
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Checkout
          </button>
        </div>
      </div>
    </div>
  );
};