/*
 * Copyright 2020 LINE Corporation
 *
 * LINE Corporation licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;

namespace TCGGameService
{
	public class SellerData
	{
		public Int64 uuid;
		public string userid;
		public string nickName;
		public LBD.TokenInfo card;
		public string comment;
		public Int32 sale_price;

		public void Clear()
		{
			if (null != card)
			{
				uuid = 0;
				nickName = string.Empty;
				card = null;
				comment = string.Empty;
				sale_price = 0;
			}
		}
	}

	public class BuyerData
	{
		public Int64 uuid;
        public string userid;
        public string nickName;
		public string buyTokenType;
		public Table.TblTrade goods;
	}
}
