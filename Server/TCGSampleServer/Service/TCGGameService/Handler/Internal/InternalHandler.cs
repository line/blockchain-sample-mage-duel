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

namespace TCGGameService
{
    public partial class InternalHandler
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        User user = null;

        public InternalHandler(User user)
        {
            this.user = user;
        }

        public void IntlMsg_SellerPayments(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_SellerPayments;

            if (null != user)
            {
                user.Currency_SetPrice(intlMsg.tokenType, intlMsg.amount);

                logger.Info($"FromUid={intlMsg.fromUserUid} TokenType={intlMsg.tokenType} Amount={intlMsg.amount}");
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void IntlMsg_BuyerGiveGoods(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_BuyerGiveGoods;

            if (null != user)
            {
                var tblTrade = user.GetTblTrade(intlMsg.goodsSeq);

                if (null != tblTrade)
                {
                    var tokenId = $"{tblTrade.tokentype}{tblTrade.tokenidx}";
                    var tokenInfo = user.NonFungibleToken_Remove(tokenId);
                    if (null != tokenInfo)
                    {
                        user.TblTradeRemove(tblTrade.seq);

                        var ackData = new TcpMsg.AckTradeBuyerGiveGoods()
                        {
                            goodsSeq = tblTrade.seq,
                            fromuuid = user.UID,
                            touuid = intlMsg.touuid,
                            cardInfo = User.TokenInfoToCardInfo(tokenInfo),
                            currencyinfos = user.ToCurrencyInfoList()
                        };

                        user.Send(new Packet(ackData));
                    }
                    else
                    {
                        logger.Warn($"NonFungibleToken_Remove fail TokenID={tokenId}");
                    }
                }
                else
                {
                    logger.Warn($"TableTrade Not Found GoodsSeq={intlMsg.goodsSeq}");
                }
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }
    }
}
