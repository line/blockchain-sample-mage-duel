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
    public partial class User
    {
        List<Table.TblTrade> tblTrades = new List<Table.TblTrade>();

        public Table.TblTrade GetTblTrade(Int64 goodsSeq)
        {
            return tblTrades.Find(x => x.seq == goodsSeq);
        }

        public bool TblTradeRemove(Int64 seq)
        {
            var ret = false;
            var tblTrade = tblTrades.Find(x => x.seq == seq);

            if (null != tblTrade)
            {
                ret = tblTrades.Remove(tblTrade);
                logger.Debug($" Memory Remove UID={UID} TokenType={tblTrade.tokentype} TokenIndex={tblTrade.tokenidx}");
            }

            return ret;
        }

        public (TcpMsg.Error, LBD.TokenInfo) IsTradePossibleCard(string tokenid)
        {
            var tokenInfo = nonFungibles.Find(x => x.GetTokenId() == tokenid);

            var errorCode = TcpMsg.Error.None;
            if (null == tokenInfo)
            {
                errorCode = TcpMsg.Error.NotFoundTokenInfo;
                tokenInfo = null;
            }
            else if (IsDeckCard(tokenid))
            {
                errorCode = TcpMsg.Error.AlreadySetDeckCard;
                tokenInfo = null;
            }
            else
            {
                var tblTrade = tblTrades.Find(x => x.tokentype == tokenInfo.tokenType && x.tokenidx == tokenInfo.tokenIdx);

                if (null != tblTrade)
                {
                    errorCode = TcpMsg.Error.AlreadyTradeCard;
                    tokenInfo = null;
                }
                /*
                var repoTrade = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();
                if (repoTrade.ExistsTrade(tokenInfo.tokenType, tokenInfo.tokenIdx))
                {
                    errorCode = TcpMsg.Error.AlreadyTradeCard;
                    tokenInfo = null;
                }
                */
            }

            if (errorCode != TcpMsg.Error.None)
            {
                logger.Warn($"{errorCode.ToString()} TokenId={tokenid}");
            }

            return (errorCode, tokenInfo);
        }

        public List<TcpMsg.TradeGoodsInfo> GetTradeGoods()
        {
            tblTrades = TradingManager.Instance.CheckGetTblTrades(UID);

            List<TcpMsg.TradeGoodsInfo> goodsInfo = new List<TcpMsg.TradeGoodsInfo>();

            if (null != tblTrades && tblTrades.Count > 0)
                goodsInfo = tblTrades.Select(x => TradingManager.Instance.TblTradeToTradeGoodsInfo(x)).ToList();

            return goodsInfo;
        }

        public void RegisterTradeGoodsSucess(Table.TblTrade tblTrade)
        {
            var errorCode = TcpMsg.Error.None;
            if (null == tblTrade)
            {
                errorCode = TcpMsg.Error.RegisterGoodsFail;
                logger.Warn($"Table Trade Data null UserUid = {UID}");
            }

            var ackData = new TcpMsg.AckRegisterTradeGoods();

            if (errorCode == TcpMsg.Error.None)
            {
                tblTrades.Add(tblTrade);

                ackData.goodsInfo = TradingManager.Instance.TblTradeToTradeGoodsInfo(tblTrade);
                ackData.currencyInfos = ToCurrencyInfoList();
            }

            ackData.errCode = errorCode;

            Send(new Packet(ackData));
        }
    }
}
