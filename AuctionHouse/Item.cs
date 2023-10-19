using System;
namespace AuctionHouse
{
	public class Item
	{
		public string Title { get; set; }
		public double Price { get; set; }
		public string Description { get; set; }

		public Item(string title, double price, string description)
		{
			Title = title;
			Price = price;
			Description = description;
		}
	}
}

