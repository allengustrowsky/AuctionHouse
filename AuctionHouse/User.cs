using System.Net.Sockets;

namespace AuctionHouse
{
	public class User
	{
		public string Name;
		public double Balance;
        public TcpClient Connection;
        public List<Item> Inventory = new List<Item>();
        public List<Item> SoldItems = new List<Item>();

        public User(string name, double balance, TcpClient conn)
		{
			Name = name;
			Balance = balance;
			Connection = conn;

			Thread thr = new Thread(HandleRequest);
			thr.Start();
		}

		private void HandleRequest()
		{
			while (true)
			{
				try
				{
					string request = Connection.ReadString();
					if (!RequestValidator.validateTopLevel(request))
					{
						Connection.WriteString("Bad Request!"); // TODO handle better error returning
                    } 
					else {
						switch (request.Split(Constants.CHR)[0])
						{
							case "DEPOSIT":
								HandleDeposit(request);
								break;
							case "AUCTION":
								HandleAuction(request);
								break;
							case "APPROVE":
								HandleApprove(request);
								break;
							case "BID":
								HandleBid(request);
								break;
							default:
								Connection.WriteString("Bad Request in default case!");// TODO handle better error returning
                                break;
                        }
					}
				} catch (Exception e)
				{
					Console.WriteLine("An Exception occurred: " + e.ToString());
					Connection.WriteString("An error occurred. Please try again."); // TODO handle better error returning
                }
			}
		}

		private void HandleDeposit(string request)
		{
			if (!RequestValidator.ValidateDeposit(request))
			{
				Connection.WriteString("Bad DEPOSIT request!"); // TODO handle better error returning
            }
			else
			{
				string apiKey = request.Split(Constants.CHR)[1];
                if (!AuctionServer.Users.ContainsKey(apiKey))
				{
					Connection.WriteString("Error: bad api key!"); // TODO handle better errror returning
				} else
				{
					double amount = double.Parse(request.Split(Constants.CHR)[2]);
					AuctionServer.Users[apiKey].Balance += amount;
                    Connection.WriteString(AuctionServer.ToJSONString(apiKey, $"You successfully deposited ${amount}"));
                }
			}
		}

        private void HandleAuction(string request)
        {
			if (!RequestValidator.ValidateAuction(request))
			{
				Connection.WriteString("Error: failed Auction validation."); // TODO handle better errror returning
            } else
			{
				string apiKey = request.Split(Constants.CHR)[1];
				//Connection.WriteString(apiKey);
				//Connection.WriteString(JsonSerializer.Serialize(AuctionServer.Users));
				if (!AuctionServer.Users.ContainsKey(apiKey))
				{
					Connection.WriteString("Error: bad api key!"); // TODO handle better errror returning
				} else if (AuctionServer.AuctionState == AuctionState.PENDING || AuctionServer.AuctionState == AuctionState.OPEN)
				{
					Connection.WriteString("Error: auction already in progress!");
				} else if (AuctionServer.Users.Count < 2)
				{
					Connection.WriteString("Error: must have more than one user to run an auction!");
				} else
				{
					string[] itemParts = request.Split(Constants.CHR)[2].Split(',');
					Item item = new Item(itemParts[0], double.Parse(itemParts[1]), itemParts[2]);
					AuctionServer.ItemForSale = item;
					AuctionServer.SalerPerson = apiKey;
					AuctionServer.AuctionState = AuctionState.PENDING;
					AuctionServer.Announce($"{Name} submitted an auction request for the follow item: {item.Title}");
				}
			}
        }

        private void HandleApprove(string request)
        {
			if (!RequestValidator.ValidateApprove(request))
			{
                Connection.WriteString("Error: failed Approve validation."); // TODO handle better error returning
            } else
			{
				string apiKey = request.Split(Constants.CHR)[1];
				int approval = int.Parse(request.Split(Constants.CHR)[2]);
				if (apiKey != AuctionServer.Admin)
				{
					Connection.WriteString("Error: unauthorized to approve this auction!"); // TODO handle better error returning
                } else if (AuctionServer.AuctionState != AuctionState.PENDING)
				{
					Connection.WriteString("Error: no auction is pending approval!"); // TODO handle better error returning
                } else
				{
					if (approval == 0) // declined
					{
						AuctionServer.ItemForSale = null;
						AuctionServer.SalerPerson = "";
						AuctionServer.AuctionState = AuctionState.CLOSED;
						AuctionServer.Announce("The admin has declined the auction request.");
						
					} else // approved
					{
                        AuctionServer.AuctionState = AuctionState.OPEN;
						AuctionServer.HandleAuctionTiming();
                        AuctionServer.Announce("The admin has approved the auction request. The auction is now live!");
                    }
				}
            }            
        }

        private void HandleBid(string request)
        {
			if (!RequestValidator.ValidateBid(request))
			{
				Connection.WriteString("Error: failed Bid validation"); // TODO handle better error returning
            } else
			{
				string apiKey = request.Split(Constants.CHR)[1];
				double bid = double.Parse(request.Split(Constants.CHR)[2]);
                if (!AuctionServer.Users.ContainsKey(apiKey)) // must be a valid user
				{
					Connection.WriteString("Error: invalid api key!"); // TODO handle better error returning
                } else if (AuctionServer.AuctionState != AuctionState.OPEN) // has to be in live auction state.
                {
					Connection.WriteString("Cannot bid! No auction is currently live!"); // TODO handle better error returning 
                } else if (apiKey == AuctionServer.SalerPerson) // must not be salesperson
                {
					Connection.WriteString("Cannot bid on your own auction!"); // TODO handle better error returning
                } else if (bid > AuctionServer.Users[apiKey].Balance) // must have enough money
                {
					Connection.WriteString("You don't have enough money to make this bid!");
				} else if (bid <= AuctionServer.ItemForSale.Price) // must be higher than initial price of item
				{
					Connection.WriteString("Bid must be higher than item's initial price!");
				} else if (bid <= AuctionServer.HighestBid) // must be higher than previous bid
                {
					Connection.WriteString($"Bid must be higher than the highest current bid - ${AuctionServer.HighestBid}!");
				} else
				{
					AuctionServer.HighestBidder = apiKey;
					AuctionServer.HighestBid = bid;
					AuctionServer.HandleAuctionTiming();
					AuctionServer.Announce($"{Name} has just bid ${bid}!");
				}
            }			
        }
    }
}

