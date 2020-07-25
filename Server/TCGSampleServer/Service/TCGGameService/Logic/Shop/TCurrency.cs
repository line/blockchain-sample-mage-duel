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
	public class TCurrency : Currency
    {
        private Resource.FungibleType fungibleType;
        private string tokenType;
        private Resource.PriceType priceType;
        private Int64 priceValue;
        public TCurrency(User user, Resource.FungibleType fungibleType, string tokenType, Resource.PriceType priceType, Int64 priceValue)
        {
            currencyType = Currency.CurrencyType.Currency_token;

            this.user = user;
            this.fungibleType = fungibleType;
            this.tokenType = tokenType;
            this.priceType = priceType;
            this.priceValue = priceValue;
        }
        public override string TokenType()
        {
            return tokenType;
        }
        protected override void SetPrice(long change)
        {
            priceValue += change;
        }
        public override Resource.PriceType PriceType()
        {
            return priceType;
        }
        public override Int64 PriceValue()
        {
            return priceValue;
        }
        public override bool CheckConsume(Resource.PriceType priceType, Int64 priceValue)
        {
            if (priceType != this.priceType)
            {
                logger.Error($"priceType != tblCurrency.priceType UserID={user.UID} priceType={priceType} data_shop.priceValue={priceValue} priceType={priceType} priceValue={priceValue}");
                return false;
            }

            if (priceValue > this.priceValue)
            {
                logger.Error($"priceValue > tblCurrency.priceValue UserID={user.UID} priceType={priceType} data_shop.priceValue={priceValue} priceType={priceType} priceValue={priceValue}");
                return false;
            }

            return true;
        }
        public override ConsumeState Consume(Resource.PriceType priceType, Int64 priceValue, string walletaddr, UserStateType userStateType, ShopSlot shopSlot)
        {
            if (!CheckConsume(priceType, priceValue))
                return ConsumeState.ConsumeState_Falied;

            if (priceValue == 0)
                return ConsumeState.ConsumeState_Done;

            user.stateType = userStateType;
            user.processShopSlot = shopSlot;

            if (fungibleType == Resource.FungibleType.Fungible)
                user.WithdrawFungible(TokenType(), (int)priceValue);
            else if (fungibleType == Resource.FungibleType.ServiceToken)
                user.WithdrawServiceToken((int)priceValue);

            return ConsumeState.ConsumeState_Request_Burn;
        }
    }
}
