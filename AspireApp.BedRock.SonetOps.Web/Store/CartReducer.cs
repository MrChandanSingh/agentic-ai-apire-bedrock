using System;
using System.Linq;
using AspireApp.BedRock.SonetOps.Web.Models;
using Fluxor;

namespace AspireApp.BedRock.SonetOps.Web.Store
{
    public class CartFeature : Feature<CartState>
    {
        public override string GetName() => "Cart";

        protected override CartState GetInitialState() => new CartState();
    }

    public static class CartReducer
    {
        [ReducerMethod]
        public static CartState OnAddToCart(CartState state, AddToCartAction action)
        {
            var items = state.Items.ToList();
            var existingItem = items.FirstOrDefault(i => i.Id == action.Item.Id);

            if (existingItem != null)
            {
                var index = items.IndexOf(existingItem);
                items[index] = new CartItem
                {
                    Id = existingItem.Id,
                    Name = existingItem.Name,
                    Price = existingItem.Price,
                    ImageUrl = existingItem.ImageUrl,
                    Quantity = existingItem.Quantity + 1
                };
            }
            else
            {
                items.Add(new CartItem
                {
                    Id = action.Item.Id,
                    Name = action.Item.Name,
                    Price = action.Item.Price,
                    ImageUrl = action.Item.ImageUrl,
                    Quantity = 1
                });
            }

            return new CartState { Items = items };
        }

        [ReducerMethod]
        public static CartState OnRemoveFromCart(CartState state, RemoveFromCartAction action)
        {
            var items = state.Items.ToList();
            items.RemoveAll(i => i.Id == action.ItemId);
            return new CartState { Items = items };
        }

        [ReducerMethod]
        public static CartState OnUpdateQuantity(CartState state, UpdateQuantityAction action)
        {
            var items = state.Items.ToList();
            var itemIndex = items.FindIndex(i => i.Id == action.ItemId);

            if (itemIndex >= 0)
            {
                var item = items[itemIndex];
                if (action.Quantity <= 0)
                {
                    items.RemoveAt(itemIndex);
                }
                else
                {
                    items[itemIndex] = new CartItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        ImageUrl = item.ImageUrl,
                        Quantity = action.Quantity
                    };
                }
            }

            return new CartState { Items = items };
        }

        [ReducerMethod]
        public static CartState OnClearCart(CartState state, ClearCartAction action)
        {
            return new CartState();
        }
    }

    public class AddToCartAction
    {
        public CartItem Item { get; }

        public AddToCartAction(CartItem item)
        {
            Item = item;
        }
    }

    public class RemoveFromCartAction
    {
        public string ItemId { get; }

        public RemoveFromCartAction(string itemId)
        {
            ItemId = itemId;
        }
    }

    public class UpdateQuantityAction
    {
        public string ItemId { get; }
        public int Quantity { get; }

        public UpdateQuantityAction(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    public class ClearCartAction { }