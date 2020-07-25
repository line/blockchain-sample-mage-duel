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
    public partial class User
    {
        UserMessageDispatcher tcpDispatcher = null;
        InternalMessageDispatcher internalDispatcher = null;

        public Int64 UID => tblUser?.uuid ?? 0;
        public string lineUID => tblUser?.lineuserid ?? "";
        public string nickName => tblUser?.nickname ?? "";
        public Table.TblUser tblUser { get; set; }

        string displayName { get; set; }
        string pictureUrl { get; set; }
        string statusMessage { get; set; }

        private DateTime keep_Alive = DateTime.Now;
        private DateTime check_Time = DateTime.Now;
        public Int32 checkTime = Setting.ProgramSetting.Instance.serverHostInfo.keepAliveCheckTime;
        public Int32 sendingCycle = Setting.ProgramSetting.Instance.serverHostInfo.sendingCycle;

        public UserStateType stateType = UserStateType.None;

        public string walletAddr { get; set; } = string.Empty;

        string accessToken { get; set; } = string.Empty;

        public void CheckTimeNowUpdate()
        {
            check_Time = DateTime.Now;
        }

        public void SetAccessToken(string token) => accessToken = token;

        public string GetAccessToken()
        {
            return accessToken;
        }

        List<LBD.TokenTypeInfo> DefaultCardToTokenTypeInfo()
        {
            var result = new List<LBD.TokenTypeInfo>();

            foreach (var defaultCard in TCGGameSrv.ResourceDataLoader.Data_DefaultDeck_List)
            {
                var tokenType = LBD.LBDApiManager.Instance.NonFungibleTokenMetaToTokenTypeInfo(defaultCard.heroCard.ToString());
                if (null != tokenType)
                    result.Add(tokenType);
                else
                    logger.Warn($"not found DefaultHeroCard mate data to tokentypeinfo meta={defaultCard.heroCard.ToString()} ");

                foreach(var meta in defaultCard.deckCard)
                {
                    tokenType = LBD.LBDApiManager.Instance.NonFungibleTokenMetaToTokenTypeInfo(meta.ToString());
                    if (null != tokenType)
                        result.Add(tokenType);
                    else
                        logger.Warn($"not found DefaultCard mate data to tokentypeinfo meta={defaultCard.heroCard.ToString()} ");
                }
            }
            return result;
        }

        public static TcpMsg.CardInfo TokenInfoToCardInfo(LBD.TokenInfo tokenInfo)
        {
            var cardInfo = new TcpMsg.CardInfo()
            {
                tokenType = tokenInfo.tokenType,
                tokenIndex = tokenInfo.tokenIdx,
                meta = Convert.ToInt32(tokenInfo.meta)
            };

            return cardInfo;
        }

        List<TcpMsg.CardInfo> NonFungiblesToCardInfo()
        {
            var cardInfos = new List<TcpMsg.CardInfo>();
            nonFungibles.ForEach(x => cardInfos.Add(TokenInfoToCardInfo(x)));
            return cardInfos;
        }
    }
}
