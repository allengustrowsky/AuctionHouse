using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Security.Cryptography;
using System.Web;
using System.Timers;

namespace AuctionHouse
{
    public enum RequestTypes
    {
        JOIN,
        DEPOSIT,
        INVENTORY,
        SALES,
        AUCTION,
        APPROVE,
        BID
    }

    public enum AuctionState
    {
        CLOSED,
        PENDING,
        OPEN
    }

    public class UserServerState
    {
        public string Name { get; set; } // user's name
        public double Balance { get; set; } // user's balance
        public string ApiKey { get; set; } // user's api key
        public List<Item>[] Inventory { get; set; } // user's inventory
        public List<Item>[] SoldItems { get; set; } // user's sold items
        public string Admin { get; set; } // name admin
        public string SalesPerson { get; set; } // name of salesperson
        public string HighestBidder { get; set; } // name of highest bidder
        public double HighestBid { get; set; } = 0;
        public AuctionState AuctionState { get; set; } = AuctionState.CLOSED;
        public Item ItemForSale { get; set; }
        public string LogMessage { get; set; }

        public UserServerState(User user, string apiKey, string admin, string salesPerson, string highestBidder, double highestBid, AuctionState auctionState, Item itemForSale, string logMessage)
        {
            Name = user.Name;
            Balance = user.Balance;
            ApiKey = apiKey;
            Admin = admin;
            SalesPerson = salesPerson;
            HighestBidder = highestBidder;
            HighestBid = highestBid;
            AuctionState = auctionState;
            ItemForSale = itemForSale;
            LogMessage = logMessage;
        }
    }

    public static class AuctionServer
    {
        public static Dictionary<string, User> Users = new Dictionary<string, User>();
        public static string Admin = "";
        public static string SalerPerson = "";
        public static AuctionState AuctionState = AuctionState.CLOSED;
        public static string HighestBidder = "";
        public static double HighestBid = 0;
        public static Item ItemForSale;
        public static System.Timers.Timer AuctionClock = new System.Timers.Timer(1000); // time doesn't matter

        public static void Main()
        {

            var ServerSocket = new TcpListener(IPAddress.Any, 8081);
            ServerSocket.Start();

            while (true)
            {
                try
                {
                    var client = ServerSocket.AcceptTcpClient();
                    string request = client.ReadString();
                    if (!RequestValidator.validateJoin(request))
                    {
                        client.WriteString("Bad request in JOIN!"); // TODO handle better error returning
                        client.Close();
                    }
                    else
                    {
                        // extract data and make a new user
                        string[] data = request.Split(Constants.CHR)[2].Split(',');
                        User usr = new User(data[0], double.Parse(data[1]), client);
                        string apiKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                        apiKey = HttpUtility.UrlDecode(apiKey);
                        Users.Add(apiKey, usr);
                        if (Users.Count == 1)
                        {
                            Admin = apiKey;
                        }
                        Announce($"{usr.Name} joined the auction house!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client closed their connection.");
                }

            }
        }

        public static void Announce(string msg)
        {
            foreach (var usr in Users)
            {
                try
                {
                    string fullMessage = ToJSONString(usr.Key, msg);
                    usr.Value.Connection.WriteString(fullMessage);
                }
                catch
                {
                    Console.WriteLine($"Failed to write to user: {usr.Value.Name}");

                }
            }
        }

        public static void HandleAuctionTiming()
        {
            AuctionClock.Stop();
            AuctionClock.Dispose();

            bool noBidders = (HighestBid == 0);
            AuctionClock = new System.Timers.Timer(noBidders ? Constants.NEW_AUCTION_EXPIRATION : Constants.BIDDER_WINS_AFTER);
            //AuctionClock.Elapsed += noBidders ? HandleNewAucExpiration : HandleAuctionWinner;
            if (noBidders)
            {
                AuctionClock.Elapsed += HandleNewAucExpiration;
            } else
            {
                AuctionClock.Elapsed += HandleAuctionWinner;
            }
            AuctionClock.AutoReset = false;
            AuctionClock.Enabled = true;
        }

        private static void HandleNewAucExpiration(System.Object? src, ElapsedEventArgs ea)
        {
            string itemTemp = ItemForSale.Title;
            SalerPerson = "";
            AuctionState = AuctionState.CLOSED;
            HighestBidder = "";
            HighestBid = 0;
            ItemForSale = null;
            AuctionClock.Stop();

            Announce($"The auction for {itemTemp} has expired because no one has submitted a bid in the last {Constants.NEW_AUCTION_EXPIRATION / 1000} seconds. The auction floor is now open.");
    }

        private static void HandleAuctionWinner(System.Object? src, ElapsedEventArgs ea)
        {
            string itemTemp = ItemForSale.Title;
            string winner = Users[HighestBidder].Name;

            Users[SalerPerson].SoldItems.Add(ItemForSale);
            Users[SalerPerson].Balance += HighestBid;
            Users[HighestBidder].Inventory.Add(ItemForSale);
            Users[HighestBidder].Balance -= HighestBid;
            SalerPerson = "";
            AuctionState = AuctionState.CLOSED;
            HighestBidder = "";
            HighestBid = 0;
            ItemForSale = null;
            AuctionClock.Stop();

            Announce($"{winner} has won the auction for {itemTemp}! The auction floor is now open.");
        }

        /*
         * Returns the stringified JSON version of the current server state and
         * details relevant to the user identified by `key`. The key must be 
         * guaranteed to exist in the Users array (a missing key is not handled 
         * by this method).  Also adds a log message to be used by the client.
        */
        public static string ToJSONString(string key, string logMessage)
        {
            string ad = (Admin != "" ? Users[Admin].Name : "None");
            string sp = (SalerPerson != "" ? Users[SalerPerson].Name : "None");
            string hb = (HighestBidder != "" ? Users[HighestBidder].Name : "None");

            UserServerState userServerState = new UserServerState(Users[key], key, ad, sp, hb, HighestBid, AuctionState, ItemForSale, logMessage);
            return JsonSerializer.Serialize(userServerState);
        }
    }
}
