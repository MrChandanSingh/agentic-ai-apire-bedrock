using System.Collections.Generic;

namespace AspireApp.BedRock.SonetOps.Web.Models
{
    public class CartItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CartState
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => CalculateTotal();
        public bool IsCheckoutEnabled => Items.Count > 0;

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Price * item.Quantity;
            }
            return total;
        }
    }
}