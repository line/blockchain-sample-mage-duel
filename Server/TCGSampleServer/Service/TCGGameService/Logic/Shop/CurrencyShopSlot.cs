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
	public class CurrencyShopSlot : ShopSlot
	{
        private Currency product_currency;
        private Resource.Data_Price data_price;

        public CurrencyShopSlot(User user, Resource.Data_Shop data_shop, Dictionary<Resource.PriceType, Currency> currencies) : base(user, data_shop, currencies)
        {
            data_price = TCGGameSrv.ResourceDataLoader.Data_Price_List.Where(x => x.priceType == (Resource.PriceType)data_shop.productSubType).FirstOrDefault();
            product_currency = currencies[data_price.priceType];
        }
        public override TcpMsg.Error Buy(User user)
        {
            var consumeState = ConsumeCurrency(user.tblUser.address, user.tblUser.level);
            if (consumeState == Currency.ConsumeState.ConsumeState_Falied)
            {
                return TcpMsg.Error.ShopBuyFailed;
            }

            if (product_currency.currencyType == Currency.CurrencyType.Currency_token)
            {
                LBD.Msg.LBDBaseMsg req = null;
                if (data_price.fungibleType == Resource.FungibleType.Fungible)
                {
                    req = new LBD.Msg.LBDMsg_MintFungible()
                    {
                        uid = UID,
                        guid = user.Id.ToString(),
                        toAddr = string.Empty,
                        toUserId = user.lineUID,
                        tokenType = product_currency.TokenType(),
                        amount = (int)data_shop.sellCount
                    };

                    /* TODO : belldan
                     * Operator Addr 에 Mint 후 User에게 지급 할때 사용
                    req = new LBD.Msg.LBDMsg_Wallet_Fungible_Transfer()
                    {
                        uid = UID,
                        guid = user.Id.ToString(),
                        fromAddr = Setting.ProgramSetting.Instance.lbdInfo.operatorAddr,
                        fromSecret = Setting.ProgramSetting.Instance.lbdInfo.secretKey,
                        toAddr = string.Empty,
                        toUserId = user.lineUID,
                        tokenType = product_currency.TokenType(),
                        amount = (int)data_shop.sellCount
                    };
                    */
                }
                else if (data_price.fungibleType == Resource.FungibleType.ServiceToken)
                {
                    req = new LBD.Msg.LBDMsg_TransferServiceToken()
                    {
                        uid = UID,
                        guid = user.Id.ToString(),
                        contractId = product_currency.TokenType(),
                        fromAddr = Setting.ProgramSetting.Instance.lbdInfo.operatorAddr,
                        fromSecret = Setting.ProgramSetting.Instance.lbdInfo.secretKey,
                        toAddr = string.Empty,
                        toUserId = user.lineUID,
                        amount = (int)data_shop.sellCount
                    };
                }
                else
                {
                    return TcpMsg.Error.ShopBuyFailed;
                }
                
                SetMint(req, UserStateType.BuyFungible);

                if (consumeState == Currency.ConsumeState.ConsumeState_Done)
                {
                    SendMint();
                }
            }
            else
            {
                if (consumeState == Currency.ConsumeState.ConsumeState_Done)
                {
                    OnBuy(new Dictionary<string, Int64>() { { product_currency.TokenType(), data_shop.sellCount } }, user);
                }
            }

            logger.Debug($"Buy UserID={UID} ShopType={data_shop.shopType} SlotIndex={data_shop.slotIndex} consumeState={consumeState}");

            return TcpMsg.Error.None;
        }
        public override TcpMsg.Error OnBurn(User user, string tokenType, int amount)
        {
            var result = base.OnBurn(user, tokenType, amount);
            if (result != TcpMsg.Error.None)
            {
                return result;
            }

            if (product_currency.currencyType == Currency.CurrencyType.Currency_token)
            {
                return SendMint();
            }
            else
            {
                OnBuy(new Dictionary<string, Int64>() { { product_currency.TokenType(), data_shop.sellCount } }, user);
                return TcpMsg.Error.None;
            }
        }
        public override void OnBuy(object data, User user)
        {
            logger.Debug($"OnBuy UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");

            var dict = data as Dictionary<string, Int64>;
            if(dict == null || dict.Count != 1)
            {
                logger.Error($"OnBuy invalid dict, UserID={UID} ShopType={data_shop?.shopType} SlotIndex={data_shop?.slotIndex}");
                return;
            }

            product_currency.SetPrice(dict.First().Key, dict.First().Value);

            var ack = new TcpMsg.AckShopBuy();
            ack.errCode = TcpMsg.Error.None;
            ack.shopSlotInfo = ToMsg(user.tblUser.level);
            ack.currencyInfos = user.ToCurrencyInfoList();

            user.Send(new Packet(ack));
        }
    }
}
