using System;
namespace AuctionHouse
{
	public static class Constants
	{
		public static int NEW_AUCTION_EXPIRATION = 10000;
		public static int BIDDER_WINS_AFTER = 7000;
		public static string CHR = "-"; // separates main request parts, e.g., JOIN-axs3sed98fue-Bob,33.33
		public static string SUB = ","; // separates the data (last) part of a request, e.g. Bob,33.33
	}
}
