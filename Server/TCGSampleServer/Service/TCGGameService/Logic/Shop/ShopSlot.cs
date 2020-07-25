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
	public abstract class ShopSlot
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected Int64 UID => user?.UID ?? 0;
        public Resource.Data_Shop data_shop { get; private set; }
        protected Currency currencyForPurchaseSlot { get; private set; }
        public bool lockOpen { get; protected set; }

        protected User user;
        private LBD.Msg.LBDBaseMsg reqhttpData;
        private UserStateType userStateType;
        public ShopSlot(User user, Resource.Data_Shop data_shop, Dictionary<Resource.PriceType, Currency> currencies)
        {
            this.user = user;
            this.data_shop = data_shop;
            this.currencyForPurchaseSlot = currencies[data_shop.priceType];
            this.lockOpen = true;
        }
        public virtual bool IsValid()
        {
            if (user == null)
            {
                Clear();
                return false;
            }    
            if (data_shop == null)
            {
                Clear();
                return false;
            }
            return true;
        }
        public virtual void Clear()
        {
            user = null;
            data_shop = null;
        }
        public Resource.ShopType ShopType()
        {
            if (!IsValid())
                return 0;

            return data_shop.shopType;
        }
        public int SlotIndex()
        {
            if (!IsValid())
                return 0;

            return data_shop.slotIndex;
        }
        public int PriceValue()
        {
            if (!IsValid())
                return 0;

            return data_shop.priceValue;
        }
        public Resource.PriceType RefreshPriceType()
        {
            if (!IsValid())
                return Resource.PriceType.None;

            return data_shop.refreshPriceType;
        }
        public int RefreshPriceValue()
        {
            if (!IsValid())
                return 0;

            return data_shop.refreshPriceValue;
        }
        public virtual TcpMsg.ShopSlotInfo ToMsg(int level)
        {
            if (!IsValid())
                return null;

            return new TcpMsg.ShopSlotInfo()
            {
                shopType = (int)ShopType(),
                slotIndex = data_shop.slotIndex,
                unlockLevel = data_shop.slotUnlockLevel,
                state = 0,
                remainSec = -1,
                productType = (int)data_shop.productType,
                productSubType = data_shop.productSubType,
                cardids = new List<int>(),
                sellCount = data_shop.sellCount,
                priceType = (int)data_shop.priceType,
                priceValue = data_shop.priceValue,
                itemName = data_shop.itemName,
                extraValue = data_shop.extraValue
            };
        }
        public virtual TcpMsg.Error LockOpenSlot(int level)
        {
            return TcpMsg.Error.ShopSlotLockOpenFailed;
        }
        public virtual TcpMsg.Error ReadySlot(User user, Currency currency)
        {
            return TcpMsg.Error.ShopSlotReadyFailed;
        }
        public virtual void OnReadySlot(User user)
        {
        }
        public virtual TcpMsg.Error OpenSlot(User user)
        {
            return TcpMsg.Error.ShopSlotOpenFailed;
        }
        public virtual Int64 GetRemainSec()
        {
            return -1;
        }
        public virtual bool CheckBuy(string walletaddr, int level)
        {
            logger.Debug($"CheckBuy UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");

            if (!IsValid())
            {
                return false;
            }

            if (data_shop.slotUnlockLevel > level)
            {
                logger.Error($"data_shop.slotUnlockLevel{data_shop.slotUnlockLevel} > level{level} UserUID={UID}");
                return false;
            }

            if (!currencyForPurchaseSlot.CheckConsume(data_shop.priceType, data_shop.priceValue))
            {
                logger.Error($"currency.CheckBuy is false UserUID={UID}");
                return false;
            }

            return true;
        }
        public abstract TcpMsg.Error Buy(User user);
        protected Currency.ConsumeState ConsumeCurrency(string walletaddr, int level)
        {
            logger.Debug($"Buy UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");

            if (!CheckBuy(walletaddr, level))
            {
                return Currency.ConsumeState.ConsumeState_Falied;
            }

            var consumeState = currencyForPurchaseSlot.Consume(data_shop.priceType, data_shop.priceValue, walletaddr, UserStateType.BuyShopSlot, this);
            if (consumeState == Currency.ConsumeState.ConsumeState_Falied)
            {
                logger.Error($"currency.Buy is false UserUID={UID} ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex}");
                return Currency.ConsumeState.ConsumeState_Falied;
            }

            return consumeState;
        }
        protected void SetMint(LBD.Msg.LBDBaseMsg req, UserStateType userStateType)
        {
            this.reqhttpData = req;
            this.userStateType = userStateType;
        }
        public TcpMsg.Error SendMint()
        {
            if (reqhttpData == null)
            {
                return TcpMsg.Error.InternalError;
            }

            user.stateType = userStateType;
            LBD.LBDApiManager.Instance.AddLBDCmd(reqhttpData);
            user.processShopSlot = this;
            reqhttpData = null;
            userStateType = UserStateType.None;

            return TcpMsg.Error.None;
        }
        public virtual TcpMsg.Error OnBurn(User user, string tokenType, int amount)
        {
            return currencyForPurchaseSlot.SetPrice(tokenType, -amount);
        }
        public abstract void OnBuy(Object data, User user);
    }
}
