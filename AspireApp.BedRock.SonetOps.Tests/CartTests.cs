using System.Linq;
using AspireApp.BedRock.SonetOps.Web.Models;
using AspireApp.BedRock.SonetOps.Web.Store;
using Xunit;

namespace AspireApp.BedRock.SonetOps.Tests
{
    public class CartTests
    {
        [Fact]
        public void AddToCart_NewItem_AddsWithQuantityOne()
        {
            // Arrange
            var state = new CartState();
            var item = new CartItem
            {
                Id = "1",
                Name = "Test Item",
                Price = 10.00m,
                ImageUrl = "test.jpg"
            };

            // Act
            var newState = CartReducer.OnAddToCart(state, new AddToCartAction(item));

            // Assert
            Assert.Single(newState.Items);
            Assert.Equal(1, newState.Items[0].Quantity);
            Assert.Equal(10.00m, newState.Total);
        }

        [Fact]
        public void AddToCart_ExistingItem_IncreasesQuantity()
        {
            // Arrange
            var item = new CartItem
            {
                Id = "1",
                Name = "Test Item",
                Price = 10.00m,
                ImageUrl = "test.jpg",
                Quantity = 1
            };
            var state = new CartState { Items = new List<CartItem> { item } };

            // Act
            var newState = CartReducer.OnAddToCart(state, new AddToCartAction(item));

            // Assert
            Assert.Single(newState.Items);
            Assert.Equal(2, newState.Items[0].Quantity);
            Assert.Equal(20.00m, newState.Total);
        }

        [Fact]
        public void RemoveFromCart_ExistingItem_RemovesItem()
        {
            // Arrange
            var item = new CartItem
            {
                Id = "1",
                Name = "Test Item",
                Price = 10.00m,
                ImageUrl = "test.jpg",
                Quantity = 1
            };
            var state = new CartState { Items = new List<CartItem> { item } };

            // Act
            var newState = CartReducer.OnRemoveFromCart(state, new RemoveFromCartAction("1"));

            // Assert
            Assert.Empty(newState.Items);
            Assert.Equal(0m, newState.Total);
        }

        [Fact]
        public void UpdateQuantity_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            var item = new CartItem
            {
                Id = "1",
                Name = "Test Item",
                Price = 10.00m,
                ImageUrl = "test.jpg",
                Quantity = 1
            };
            var state = new CartState { Items = new List<CartItem> { item } };

            // Act
            var newState = CartReducer.OnUpdateQuantity(state, new UpdateQuantityAction("1", 3));

            // Assert
            Assert.Single(newState.Items);
            Assert.Equal(3, newState.Items[0].Quantity);
            Assert.Equal(30.00m, newState.Total);
        }

        [Fact]
        public void UpdateQuantity_ZeroQuantity_RemovesItem()
        {
            // Arrange
            var item = new CartItem
            {
                Id = "1",
                Name = "Test Item",
                Price = 10.00m,
                ImageUrl = "test.jpg",
                Quantity = 1
            };
            var state = new CartState { Items = new List<CartItem> { item } };

            // Act
            var newState = CartReducer.OnUpdateQuantity(state, new UpdateQuantityAction("1", 0));

            // Assert
            Assert.Empty(newState.Items);
            Assert.Equal(0m, newState.Total);
        }

        [Fact]
        public void ClearCart_RemovesAllItems()
        {
            // Arrange
            var item1 = new CartItem { Id = "1", Name = "Item 1", Price = 10.00m, Quantity = 1 };
            var item2 = new CartItem { Id = "2", Name = "Item 2", Price = 20.00m, Quantity = 2 };
            var state = new CartState { Items = new List<CartItem> { item1, item2 } };

            // Act
            var newState = CartReducer.OnClearCart(state, new ClearCartAction());

            // Assert
            Assert.Empty(newState.Items);
            Assert.Equal(0m, newState.Total);
        }
    }
}