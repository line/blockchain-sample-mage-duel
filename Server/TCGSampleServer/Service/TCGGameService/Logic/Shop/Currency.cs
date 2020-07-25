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
    public abstract class Currency
    {
        public enum CurrencyType : int
        {
            Currency_token,
            Currency_nonToken,
        };

        public enum ConsumeState : int
        {
            ConsumeState_Falied,
            ConsumeState_Done,
            ConsumeState_Request_Burn,
        }

        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected User user;
        public CurrencyType currencyType { get; protected set; }
        public virtual string TokenType()
        {
            return "";
        }
        public TcpMsg.Error SetPrice(string tokenType, long change)
        {
            if (string.Compare(tokenType, TokenType()) != 0)
            {
                logger.Error($"SetPrice invalid tokenType, UserID={user.UID} tokenType={tokenType} TokenType()={TokenType()} change={change} PriceValue={PriceValue()}");
                return TcpMsg.Error.InternalError;
            }

            SetPrice(change);
            return TcpMsg.Error.None;
        }
        protected abstract void SetPrice(long change);
        public abstract Resource.PriceType PriceType();
        public abstract Int64 PriceValue();
        public abstract bool CheckConsume(Resource.PriceType priceType, Int64 priceValue);
        public abstract ConsumeState Consume(Resource.PriceType priceType, Int64 priceValue, string walletaddr, UserStateType userStateType, ShopSlot shopSlot);
    }
}
