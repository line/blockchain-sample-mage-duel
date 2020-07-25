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
    public partial class UserHandler
    {
        public void ReqTradeInfo(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqTradeInfo>();

            if (null != user)
            {
                var result = TradingManager.Instance.GetTradeGoodsList(reqData);

                var ackData = new TcpMsg.AckTradeInfo()
                {
                    itemCountPerPage = reqData.itemCountPerPage,
                    totalCount = result.Item2,
                    pageNum = result.Item1,
                    goodsInfos = result.Item3
                };

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqRegisterTradeGoods(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqRegisterTradeGoods>();

            if (null != user)
            {
                var spendCoin = Define.GetCoinFees(reqData.sale_price);

                logger.Debug($"ReqRegisterTradeGoods UserID={user.UID} SalePrice={reqData.sale_price} SpendRuby={spendCoin}");

                var tokenTypeInfo = LBD.LBDApiManager.Instance.GetFungibleTypeToMeta(((Int32)Resource.PriceType.Coin).ToString());

                var errorCode = TcpMsg.Error.None;
                var currencyInfo = user.CheckFungibleToken(tokenTypeInfo.tokenType, spendCoin);
                errorCode = currencyInfo.Item1;

                if (TcpMsg.Error.None == errorCode)
                {
                    var currency = currencyInfo.Item2 as TCurrency;

                    var tokenData = user.IsTradePossibleCard(reqData.tokenid);
                    errorCode = tokenData.Item1;

                    if (TcpMsg.Error.None == errorCode)
                    {
                        var sellerData = new SellerData()
                        {
                            uuid = user.UID,
                            userid = user.lineUID,
                            nickName = user.nickName,
                            card = tokenData.Item2,
                            comment = reqData.comment,
                            sale_price = reqData.sale_price
                        };

                        if (TradingManager.Instance.AddSellerData(sellerData))
                        {
                            user.stateType = UserStateType.RegisterTradeGoodsFees;
                            user.WithdrawFungible(currency.TokenType(), spendCoin);
                            return;
                        }
                        else
                        {
                            errorCode = TcpMsg.Error.RegisterGoodsFail;
                        }
                    }
                }

                logger.Warn($"{errorCode.ToString()}");

                var ackData = new TcpMsg.AckRegisterTradeGoods()
                {
                    errCode = errorCode,
                    goodsInfo = null
                };

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqRegisterTradeGoodsCancel(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqRegisterTradeGoodsCancel>();

            if (null != user)
            {
                var tblTrade = user.GetTblTrade(reqData.goodsSeq);

                TcpMsg.Error errorCode = TcpMsg.Error.None;
                TcpMsg.TradeGoodsInfo goodsInfo = null;

                if (null != tblTrade)
                {
                    var dbtblTrade = TradingManager.Instance.GetTradeGoods(reqData.goodsSeq);

                    if (dbtblTrade.status == (Int32)TcpMsg.TradeStatus.Onsale)
                    {
                        if (user.TblTradeRemove(reqData.goodsSeq))
                        {
                            if (TradingManager.Instance.DeleteTradeGoods(tblTrade.seq))
                            {
                                goodsInfo = TradingManager.Instance.TblTradeToTradeGoodsInfo(tblTrade);
                            }
                            else
                            {
                                errorCode = TcpMsg.Error.DataBaseTradeDeleteFail;
                            }
                        }
                        else
                        {
                            errorCode = TcpMsg.Error.ServerTradeDeleteFail;
                        }
                    }
                    else
                    {
                        errorCode = TcpMsg.Error.NotOnsale;
                    }
                }
                else
                {
                    errorCode = TcpMsg.Error.NotFoundGoodsOrTradeing;
                }

                if (errorCode != TcpMsg.Error.None)
                {
                    logger.Warn($"UID={user.UID} ErrorCode={errorCode.ToString()}");
                }

                var ackData = new TcpMsg.AckRegisterTradeGoodsCancel()
                {
                    errCode = errorCode,
                    goodsInfo = goodsInfo
                };

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqTradeGoodsBuy(NetMessage message)
        {
            if (null != user)
            {
                var reqData = message.GetData<TcpMsg.ReqTradeGoodsBuy>();

                var tblTrade = TradingManager.Instance.GetTradeGoods(reqData.goodsSeq, TcpMsg.TradeStatus.Onsale);

                var errorCode = TcpMsg.Error.None;
                if (null != tblTrade)
                {
                    if (tblTrade.uuid != user.UID)
                    {                      
                        var tokenTypeInfo = LBD.LBDApiManager.Instance.GetFungibleTypeToMeta(((Int32)Resource.PriceType.Coin).ToString());

                        var currencyInfo = user.CheckFungibleToken(tokenTypeInfo.tokenType, tblTrade.sale_price);
                        errorCode = currencyInfo.Item1;

                        if (TcpMsg.Error.None == errorCode)
                        {
                            var currency = currencyInfo.Item2 as TCurrency;
                            var tokenType = currency.TokenType();
                            var buyerData = new BuyerData()
                            {
                                uuid = user.UID,
                                userid = user.lineUID,
                                nickName = user.nickName,
                                buyTokenType = tokenType,
                                goods = tblTrade
                            };

                            if (TradingManager.Instance.AddBuyerData(buyerData))
                            {
                                user.stateType = UserStateType.SellerPayments;
                                LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_UserId_Fungible_Transfer()
                                {
                                    uid = user.UID,
                                    guid = user.Id.ToString(),
                                    fromUserId = user.lineUID,
                                    toAddr = string.Empty,
                                    toUserId = tblTrade.userid,
                                    tokenType = buyerData.buyTokenType,
                                    amount = tblTrade.sale_price
                                });
                                return;
                            }
                            else
                            {
                                errorCode = TcpMsg.Error.NotFoundGoodsOrTradeing;
                            }
                        }
                    }
                    else
                    {
                        errorCode = TcpMsg.Error.NotTradeSelfGoods;
                    }
                }
                else
                {
                    errorCode = TcpMsg.Error.NotFoundGoodsOrTradeing;
                }

                logger.Warn($"{errorCode.ToString()}");

                var ackData = new TcpMsg.AckTradeGoodsBuy()
                {
                    errCode = errorCode
                };

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }



        public void ReqTradeHistory(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqTradeHistory>();

            if (null != user)
            {
                var result = TradingManager.Instance.GetTradeHistory(user.UID, reqData.pageNum, reqData.itemCountPerPage);

                var ackData = new TcpMsg.AckTradeHistory()
                {
                    itemCountPerPage = reqData.itemCountPerPage,
                    totalCount = result.Item1,
                    pageNum = reqData.pageNum,
                    tradeHistorys = result.Item2
                };

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }
    }
}
