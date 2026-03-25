import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchAllFish } from '../store/fishSlice';
import { addToCart } from '../store/cartSlice';

export const FishList: React.FC = () => {
  const dispatch = useAppDispatch();
  const { items, loading, error } = useAppSelector((state) => state.fish);

  useEffect(() => {
    dispatch(fetchAllFish());
  }, [dispatch]);

  const handleAddToCart = (fishId: number) => {
    const fish = items.find((f) => f.id === fishId);
    if (fish) {
      dispatch(addToCart({
        fishId: fish.id,
        name: fish.name,
        price: fish.price,
        quantity: 1,
      }));
    }
  };

  if (loading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {items.map((fish) => (
        <div key={fish.id} className="border rounded-lg p-4 shadow-sm">
          <img src={fish.imageUrl} alt={fish.name} className="w-full h-48 object-cover rounded-md" />
          <h3 className="text-lg font-semibold mt-2">{fish.name}</h3>
          <p className="text-gray-600">{fish.description}</p>
          <div className="mt-2 flex items-center justify-between">
            <span className="text-lg font-bold">${fish.price.toFixed(2)}</span>
            <button
              onClick={() => handleAddToCart(fish.id)}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              disabled={!fish.inStock}
            >
              {fish.inStock ? 'Add to Cart' : 'Out of Stock'}
            </button>
          </div>
          <div className="mt-2 text-sm text-gray-500">
            <span>Species: {fish.species}</span>
            <span className="ml-4">Weight: {fish.weight}kg</span>
          </div>
        </div>
      ))}
    </div>
  );
};