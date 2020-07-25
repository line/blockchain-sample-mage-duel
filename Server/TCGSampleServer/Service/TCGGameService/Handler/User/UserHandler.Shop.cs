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

using TcpMsg;

namespace TCGGameService
{
	public partial class UserHandler
    {
        public void ReqShopInfo(NetMessage message)
        {
            if (user == null)
            {
                logger.Warn($"Is User Null~~~");
                return;
            }

            var req = message.GetData<ReqShopInfo>();

            user.LoadShopSlots();

            var ack = new TcpMsg.AckShopInfo();
            ack.shopSlotInfos = new Dictionary<int, List<TcpMsg.ShopSlotInfo>>();
            ack.currencyInfos = user.ToCurrencyInfoList();

            foreach (var it in Enum.GetValues(typeof(Resource.ShopType)))
            {
                var shopType = (int)it;
                if (shopType == (int)Resource.ShopType.None)
                    continue;

                ack.shopSlotInfos.Add(shopType, new List<TcpMsg.ShopSlotInfo>());
            }

            foreach (var it in user.shopSlots.Values)
            {
                foreach (var it2 in it.Values)
                {
                    var shopSlotInfo = it2.ToMsg(user.tblUser.level);
                    if (shopSlotInfo != null && ack.shopSlotInfos.ContainsKey(shopSlotInfo.shopType))
                        ack.shopSlotInfos[shopSlotInfo.shopType].Add(shopSlotInfo);
                }
            }

            user.Send(new Packet(ack));
        }
        public void ReqShopSlotReady(NetMessage message)
        {
            if (user == null)
            {
                logger.Warn($"Is User Null~~~");
                return;
            }

            var req = message.GetData<ReqShopSlotReady>();

            var ack = new TcpMsg.AckShopSlotReady();
            ack.currencyInfos = user.ToCurrencyInfoList();

            var shopSlot = user.GetShopSlot((Resource.ShopType)req.shopType, req.slotIndex);
            if (shopSlot == null)
            {
                ack.errCode = TcpMsg.Error.ShopSlotReadyFailed;
                user.Send(new Packet(ack));
                return;
            }

            if (!shopSlot.lockOpen)
            {
                ack.errCode = shopSlot.LockOpenSlot(user.tblUser.level);
                ack.shopSlotInfo = shopSlot.ToMsg(user.tblUser.level);
                user.Send(new Packet(ack));
                return;
            }

            if (shopSlot.GetRemainSec() == 0 || shopSlot.data_shop.slotTimer == 0)
            {
                ack.errCode = TcpMsg.Error.ShopSlotReadyFailed;
                user.Send(new Packet(ack));
                return;
            }

            var currency = user.GetCurrency(shopSlot.RefreshPriceType());
            if (currency == null)
            {
                ack.errCode = TcpMsg.Error.ShopSlotReadyFailed;
                user.Send(new Packet(ack));
                return;
            }

            var result = currency.CheckConsume(shopSlot.RefreshPriceType(), shopSlot.RefreshPriceValue());
            if (!result)
            {
                ack.errCode = TcpMsg.Error.ShopSlotReadyFailed;
                user.Send(new Packet(ack));
                return;
            }

            ack.errCode = shopSlot.ReadySlot(user, currency);
            if (ack.errCode != TcpMsg.Error.None)
            {
                user.Send(new Packet(ack));
                return;
            }
        }
        public void ReqShopSlotOpen(NetMessage message)
        {
            if (user == null)
            {
                logger.Warn($"Is User Null~~~");
                return;
            }

            var req = message.GetData<ReqShopSlotOpen>();

            var ack = new TcpMsg.AckShopSlotOpen();

            var shopSlot = user.GetShopSlot((Resource.ShopType)req.shopType, req.slotIndex);
            if (shopSlot == null)
            {
                ack.errCode = TcpMsg.Error.ShopSlotOpenFailed;
                user.Send(new Packet(ack));
                return;
            }

            ack.errCode = shopSlot.OpenSlot(user);
            if (ack.errCode != TcpMsg.Error.None)
            {
                user.Send(new Packet(ack));
                return;
            }

            ack.shopSlotInfo = shopSlot.ToMsg(user.tblUser.level);
            if (ack.shopSlotInfo == null)
                ack.errCode = TcpMsg.Error.ShopSlotOpenFailed;

            user.Send(new Packet(ack));
        }
        public void ReqShopBuy(NetMessage message)
        {
            if (user == null)
            {
                logger.Warn($"Is User Null~~~");
                return;
            }

            var req = message.GetData<ReqShopBuy>();

            var ack = new TcpMsg.AckShopBuy();
            var shopSlot = user.GetShopSlot((Resource.ShopType)req.shopType, req.slotIndex);
            if (shopSlot == null)
            {
                ack.errCode = TcpMsg.Error.ShopBuyFailed;
                user.Send(new Packet(ack));
                return;
            }

            var errCode = shopSlot.Buy(user);
            if (errCode != TcpMsg.Error.None)
            {
                ack.errCode = errCode;
                user.Send(new Packet(ack));
                return;
            }
        }
    }
}
