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

namespace TCGGameService
{
    public class CardShopSlot : ShopSlot
    {
        private Table.TblShopSlot tblShop = null;
        private List<Table.TblShopSlotCard> tblShopCard = null;

        // DB LOAD
        public CardShopSlot(User user, Resource.Data_Shop data_shop, Dictionary<Resource.PriceType, Currency> currencies, Table.TblShopSlot tblShop, List<Table.TblShopSlotCard> tblShopCard) : base(user, data_shop, currencies)
        {
            logger.Debug($"Init UserID={UID} ShopType={data_shop?.shopType} SlotIndex={tblShop?.slotIndex} cardCount={tblShopCard?.Count}");

            this.tblShop = tblShop;
            this.tblShopCard = tblShopCard;
            this.lockOpen = true;
        }

        public CardShopSlot(User user, Resource.Data_Shop data_shop, Dictionary<Resource.PriceType, Currency> currencies, int level, bool lockOpen) : base(user, data_shop, currencies)
        {
            logger.Debug($"Init UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex} Level={level} LockOpen={lockOpen}");
            this.lockOpen = lockOpen;
            if (data_shop.slotUnlockLevel < 2)
                this.lockOpen = true;
        }
        public override void Clear()
        {
            base.Clear();

            tblShop = null;
            if (tblShopCard != null)
            {
                tblShopCard.Clear();
                tblShopCard = null;
            }
        }
        public override TcpMsg.ShopSlotInfo ToMsg(int level)
        {
            if (!IsValid())
                return null;

            var msg = new TcpMsg.ShopSlotInfo();

            msg.shopType = (int)ShopType();
            msg.slotIndex = SlotIndex();
            msg.unlockLevel = data_shop.slotUnlockLevel;
            if (RefreshPriceType() != Resource.PriceType.None)
            {
                msg.readyPrice = new TcpMsg.CurrencyInfo()
                {
                    priceType = (int)RefreshPriceType(),
                    priceValue = RefreshPriceValue()
                };
            }

            msg.state = (int)TcpMsg.ShopSlotInfo.eState.eState_Ready;
            if (data_shop.slotUnlockLevel > level)
            {
                msg.state = (int)TcpMsg.ShopSlotInfo.eState.eState_Lock;
            }
            else
            {
                if (GetRemainSec() == 0)
                {
                    if (lockOpen)
                        msg.state = (int)TcpMsg.ShopSlotInfo.eState.eState_Ready;
                    else
                        msg.state = (int)TcpMsg.ShopSlotInfo.eState.eState_LockOpen;
                }
                else
                {
                    msg.state = (int)TcpMsg.ShopSlotInfo.eState.eState_Open;

                    msg.remainSec = GetRemainSec();
                    msg.productType = tblShop.productType;
                    msg.productSubType = tblShop.productSubType;
                    msg.cardids = tblShopCard.Select(x => x.cardid).ToList();
                    msg.sellCount = tblShop.sellCount;
                    msg.priceType = tblShop.priceType;
                    msg.priceValue = tblShop.priceValue;
                    msg.itemName = tblShop.itemName;
                    msg.extraValue = tblShop.extraValue;
                }
            }

            return msg;
        }
        public override TcpMsg.Error LockOpenSlot(int level)
        {
            logger.Debug($"ReadySlot UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop.slotIndex}");

            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();

            if (lockOpen)
                return TcpMsg.Error.ShopSlotLockOpenFailed;

            if (data_shop.slotUnlockLevel > level)
                return TcpMsg.Error.ShopSlotReadyFailed;

            lockOpen = true;
            var insert = new Table.TblShopSlotOpen()
            {
                uuid = UID,
                shopType = (int)data_shop.shopType,
                slotIndex = data_shop.slotIndex,
            };
            repoUser.InsertShopSlotOpen(insert);

            return TcpMsg.Error.None;
        }
        public override TcpMsg.Error ReadySlot(User user, Currency currency)
        {
            logger.Debug($"ReadySlot UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop.slotIndex}");

            if (!lockOpen)
                return TcpMsg.Error.ShopSlotReadyFailed;

            if (currency != null)
            {
                var result = currency.Consume(RefreshPriceType(), RefreshPriceValue(), user.tblUser.address, UserStateType.RefreshShopCardSlot, this);
                if (result == Currency.ConsumeState.ConsumeState_Request_Burn)
                {
                    return TcpMsg.Error.None;
                }
            }

            OnReadySlot(user);

            return TcpMsg.Error.None;
        }
        public override void OnReadySlot(User user)
        {
            logger.Debug($"OnReadySlot UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop.slotIndex}");

            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();
            repoUser.DeleteShopSlot(tblShop);
            repoUser.DeleteShopSlotCard(tblShopCard);

            tblShop = null;
            tblShopCard = null;

            var ack = new TcpMsg.AckShopSlotReady();
            ack.shopSlotInfo = ToMsg(user.tblUser.level);
            if (ack.shopSlotInfo == null)
                ack.errCode = TcpMsg.Error.ShopSlotReadyFailed;

            ack.currencyInfos = user.ToCurrencyInfoList();
            user.Send(new Packet(ack));
        }
        public override TcpMsg.Error OpenSlot(User user)
        {
            logger.Debug($"OpenSlot UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop.slotIndex}");

            if (!lockOpen)
                return TcpMsg.Error.ShopSlotOpenFailed;

            if (tblShop != null)
            {
                if (GetRemainSec() > 0)
                {
                    logger.Warn($"GetRemainSec() > 0 UserUID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop.slotIndex}");
                    return TcpMsg.Error.RemainShopSlotTime;
                }
                ReadySlot(user, null);
            }

            var cardids = ChooseCardIds();
            if (cardids.Count < 1)
            {
                logger.Warn($"cardids.Count < 1");
                return TcpMsg.Error.ShopSlotOpenFailed;
            }

            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();

            var insert = new Table.TblShopSlot()
            {
                uuid = UID,
                shopType = (int)data_shop.shopType,
                slotIndex = data_shop.slotIndex,
                resetTime = DateTime.Now.AddSeconds(data_shop.slotTimer),
                productType = (int)data_shop.productType,
                productSubType = data_shop.productSubType,
                sellCount = data_shop.sellCount,
                priceType = (int)data_shop.priceType,
                priceValue = data_shop.priceValue,
                itemName = data_shop.itemName,
                extraValue = data_shop.extraValue

            };
            repoUser.InsertShopSlot(insert);
            tblShop = insert;

            List<Table.TblShopSlotCard> insert_failed;
            (tblShopCard, insert_failed) = repoUser.InsertShopSlotCard(tblShop.suid, cardids.ToArray());
            foreach (var it in insert_failed)
                logger.Warn($"{Table.TblShopSlotCard.Name} Insert Fail UserUID={UID} SUID={it.suid} CardID={it.cardid}");

            return TcpMsg.Error.None;
        }
        public override Int64 GetRemainSec()
        {
            if (data_shop == null)
                return 0;

            if (data_shop.slotTimer == 0)
                return -1;

            if (tblShop == null)
                return 0;

            var calc_time = tblShop.resetTime - DateTime.Now;
            return (Int64)(calc_time.TotalSeconds > -1 ? calc_time.TotalSeconds : 0);
        }
        public override bool CheckBuy(string walletaddr, int level)
        {
            logger.Debug($"Buy UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");

            if (!base.CheckBuy(walletaddr, level))
            {
                return false;
            }

            if (GetRemainSec() == 0)
            {
                logger.Error($"GetRemainSec() == 0 UserUID={UID} ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex}");
                return false;
            }

            if (tblShopCard.Count < 1)
            {
                logger.Error($"tblShopCard.Count < 1 UserUID={UID} ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex}");
                return false;
            }

            return true;
        }
        public override TcpMsg.Error Buy(User user)
        {
            var consumeState = ConsumeCurrency(user.tblUser.address, user.tblUser.level);
            if (consumeState == Currency.ConsumeState.ConsumeState_Falied)
            {
				logger.Warn($"Buy UserID={UID} ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex} consumeState={consumeState}");
				return TcpMsg.Error.ShopBuyFailed;
            }

            var tokenTypeInfo = new List<LBD.TokenTypeInfo>();
            foreach (var it in tblShopCard)
            {
                var tokenType = LBD.LBDApiManager.Instance.NonFungibleTokenMetaToTokenTypeInfo(it.cardid.ToString());
                if (tokenType != null)
                    tokenTypeInfo.Add(tokenType);
                else
                    logger.Warn($"NonFungibleTokenMetaToTokenTypeInfo failed, it.cardid={it.cardid}");
            }

            var reqhttpData = new LBD.Msg.LBDMsg_MultiMintNonFungible()
            {
                uid = UID,
                guid = user.Id.ToString(),
                toAddr = string.Empty,
                toUserId = user.lineUID,
                mints = tokenTypeInfo,
            };

            SetMint(reqhttpData, UserStateType.BuyNonFungible);

			if (consumeState == Currency.ConsumeState.ConsumeState_Done)
			{
				return SendMint();
			}

			logger.Debug($"Buy UserID={UID} ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex} tblShopCard={tblShopCard.Count} consumeState={consumeState}");

			return TcpMsg.Error.None;
		}
        public override TcpMsg.Error OnBurn(User user, string tokenType, int amount)
        {
            var result = base.OnBurn(user, tokenType, amount);
            if (result != TcpMsg.Error.None)
            {
                return result;
            }

            return SendMint();
        }
        public override void OnBuy(object data, User user)
        {
            var addTblCards = data as List<LBD.TokenInfo>;
            logger.Debug($"OnBuy UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");

            ReadySlot(user, null);

            var ack = new TcpMsg.AckShopBuy();
            ack.errCode = TcpMsg.Error.None;
            ack.shopSlotInfo = ToMsg(user.tblUser.level);
            ack.currencyInfos = user.ToCurrencyInfoList();
            addTblCards.ForEach(x => ack.cards.Add(User.TokenInfoToCardInfo(x)));

            user.Send(new Packet(ack));
        }
        public static List<Int32> ChooseCardIds()
        {
            var cardids = new List<Int32>();

            var sumRate = 0;
            var countPickGroups = TCGGameSrv.ResourceDataLoader.Data_CountPick_List.GroupBy(g => g.groupID);
            foreach (var countPickGroup in countPickGroups)
            {
                sumRate += countPickGroup.FirstOrDefault().groupRate;
            }

            var random = new Random((int)DateTime.Now.Ticks);
            var ran = random.Next(0, sumRate - 1);
            foreach (var countPickGroup in countPickGroups)
            {
                ran -= countPickGroup.FirstOrDefault().groupRate;
                if (ran < 0)
                {
                    var countPick = countPickGroup.PickRandom();

                    var gradeCounts = new Dictionary<Int32, Int32>()
                                    {
                                        {1, countPick.commonGradeCount},
                                        {2, countPick.uncommonGradeCount},
                                        {3, countPick.rareGradeCount},
                                        {4, countPick.superGradeCount},
                                        {5, countPick.ultraGradeCount},
                                        {6, countPick.heroCardCount}
                                    };

                    var randomSelectGroups = TCGGameSrv.ResourceDataLoader.Data_RandomSelect_List.GroupBy(g => g.grade);
                    foreach (var randomSelectGroup in randomSelectGroups)
                    {
                        var gradeCount = gradeCounts[randomSelectGroup.Key];
                        for (int i = 0; i < gradeCount; ++i)
                        {
                            sumRate = 0;
                            foreach (var randomSelect in randomSelectGroup.ToList())
                            {
                                sumRate += randomSelect.pickRate;
                            }

                            ran = random.Next(0, sumRate - 1);
                            foreach (var randomSelect in randomSelectGroup.ToList())
                            {
                                ran -= randomSelect.pickRate;
                                if (ran < 0)
                                {
                                    cardids.Add(randomSelect.cardID);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return cardids;
        }
    }
}
