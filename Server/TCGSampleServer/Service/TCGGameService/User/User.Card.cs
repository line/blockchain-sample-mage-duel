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
        public List<LBD.TokenInfo> nonFungibles;

        public void NonFungibleToken_Add(List<LBD.TokenInfo> tokenInfos)
        {
            tokenInfos.ForEach(x => logger.Debug($"NonFungible TokenType={x.tokenType} Name={x.name} Meta={x.meta} Amount={x.amount}"));

            if (null == nonFungibles)
                nonFungibles = new List<LBD.TokenInfo>();

            nonFungibles.AddRange(tokenInfos);
        }

        public LBD.TokenInfo NonFungibleToken_Remove(string tokenId)
        {
            var tokenInfo = nonFungibles.Find(x => x.GetTokenId() == tokenId);

            if (null != tokenInfo)
            {
                nonFungibles.Remove(tokenInfo);
                logger.Debug($" Memory Remove NonFungibleToken UID={UID} TokenType={tokenInfo.tokenType} TokenIndex={tokenInfo.tokenIdx}");
            }

            return tokenInfo;
        }

        public void SetNonFungibleTokens(string tokenType, List<LBD.NonFungibleTokenIndexBalancesData> tokenDatas)
        {
            tokenDatas.ForEach(x => logger.Debug($"TokenType={tokenType} TokenIndex={x.tokenIndex} Name={x.name} Meta={x.meta}"));

            if (tokenDatas.Count > 0)
            {
                if (null == nonFungibles)
                    nonFungibles = new List<LBD.TokenInfo>();

                var tempnonFungibles = tokenDatas.Select(x => new LBD.TokenInfo()
                {
                    tokenType = tokenType,
                    tokenIdx = x.tokenIndex,
                    name = x.name,
                    meta = x.meta,
                    amount = 1
                }).ToList();

                foreach (var nft in tempnonFungibles)
                {
                    if (null != nft)
                    {
                        var nftoken = nonFungibles.Find(x => x.tokenType == nft.tokenType && x.tokenIdx == nft.tokenIdx);

                        if (null != nftoken)
                            continue;

                        nonFungibles.Add(nft);
                    }
                }
            }
            else
            {
                logger.Info($"There is no NonFungible Token Uid={UID}");
            }
        }
    }
}
