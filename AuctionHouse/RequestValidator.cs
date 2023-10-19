
namespace AuctionHouse
{
	public static class RequestValidator
    {
		/// <summary>
		/// [TYPE]-[apiKey]-[data]
		/// Performs general validation on all requests that are not JOIN.
		/// Makes sure the type is valid and that it contains all three general
		/// parts (TYPE, apiKey, and data).  Must include a valid request type.
		/// Any part of the request may be empty.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static bool validateTopLevel(string request)
		{
            string[] parts = request.Split(Constants.CHR);
            if (parts.Length != 3) { return false; }
            if (!Enum.IsDefined(typeof(RequestTypes), parts[0])) { return false; }

            return true;
		}

        /// <summary>
        /// JOIN--name,balance
		/// name: <c>string</c>
		/// balance: <c>double</c>
        /// </summary>
        /// <param name="request">request string to validate with the above format.</param>
        /// <returns>Whether the request passes validation.</returns>
        public static bool validateJoin(string request)
		{
			string[] parts = request.Split(Constants.CHR);
			if (parts.Length != 3) { return false; }
			if (parts[0] != "JOIN") { return false; }

			// validate user props
			string[] props = parts[2].Split(',');
			if (props.Length != 2) { return false; }
			try { double initialDeposit = double.Parse(props[1]); } catch { return false; }

			return true;
		}

        /// <summary>
        /// DEPOSIT-[apiKey]-123.32 
        /// </summary>
        /// <param name="request">request string to validate with the above format.</param>
        /// <returns>Whether the request passes validation.</returns>
        public static bool ValidateDeposit(string request)
		{
			string[] parts = request.Split(Constants.CHR);
			if (parts.Length != 3) { return false; }
			try { double amount = double.Parse(parts[2]); } catch { return false; }

			return true;
		}

        /// <summary>
        /// AUCTION-[apiKey]-title,price,description
		/// title: <c>string</c>
		/// price: <c>double</c>
		/// description: <c>string</c>
        /// </summary>
        /// <param name="request">request string to validate with the above format.</param>
        /// <returns>Whether the request passes validation.</returns>
        public static bool ValidateAuction(string request)
		{
            string[] parts = request.Split(Constants.CHR);
            if (parts.Length != 3) { return false; }

            //title,price,description
            string[] details = parts[2].Split(',');
			if (details.Length != 3) { return false; }
            try
			{
				double amount = double.Parse(details[1]);
				if (amount <= 0) { return false; }
			} catch { return false; }

            return true;
		}

        /// <summary>
        /// APPROVE-[apiKey]-[1 or 0]
		/// The approval must be 1 or 0 - otherwise validation will fail.
        /// </summary>
        /// <param name="request">request string to validate with the above format.</param>
        /// <returns>Whether the request passes validation.</returns>
        public static bool ValidateApprove(string request)
		{
			string[] parts = request.Split(Constants.CHR);
			if (parts.Length != 3) { return false; }

			try
			{
				int approve = int.Parse(parts[2]);
                if (approve != 0 && approve != 1) { return false; }
			} catch { return false; }

			return true;
		}

        /// <summary>
        /// BID-[apiKey]-12.32
        /// </summary>
        /// <param name="request">request string to validate with the above format.</param>
        /// <returns>Whether the request passes validation.</returns>
        public static bool ValidateBid(string request)
		{
			string[] parts = request.Split(Constants.CHR);
			if (parts.Length != 3) { return false; }

			try { double bid = double.Parse(parts[2]); } catch { return false; }

			return true;
		}

    }
}

