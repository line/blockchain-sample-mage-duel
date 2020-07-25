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

namespace TcpMsg
{
	public enum Error
	{
		None = 0,

        AccessToken_CheckError,
        AccessToken_NullOrEmpty,
        AuthorizationFail,

        NickNameNullOrEmpty,
        NickName_Already_Exists,

        IsNot_LBW_Member,
        IsNot_ProxyApproved,

        Deck_Already_Exists,
        IsNullOrEmpty_DeckName,
        Deck_SameName,
        NotFoundDeckInfo,
        DeckUnitMaxCount,

        DBUpdateError,

        NotFoundTokenType,
        NotFoundTokenInfo,

        NotHeroCard,
        ResourcdNotFound,

        DeckAddError,
        DeckRemoveError,
        AlreadySetDeckCard,

        Lack_the_currency,

        RegisterGoodsFail,              // Product registration failed
        AlreadyTradeCard,               // Products already registered
        DataBaseTradeDeleteFail,        // Product deletion failed in database
        ServerTradeDeleteFail,          // Product deletion failed on server
        NotOnsale,                      // Not on sale
        NotFoundGoodsOrTradeing,        // There are no purchased products or they have already been traded.
        NotTradeSelfGoods,              // Self-trading unavailable


        ShopSlotLockOpenFailed,         // LockOpen failed
        ShopSlotReadyFailed,            // Ready failed
        RemainShopSlotTime,             // Time remain
        ShopSlotOpenFailed,             // Open failed
        ShopBuyFailed,                  // Purchase failed
        AfterShopSlotRefreshTime,       // Purchase time passed

        InternalError
	};

	public enum ErrorInternal
	{

	}
}
