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
using System.Text;
using System.Threading.Tasks;

namespace TCGGameService
{
    public partial class User
    {
        Int32 ComposableTokenRequest = 0;
        Int32 ComposableTokenReceive = 0;

        public List<Table.TblDeck> tblDecks;
        public List<TcpMsg.DeckInfo> deckInfos;

        public TcpMsg.Error CheckDeckCount(string deckuid)
        {
            var result = TcpMsg.Error.None;
            var deckInfo = deckInfos.Find(x => x.GetDeckUid() == deckuid);

            if (null == deckInfo)
            {
                result = TcpMsg.Error.NotFoundDeckInfo;
            }
            else if (deckInfo.unitList.Count >= 40)
            {
                result = TcpMsg.Error.DeckUnitMaxCount;
            }

            return result;
        }

        public bool IsDeckCard(string tokenid)
        {
            var cardList = deckInfos.SelectMany(x => x.unitList).ToList();
            return cardList.Exists(x => x.GetCardID() == tokenid);
        }

        public void DBSelectDeck()
        {
            var heroIdList = TCGGameSrv.ResourceDataLoader.Data_Card_List.Where(x => x.cardType == Resource.CardType.hero).Select(x => x.ID).ToList();

            var heroTokens = new List<LBD.TokenInfo>();

            foreach (var meta in heroIdList)
            {
                var token = nonFungibles.Find(x => x.meta == meta.ToString());
                if (null != token)
                    heroTokens.Add(token);
            }

            if (heroTokens.Count > 0)
            {
                var tokenIds = heroTokens.Select(x => x.GetTokenId()).ToArray();
                var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();

                if (null == tblDecks)
                {
                    tblDecks = new List<Table.TblDeck>();
                }

                foreach(var tokenid in tokenIds)
                {
                    var tblDeck = repoUser.GetDeck(tokenid);

                    if (null == tblDeck)
                    {
                        tblDeck = new Table.TblDeck()
                        {
                            tokenid = tokenid,
                            deckname = "NoName",
                            regDate = DateTime.Now
                        };

                        tblDeck.seq = repoUser.InsertDeck(tblDeck);
                        tblDecks.Add(tblDeck);
                    }
                    else
                    {
                        tblDecks.Add(tblDeck);
                    }
                }

                if (null != tblDecks)
                {
                    ComposableTokenRequest = tblDecks.Count;
                    foreach (var tblDeck in tblDecks)
                    {
                        LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_GetComposableToken()
                        {
                            uid = UID,
                            guid = Id.ToString(),
                            type = TcpMsg.GetComposableType.GetTokenChildren,
                            tokenType = LBD.LBDApiManager.TokenIdToTokenType(tblDeck.tokenid),
                            tokenIdx = LBD.LBDApiManager.TokenIdToTokenIndex(tblDeck.tokenid),

                            limit = 0,
                            orderBy = string.Empty,
                            page = 0,

                            value = string.Empty
                        });
                    }
                }
                else
                {
                    if (stateType == UserStateType.InitGetDeck)
                    {
                        UserData();
                    }
                }
            }
            else
            {
                if (stateType == UserStateType.InitGetDeck)
                {
                    UserData();
                }
            }
        }

        public void SetDeckChildren(string parenttokenType, string parentTokenIdx, List<LBD.GetComposableData> childrenTokens)
        {
            var deckInfo = new TcpMsg.DeckInfo();
            var tblDeck = tblDecks.Find(x => x.tokenid == $"{parenttokenType}{parentTokenIdx}");

            if (null != tblDeck)
            {
                if (null == deckInfos)
                    deckInfos = new List<TcpMsg.DeckInfo>();

                deckInfo.name = tblDeck.deckname;
                var parentTokenInfo = nonFungibles.Find(x => x.tokenType == parenttokenType && x.tokenIdx == parentTokenIdx);
                deckInfo.heroCard = TokenInfoToCardInfo(parentTokenInfo);

                deckInfo.unitList = new List<TcpMsg.CardInfo>();
                foreach (var ctokenData in childrenTokens)
                {
                    var childrenTokenInfo = nonFungibles.Find(x => x.GetTokenId() == ctokenData.tokenId);
                    if (null != childrenTokenInfo)
                    {
                        deckInfo.unitList.Add(TokenInfoToCardInfo(childrenTokenInfo));
                    }
                }

                deckInfos.Add(deckInfo);
            }
            ComposableTokenReceive++;

            if (ComposableTokenRequest == ComposableTokenReceive)
            {
                // Load complete
                ComposableTokenRequest = 0;
                ComposableTokenReceive = 0;

                if (stateType == UserStateType.InitGetDeck)
                {
                    UserData();
                }
            }
        }

