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
using System.Collections.Generic;

namespace TCGGameService
{
	public class EtcShopSlot : ShopSlot
	{
        public EtcShopSlot(User user, Resource.Data_Shop data_shop, Dictionary<Resource.PriceType, Currency> currencies) : base(user, data_shop, currencies)
        {

        }
        public override TcpMsg.Error Buy(User user)
        {
            var consumeState = ConsumeCurrency(user.tblUser.address, user.tblUser.level);
            if (consumeState == Currency.ConsumeState.ConsumeState_Falied)
            {
                return TcpMsg.Error.ShopBuyFailed;
            }

            if (consumeState == Currency.ConsumeState.ConsumeState_Done)
            {
                OnBuy(data_shop.sellCount, user);
                return TcpMsg.Error.None;
            }

            logger.Debug($"Buy UserID={UID}  ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex} address={user.tblUser.address}");

            return TcpMsg.Error.None;
        }
        public override TcpMsg.Error OnBurn(User user, string tokenType, int amount)
        {
            var result = base.OnBurn(user, tokenType, amount);
            if (result != TcpMsg.Error.None)
            {
                return result;
            }

            OnBuy(null, user);
            return TcpMsg.Error.None;
        }
        public override void OnBuy(Object data, User user)
        {
            logger.Debug($"OnBuy UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");

            var ack = new TcpMsg.AckShopBuy();
            ack.errCode = TcpMsg.Error.None;
            ack.shopSlotInfo = ToMsg(user.tblUser.level);
            ack.currencyInfos = user.ToCurrencyInfoList();

            user.Send(new Packet(ack));
        }
    }
}
