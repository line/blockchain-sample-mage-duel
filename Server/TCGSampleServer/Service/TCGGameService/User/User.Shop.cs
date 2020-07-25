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
using System.Linq;
using Newtonsoft.Json;

namespace TCGGameService
{
	public partial class User
	{
        public Dictionary<Resource.ShopType, Dictionary<Int32, ShopSlot>> shopSlots { get; set; }
        public ShopSlot processShopSlot;

        public bool LoadShopSlots()
        {
            if (shopSlots != null)
                return true;

            shopSlots = new Dictionary<Resource.ShopType, Dictionary<Int32, ShopSlot>>();
            foreach (Resource.ShopType shopType in Enum.GetValues(typeof(Resource.ShopType)))
            {
                if (shopType == Resource.ShopType.None)
                    continue;

                shopSlots.Add(shopType, new Dictionary<Int32, ShopSlot>());
            }

            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();
            var tblShopSlots = repoUser.GetShopSlotInfo(UID);
            var tblShopSlotCards = repoUser.GetShopSlotCardInfo(tblShopSlots.Select(x => x.suid).ToArray());
            var tblShopSlotOpnes = repoUser.GetShopSlotOpenInfo(UID);

            foreach (var it_shop in TCGGameSrv.ResourceDataLoader.Data_Shop_List.GroupBy(g => g.shopType).OrderBy(x => x.Key).ToList())
            {
                var data_slots = it_shop.ToList().OrderBy(x => x.slotIndex);
                var data_slot = data_slots.FirstOrDefault();
                if (data_slot == null)
                {
                    logger.Warn($"data_slot is null UserID={UID}");
                    continue;
                }
                if (!shopSlots.ContainsKey(data_slot.shopType))
                {
                    logger.Warn($"data_slot.shopType{data_slot.shopType} is invalid UserID={UID}");
                    continue;
                }

                var shopSlotsByType = shopSlots[data_slot.shopType];

                foreach (var it_slot in data_slots)
                {
                    if (it_slot.productType == Resource.ProductType.card)
                    {
                        var tblShopSlot = tblShopSlots.Where(x => x.shopType == (int)it_slot.shopType && x.slotIndex == it_slot.slotIndex).FirstOrDefault();
                        if (tblShopSlot != null)
                        {
                            var slot = new CardShopSlot(this, it_slot, currencies, tblShopSlot, tblShopSlotCards.Where(x => x.suid == tblShopSlot.suid).ToList());
                            shopSlotsByType.Add(slot.SlotIndex(), slot);
                        }
                        else
                        {
                            bool lockOpen = tblShopSlotOpnes.Where(x => x.shopType == (int)it_slot.shopType && x.slotIndex == it_slot.slotIndex).Count() > 0;
                            var slot = new CardShopSlot(this, it_slot, currencies, tblUser.level, lockOpen);
                            shopSlotsByType.Add(slot.SlotIndex(), slot);
                        }
                    }
                    else if (it_slot.productType == Resource.ProductType.currency)
                    {
                        var slot = new CurrencyShopSlot(this, it_slot, currencies);
                        shopSlotsByType.Add(slot.SlotIndex(), slot);
                    }
                    else if (it_slot.productType == Resource.ProductType.etc)
                    {
                        var slot = new EtcShopSlot(this, it_slot, currencies);
                        shopSlotsByType.Add(slot.SlotIndex(), slot);
                    }
                }
            }

            return true;
        }

        public ShopSlot GetShopSlot(Resource.ShopType shopType, int slotIndex)
        {
            if (shopSlots == null)
            {
                logger.Error($"shopSlots is null UserUID={UID} shopType={shopType} slotIndex={slotIndex}");
                return null;
            }

            if (!shopSlots.ContainsKey(shopType))
            {
                logger.Error($"shopSlots.ContainsKey is false UserUID={UID} shopType={shopType} slotIndex={slotIndex}");
                return null;
            }

            var shopSlotsByType = shopSlots[(Resource.ShopType)shopType];

            if (!shopSlotsByType.ContainsKey(slotIndex))
            {
                logger.Error($"shopSlotsByType.ContainsKey is false UserUID={UID} shopType={shopType} slotIndex={slotIndex}");
                return null;
            }
            return shopSlotsByType[slotIndex];
        }
        private ShopSlot GetShopSlot(string jsonData)
        {
            var data_shop = JsonConvert.DeserializeObject<Resource.Data_Shop>(jsonData);
            if (data_shop == null)
            {
                logger.Error($"data_shop is null UserID={UID} jsonData={jsonData}");
                return null;
            }
            var shopSlot = GetShopSlot(data_shop.shopType, data_shop.slotIndex);
            if (shopSlot == null)
            {
                logger.Error($"shopSlot is null UserID={UID} jsonData={jsonData}");
                return null;
            }
            return shopSlot;
        }
        public void OnBurnShopSlot(List<LBD.TokenInfo> tokenInfos)
        {
            logger.Debug($"OnBurnShopSlot UserID={UID} tokenInfos.Count={tokenInfos.Count}");

            var shopSlot = processShopSlot;
            if (shopSlot == null)
            {
                logger.Error($"OnBurnShopSlot shopSlot is null UserID={UID}");
                return;
            }

            foreach (var it in tokenInfos)
                shopSlot.OnBurn(this, it.tokenType, it.amount);
        }
        public void OnBurnRefreshShopCardSlot(List<LBD.TokenInfo> tokenInfos)
        {
            logger.Debug($"OnBurnRefreshShopCardSlot UserID={UID} tokenInfos.Count={tokenInfos.Count}");

            foreach (var it in tokenInfos)
            {
                var currency = GetCurrency(it.tokenType);
                if (currency == null)
                {
                    logger.Error($"OnBurnRefreshShopCardSlot currency is null UserID={UID}");
                    continue;
                }
                currency.SetPrice(it.tokenType, -it.amount);
            }

            var shopSlot = processShopSlot;
            if (shopSlot == null)
            {
                logger.Error($"OnBurnRefreshShopCardSlot shopSlot is null UserID={UID}");
                return;
            }
            shopSlot.OnReadySlot(this);
        }
        public void OnMintBuyShopSlot(object response)
        {
            logger.Debug($"OnMintBuyShopSlot UserID={UID}");

            var shopSlot = processShopSlot;
            if (shopSlot == null)
            {
                logger.Error($"OnMintBuyShopSlot shopSlot is null UserID={UID}");
                return;
            }
            shopSlot.OnBuy(response, this);
            stateType = UserStateType.None;
            processShopSlot = null;
        }
    }
}