        public void RegisterDeck(string tokenId, string deckName)
        {
            var tokenInfo = nonFungibles.Find(x => x.GetTokenId() == tokenId);
            var ackData = new TcpMsg.AckRegisterDeck();

            if (null == tokenInfo)
            {
                ackData.errCode = TcpMsg.Error.NotFoundTokenInfo;
            }
            else
            {
                var resourceCardData = TCGGameSrv.ResourceDataLoader.Data_Card_Dictionary_ID[Convert.ToInt32(tokenInfo.meta)];

                if (null == resourceCardData)
                {
                    ackData.errCode = TcpMsg.Error.ResourcdNotFound;
                }
                else
                {
                    if(resourceCardData.cardType != Resource.CardType.hero)
                    {
                        ackData.errCode = TcpMsg.Error.NotHeroCard;
                    }
                    else
                    {
                        if (null == deckInfos)
                            deckInfos = new List<TcpMsg.DeckInfo>();

                        var deck = deckInfos.Find(x => x.GetDeckUid() == tokenId);
                        if (null != deck)
                        {
                            ackData.errCode = TcpMsg.Error.Deck_Already_Exists;
                        }
                        else
                        {
                            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();

                            var tblDeck = new Table.TblDeck()
                            {
                                tokenid = tokenId,
                                deckname = deckName,
                                regDate = DateTime.Now
                            };

                            tblDeck.seq = repoUser.InsertDeck(tblDeck);
                            tblDecks.Add(tblDeck);

                            deck = new TcpMsg.DeckInfo()
                            {
                                name = deckName,
                                heroCard = TokenInfoToCardInfo(tokenInfo),
                                unitList = new List<TcpMsg.CardInfo>()
                            };

                            deckInfos.Add(deck);
                            ackData.deckInfo = deck;
                        }
                    }
                }
            }

            if (ackData.errCode != TcpMsg.Error.None)
            {
                logger.Warn($"UID={UID} Error Code={ackData.errCode.ToString()}");
            }

            Send(new Packet(ackData));
        }

        public void DeckNameChange(string deckuid, string deckName)
        {
            var deck = deckInfos.Find(x => x.GetDeckUid() == deckuid);
            var ackData = new TcpMsg.AckDeckNameChange();

            if (null == deck)
            {
                ackData.errCode = TcpMsg.Error.NotFoundDeckInfo;
            }
            else
            {
                var tblDeck = tblDecks.Find(x => x.tokenid == deckuid);

                if (string.IsNullOrEmpty(deckName))
                {
                    ackData.errCode = TcpMsg.Error.IsNullOrEmpty_DeckName;
                }
                else if (tblDeck.deckname == deckName)
                {
                    ackData.errCode = TcpMsg.Error.Deck_SameName;
                }
                else
                {
                    var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();
                    tblDeck.deckname = deckName;

                    if (!repoUser.UpdateDeck(tblDeck))
                    {
                        ackData.errCode = TcpMsg.Error.DBUpdateError;
                    }
                    else
                    {
                        deck.name = tblDeck.deckname;
                        ackData.deckInfo = deck;
                    }
                }
            }

            if (ackData.errCode != TcpMsg.Error.None)
            {
                logger.Warn($"UID={UID} Error Code={ackData.errCode.ToString()}");
            }

            Send(new Packet(ackData));
        }

        public void AddComposableTokenInfo(LBD.TokenInfo ptokenInfo, LBD.TokenInfo ctokenInfo)
        {
            var deckInfo = deckInfos.Find(x => x.GetDeckUid() == ptokenInfo.GetTokenId());

            var ackData = new TcpMsg.AckDeckChildrenCardAdd();
            if (null != deckInfo)
            {
                deckInfo.unitList.Add(TokenInfoToCardInfo(ctokenInfo));

                ackData.deckInfo = deckInfo;
            }
            else
            {
                ackData.errCode = TcpMsg.Error.NotFoundDeckInfo;
            }

            if (ackData.errCode != TcpMsg.Error.None)
            {
                logger.Warn($"UID={UID} Error Code={ackData.errCode.ToString()}");
            }

            stateType = UserStateType.None;
            Send(new Packet(ackData));
        }

        public void RemoveComposableTokenInfo(LBD.TokenInfo ptokenInfo, LBD.TokenInfo ctokenInfo)
        {
            var deckInfo = deckInfos.Find(x => x.GetDeckUid() == ptokenInfo.GetTokenId());

            var ackData = new TcpMsg.AckDeckChildrenCardRemove();
            if (null != deckInfo)
            {
                var removeToken = deckInfo.unitList.Find(x => x.GetCardID() == ctokenInfo.GetTokenId());

                if (null != removeToken)
                {
                    deckInfo.unitList.Remove(removeToken);
                    ackData.deckInfo = deckInfo;
                }
                else
                {
                    ackData.errCode = TcpMsg.Error.NotFoundTokenInfo;
                }
            }
            else
            {
                ackData.errCode = TcpMsg.Error.NotFoundDeckInfo;
            }

            if (ackData.errCode != TcpMsg.Error.None)
            {
                logger.Warn($"UID={UID} Error Code={ackData.errCode.ToString()}");
            }

            stateType = UserStateType.None;
            Send(new Packet(ackData));
        }
    }
}
