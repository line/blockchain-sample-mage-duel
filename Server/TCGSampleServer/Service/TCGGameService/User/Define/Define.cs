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
    public class Define
    {
        public const Int32 LEVELUP = 10;
        public const Int32 LEVELUPHP = 20;

        public const Int32 PAGENUM = 12;
        public const Int32 EXPIRY_DATE = 10;

        public const Int32 saleMinimumCoin = 30;
        public const Int32 defaultCoin = 2;
        public const Int32 percent = 100;
        public const Int32 saleFeeRatio = 10;
        public const Int32 expirationTime = 10 * 24;

        public const Int32 perDefaultRuby = 500;

        public const Int32 NEW_MARK_PERIOD_FOR_ICON_TITLE = 3;

        public static Int32 GetCoinFees(Int32 price)
        {
            var spendCoin = 0;
            if (price < saleMinimumCoin)
            {
                spendCoin = defaultCoin;
            }
            else
            {
                spendCoin = price * saleFeeRatio / percent;
            }

            return spendCoin;
        }
    }

    public enum UserStateType
    {
        None = 0,
        Login,
        RequestProxy,
        NewUserConnect,
        ExistingUserConnect,
        DefaultCardCreate,
        CreateNickName,

        InitGetFungibleToken,
        InitGetServiceToken,
        InitGetNonFungibleTokenType,

        InitGetDeck,
        AuthSucess,


        DeckCardAdd,
        DeckCardRemove,

        RegisterTradeGoodsFees,
        SellerPayments,
        BuyerGiveGoods,

        BuyFungible,
        BuyShopSlot,
        RefreshShopCardSlot,
        BuyNonFungible,
    }
}
