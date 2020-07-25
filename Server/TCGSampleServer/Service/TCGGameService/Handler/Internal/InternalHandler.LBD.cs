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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TCGGameService
{
    public partial class InternalHandler
    {
        Int32 TokenTypeBalancesRequest = 0;
        Int32 TokenTypeBalancesReceive = 0;

        List<LBD.NonFungibleTokenTypeBalancesData> nftTypeBalances;
        Dictionary<string, List<LBD.NonFungibleTokenIndexBalancesData>> nftIndexBalances;



        public void IntlMsg_GetUser(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_GetUser;
            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<LBD.Respones_GetUserInfio>(intlMsg.result);

                if (null != user)
                {
                    if (!string.IsNullOrEmpty(resData.responseData.walletAddress))
                    {
                        user.walletAddr = resData.responseData.walletAddress;

                        LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_GetProxy()
                        {
                            uid = user.UID,
                            guid = user.Id.ToString(),
                            lineuserid = user.lineUID
                        });
                    }
                    else
                    {
                        // Is not LBW member
                        TcpMsg.MessageResponse ackData;
                        if (user.stateType == UserStateType.CreateNickName)
                        {
                            ackData = new TcpMsg.AckCreateNickName()
                            {
                                errCode = TcpMsg.Error.IsNot_LBW_Member
                            };
                        }
                        else if(user.stateType == UserStateType.ExistingUserConnect)
                        {
                            ackData = new TcpMsg.AckVerifyAccessToken()
                            {
                                errCode = TcpMsg.Error.IsNot_LBW_Member,
                                authType = TcpMsg.AuthType.IsNot_LBW_Member
                            };
                        }
                        else
                        {
                            logger.Warn($"User is not an LBW member User StateError StateType={user.stateType.ToString()}");
                            return;
                        }
                        logger.Warn($"User is not an LBW member StateType={user.stateType.ToString()}");
                        user.Send(new Packet(ackData));
                    }
                }
                else
                {
                    logger.Warn($"Is User Null~~~");
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public void IntlMsg_GetProxy(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_GetProxy;

            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<LBD.Respones_GetProxyInfo>(intlMsg.result);

                if (null != user)
                {
                    if (resData.responseData.isApproved)
                    {
                        TcpMsg.MessageResponse ackData;
                        if (user.stateType == UserStateType.CreateNickName)
                        {
                            ackData = new TcpMsg.AckCreateNickName();
                        }
                        else if (user.stateType == UserStateType.ExistingUserConnect)
                        {
                            ackData = new TcpMsg.AckVerifyAccessToken()
                            {
                                authType = TcpMsg.AuthType.AuthSucess
                            };
                        }
                        else
                        {
                            logger.Warn($"User is not Proxy Approved User StateError StateType={user.stateType.ToString()}");
                            return;
                        }
                        user.Send(new Packet(ackData));
                    }
                    else
                    {
                        logger.Debug($"User is not Proxy Approved RequestProxy UID={user.UID}");

                        LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_RequestProxy()
                        {
                            uid = user.UID,
                            guid = user.Id.ToString(),
                            fromUserId = user.lineUID,
                            landingUri = string.Empty
                        });

                        var ackData = new TcpMsg.AckRequestProxy();
                        user.Send(new Packet(ackData));
                    }
                }
                else
                {
                    logger.Warn($"Is User Null~~~");
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public void IntlMsg_TokenBalancesOf_Fungible(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_TokenBalancesOf_Fungible;

            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<LBD.Respones_GetFungibleTokenTypeBalances>(intlMsg.result);

                if (null != user)
                {
                    user.SetFungibleTokensBalances(resData.responseData);

                    if (user.stateType == UserStateType.InitGetFungibleToken)
                    {
                        user.stateType = UserStateType.InitGetServiceToken;
                        LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenBalancesOf_ServiceToken()
                        {
                            uid = user.UID,
                            guid = user.Id.ToString(),
                            toAddr = string.Empty,
                            toUserId = user.lineUID,

                            limit = 50,
                            orderBy = string.Empty,
                            page = 1
                        });
                    }
                }
                else
                {
                    logger.Warn($"Is User Null~~~");
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public void IntlMsg_TokenBalancesOf_ServiceToken(InternalMsg.InternalBaseMsg msg)
        {
            if (null == user)
            {
                logger.Warn($"Is User Null~~~");
                return;
            }

            var intlMsg = msg as InternalMsg.IntlMsg_TokenBalancesOf_ServiceToken;

            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<LBD.Respones_GetServiceTokenBalance>(intlMsg.result);

                user.SetServiceTokensBalances(resData.responseData);

                if (resData.responseData.Count >= 50)
                {
                    LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenBalancesOf_ServiceToken()
                    {
                        uid = user.UID,
                        guid = user.Id.ToString(),
                        toAddr = string.Empty,
                        toUserId = user.lineUID,

                        limit = 50,
                        orderBy = string.Empty,
                        page = intlMsg.page + 1
                    });
                    return;
                }

                if (user.stateType == UserStateType.InitGetServiceToken)
                {
                    user.stateType = UserStateType.InitGetNonFungibleTokenType;
                    LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenBalancesOf_NonFungible()
                    {
                        uid = user.UID,
                        guid = user.Id.ToString(),
                        toAddr = string.Empty,
                        toUserId = user.lineUID,

                        limit = 50,
                        orderBy = string.Empty,
                        page = 1
                    });
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public void IntlMsg_TokenBalancesOf_NonFungible(InternalMsg.InternalBaseMsg msg)
        {
            if (null == user)
            {
                logger.Warn($"Is User Null~~~");
                return;
            }

            var intlMsg = msg as InternalMsg.IntlMsg_TokenBalancesOf_NonFungible;

            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<LBD.Respones_GetNonFungibleTokenTypeBalances>(intlMsg.result);

                if (null == nftTypeBalances)
                    nftTypeBalances = new List<LBD.NonFungibleTokenTypeBalancesData>();

                if (resData.responseData.Count > 0 )
                    nftTypeBalances.AddRange(resData.responseData);

                if (resData.responseData.Count >= 50)
                {
                    LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenBalancesOf_NonFungible()
                    {
                        uid = user.UID,
                        guid = user.Id.ToString(),
                        toAddr = string.Empty,
                        toUserId = user.lineUID,

                        limit = 50,
                        orderBy = string.Empty,
                        page = intlMsg.page + 1
                    });
                    return;
                }

                TokenTypeBalancesRequest = nftTypeBalances.Count;

                foreach (var data in nftTypeBalances)
                {
                    LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenTypeBalancesOf_NonFungible()
                    {
                        uid = user.UID,
                        guid = user.Id.ToString(),
                        toAddr = string.Empty,
                        toUserId = user.lineUID,
                        tokenType = data.tokenType,

                        limit = 50,
                        orderBy = string.Empty,
                        page = 1
                    });
                }

                nftTypeBalances.Clear();
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public void IntlMsg_TokenTypeBalancesOf_NonFungible(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_TokenTypeBalancesOf_NonFungible;

            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<LBD.Respones_GetNonFungibleTokenIndexBalances>(intlMsg.result);

                if (null != user)
                {
                    if (null == nftIndexBalances)
                        nftIndexBalances = new Dictionary<string, List<LBD.NonFungibleTokenIndexBalancesData>>();

                    if (resData.responseData.Count > 0)
                    {
                        if (nftIndexBalances.ContainsKey(intlMsg.tokenType))
                        {
                            nftIndexBalances[intlMsg.tokenType].AddRange(resData.responseData);
                        }
                        else
                        {
                            nftIndexBalances.Add(intlMsg.tokenType, resData.responseData);
                        }
                    }
                    else
                    {
                        logger.Warn($"TokenIndex Not Found UserId={user.lineUID} TokenType={intlMsg.tokenType}");
                    }

                    if (resData.responseData.Count >= 50)
                    {
                        LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenTypeBalancesOf_NonFungible()
                        {
                            uid = user.UID,
                            guid = user.Id.ToString(),
                            toAddr = string.Empty,
                            toUserId = user.lineUID,
                            tokenType = intlMsg.tokenType,

                            limit = 50,
                            orderBy = string.Empty,
                            page = intlMsg.page + 1
                        });
                        return;
                    }

                    if (nftIndexBalances.ContainsKey(intlMsg.tokenType))
                    {
                        user.SetNonFungibleTokens(intlMsg.tokenType, nftIndexBalances[intlMsg.tokenType]);
                    }
                    else
                    {
                        logger.Warn($"TokenType Not Found UserId={user.lineUID} TokenType={intlMsg.tokenType}");
                    }

                    TokenTypeBalancesReceive++;

                    if (TokenTypeBalancesRequest == TokenTypeBalancesReceive)
                    {
                        TokenTypeBalancesRequest = 0;
                        TokenTypeBalancesReceive = 0;

                        if (user.stateType == UserStateType.InitGetNonFungibleTokenType)
                        {
                            user.stateType = UserStateType.InitGetDeck;
                            user.DBSelectDeck();
                        }
                        else
                            logger.Debug($"User StatType = {user.stateType.ToString()}");
                    }
                }
                else
                {
                    logger.Warn($"Is User Null~~~");
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public void IntlMsg_GetComposableToken(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_GetComposableToken;

            if (null != user)
            {
                var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

                if (resDataHeader.statusCode == 1000)
                {
                    if (intlMsg.type == TcpMsg.GetComposableType.GetTokenChildren)
                    {
                        if (user.stateType == UserStateType.InitGetDeck)
                        {
                            var resData = JsonConvert.DeserializeObject<LBD.Respones_GetComosableListInfo>(intlMsg.result);

                            user.SetDeckChildren(intlMsg.tokenType, intlMsg.tokenIndex, resData.responseData);
                        }
                    }
                    else if (intlMsg.type == TcpMsg.GetComposableType.GetTokenParent)
                    {
                        if (user.stateType == UserStateType.DeckCardAdd)
                        {
                            var ackData = new TcpMsg.AckDeckChildrenCardAdd()
                            {
                                errCode = TcpMsg.Error.AlreadySetDeckCard,
                            };
                            user.Send(new Packet(ackData));
                        }
                    }
                }
                else if (resDataHeader.statusCode == 4047)
                {
                    if (user.stateType == UserStateType.DeckCardAdd)
                    {
                        // Parent token not exist
                        if (intlMsg.type == TcpMsg.GetComposableType.GetTokenParent)
                        {
                            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_AddTokenParent()
                            {
                                uid = user.UID,
                                guid = user.Id.ToString(),
                                holderAddr = string.Empty,
                                holderUserId = user.lineUID,
                                pTokenType = LBD.LBDApiManager.TokenIdToTokenType(intlMsg.value),
                                pTokenIdx = LBD.LBDApiManager.TokenIdToTokenIndex(intlMsg.value),
                                cTokenType = intlMsg.tokenType,
                                cTokenIdx = intlMsg.tokenIndex
                            });
                            return;
                        }
                    }

                    logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
                }
                else
                {
                    logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
                }
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void IntlMsg_TransactionError(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_TransactionError;

            if (null != user)
            {
                TcpMsg.MessageResponse ackData;
                switch (intlMsg.errorCode)
                {
                    case InternalMsg.Error.AddTokenParent:
                        user.stateType = UserStateType.None;
                        ackData = new TcpMsg.AckDeckChildrenCardAdd()
                        {
                            errCode = TcpMsg.Error.DeckAddError,
                            statusCode = intlMsg.statusCode,
                            statusMessage = intlMsg.statusMessage
                        };
                        break;
                    case InternalMsg.Error.RemoveTokenParent:
                        user.stateType = UserStateType.None;
                        ackData = new TcpMsg.AckDeckChildrenCardRemove()
                        {
                            errCode = TcpMsg.Error.DeckRemoveError,
                            statusCode = intlMsg.statusCode,
                            statusMessage = intlMsg.statusMessage
                        };
                        break;
                    default:
                        logger.Warn($"Not Found ErrorCode={intlMsg.errorCode}");
                        return;
                }

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void IntlMsg_TransactionHash(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_TransactionHash;

            var resDataHeader = JsonConvert.DeserializeObject<LBD.ResponesDataHeader>(intlMsg.result);

            if (resDataHeader.statusCode == 1000)
            {
                if (null == user)
                {
                    return;
                }

                var code = LBD.LBDApiManager.TxHashToCode(intlMsg.result);

                if (code == 0)
                {
                    var logs = LBD.LBDApiManager.TxHashToLogs(intlMsg.result);

                    // Used for nontFungible Mint and Transfer. Use both single and multi.
                    // If future Mint and Transfer are included in one log, you have to think again.
                    var nontFungible_tokenInfos = new List<LBD.TokenInfo>();

                    // Currently, only one type of eventType is used.
                    // If you use eventType because it is mixed, you have to think again.
                    var eventType = string.Empty;

                    var fromAddr = string.Empty;
                    var toAddr = string.Empty;

                    foreach (var log in logs)
                    {
                        foreach (var evnt in log.events)
                        {
                            eventType = evnt.type;
                            switch (evnt.type)
                            {
                                case "mint_ft":
                                    {
                                        var tokenInfos = new List<LBD.TokenInfo>();
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "to")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "amount")
                                            {
                                                var value = attr.value.Split(':');
                                                var tokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(true, value[1], Convert.ToInt32(value[0]));
                                                tokenInfos.Add(tokenInfo);
                                            }
                                        }
                                        user.SetFungibleTokenMint(tokenInfos);

                                        if (user.stateType == UserStateType.NewUserConnect)
                                        {
                                            tokenInfos.ForEach(x =>
                                            {
                                                user.Currency_SetPrice(x.tokenType, x.amount);
                                            });

                                            user.OperatorServiceTokenMint(1000);
                                        }
                                        else if (user.stateType == UserStateType.BuyFungible)
                                        {
                                            var currency = tokenInfos.ToDictionary(x => x.tokenType, x => (Int64)x.amount);
                                            user.OnMintBuyShopSlot(currency);
                                        }
                                    }
                                    break;
                                case "mint_nft":
                                    {
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "to")
                                            {
                                                toAddr = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "name")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "token_id")
                                            {
                                                var tokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                                nontFungible_tokenInfos.Add(tokenInfo);
                                            }
                                        }
                                    }
                                    break;
                                case "burn_ft":
                                case "burn_ft_from":
                                    {
                                        var tokenInfos = new List<LBD.TokenInfo>();
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "amount")
                                            {
                                                var value = attr.value.Split(':');
                                                var tokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(true, value[1], Convert.ToInt32(value[0]));
                                                tokenInfos.Add(tokenInfo);
                                            }
                                        }
                                        //user.TokenBurn(ackData.fungible, tokenInfos);
                                    }
                                    break;
                                case "burn_nft":
                                case "burn_nft_from":
                                    {
                                        var tokenInfos = new List<LBD.TokenInfo>();
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "token_id")
                                            {
                                                var tokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                                tokenInfos.Add(tokenInfo);
                                            }
                                        }
                                        //user.TokenBurn(ackData.fungible, tokenInfos);
                                    }
                                    break;
                                case "transfer_ft":
                                case "transfer_ft_from":
                                    {
                                        var tokenInfos = new List<LBD.TokenInfo>();
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                fromAddr = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "to")
                                            {
                                                toAddr = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "amount")
                                            {
                                                var value = attr.value.Split(':');
                                                var tokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(true, value[1], Convert.ToInt32(value[0]));
                                                tokenInfos.Add(tokenInfo);
                                            }
                                        }

                                        FungibleTransferUserStateProcess(fromAddr, toAddr, tokenInfos);
                                    }
                                    break;
                                case "transfer_nft":
                                case "transfer_nft_from":
                                    {
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                fromAddr = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "to")
                                            {
                                                toAddr = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "token_id")
                                            {
                                                var tokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                                nontFungible_tokenInfos.Add(tokenInfo);
                                            }
                                        }
                                        //user.TokenTransfer(false, tokenInfos);
                                    }
                                    break;
                                case "attach":
                                case "attach_from":
                                    {
                                        LBD.TokenInfo ptokenInfo = null;
                                        LBD.TokenInfo ctokenInfo = null;
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "to_token_id")
                                            {
                                                ptokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                            }
                                            else if (attr.key == "token_id")
                                            {
                                                ctokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                            }
                                        }
                                        user.AddComposableTokenInfo(ptokenInfo, ctokenInfo);
                                    }
                                    break;
                                case "detach":
                                case "detach_from":
                                    {
                                        LBD.TokenInfo ptokenInfo = null;
                                        LBD.TokenInfo ctokenInfo = null;
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "from_token_id")
                                            {
                                                ptokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                            }
                                            else if (attr.key == "token_id")
                                            {
                                                ctokenInfo = LBD.LBDApiManager.Instance.TokenIdToTokenInfo(false, attr.value, 1);
                                            }
                                        }
                                        user.RemoveComposableTokenInfo(ptokenInfo, ctokenInfo);
                                    }
                                    break;
                                case "mint":
                                case "mint_from":
                                    {
                                        var contractId = string.Empty;
                                        Int32 amount = 0;
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "contract_id")
                                            {
                                                contractId = attr.value;
                                            }
                                            else if (attr.key == "amount")
                                            {
                                                amount = Convert.ToInt32(attr.value);
                                            }
                                            else if (attr.key == "from")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "to")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                        }
                                        //user.SetServiceTokenMint(contractId, amount);
                                    }
                                    break;
                                case "transfer":
                                case "transfer_from":
                                    {
                                        var from = string.Empty;
                                        var to = string.Empty;
                                        var contractId = string.Empty;
                                        Int32 amount = 0;
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "from")
                                            {
                                                from = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "to")
                                            {
                                                to = attr.value;
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "contract_id")
                                            {
                                                contractId = attr.value;
                                            }
                                            else if (attr.key == "amount")
                                            {
                                                amount = Convert.ToInt32(attr.value);
                                            }
                                        }

                                        if (from == Setting.ProgramSetting.Instance.lbdInfo.operatorAddr)
                                        {
                                            user.ServiceTokenTransfer(contractId, amount);
                                        }
                                        else if (from == user.walletAddr)
                                        {
                                            user.ServiceTokenTransfer(contractId, -amount);
                                        }
                                        var tokenInfos = new List<LBD.TokenInfo>
                                        {
                                            new LBD.TokenInfo()
                                            {
                                                tokenType = contractId,
                                                amount = amount,
                                            }
                                        };

                                        if (user.stateType == UserStateType.NewUserConnect)
                                        {
                                            user.stateType = UserStateType.CreateNickName;

                                            var ackData = new TcpMsg.AckVerifyAccessToken();
                                            ackData.authType = TcpMsg.AuthType.CreateNickname;

                                            user.Send(new Packet(ackData));

                                            logger.Debug($"UserUID={user.UID} UserStateType={user.stateType.ToString()}");
                                        }
                                        else if(user.stateType == UserStateType.BuyFungible)
                                        {
                                            var currency = tokenInfos.ToDictionary(x => x.tokenType, x => (Int64)x.amount);
                                            user.OnMintBuyShopSlot(currency);
                                        }
                                        else if (user.stateType == UserStateType.BuyShopSlot)
                                        {
                                            user.OnBurnShopSlot(tokenInfos);
                                        }
                                        else if (user.stateType == UserStateType.RefreshShopCardSlot)
                                        {
                                            user.OnBurnRefreshShopCardSlot(tokenInfos);
                                        }
                                    }
                                    break;
                                case "approve_collection":
                                    {
                                        var contractId = string.Empty;
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "proxy")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "approver")
                                            {
                                                logger.Info($"{attr.key} {attr.key}={attr.value}");
                                            }
                                            else if (attr.key == "contract_id")
                                            {
                                                contractId = attr.value;
                                            }
                                        }

                                        LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_GetProxy()
                                        {
                                            uid = user.UID,
                                            guid = user.Id.ToString(),
                                            lineuserid = user.lineUID
                                        });
                                    }
                                    break;
                            }
                        }
                    }

                    MultNonFungibleUserStateProcess(eventType, fromAddr, toAddr, nontFungible_tokenInfos);
                }
                else
                {
                    logger.Warn($"TransactionHash fail Code={code}");
                }
            }
        }

        void MultNonFungibleUserStateProcess(string eventType, string fromAddr, string toAddr, List<LBD.TokenInfo> tokenInfos)
        {
            if (eventType == "mint_nft")
            {
                user.NonFungibleToken_Add(tokenInfos);

                if (user.stateType == UserStateType.NewUserConnect)
                {
                    // In Consloe, the currency to be used in advance in the operating wallet must be minted.
                    user.OperatorFungibleMint(Resource.PriceType.Coin, 1000);
                }
                else if (user.stateType == UserStateType.BuyNonFungible)
                {
                    if (tokenInfos.Count > 0)
                        user.OnMintBuyShopSlot(tokenInfos);
                }
            }
            else if (eventType == "transfer_nft" || eventType == "transfer_nft_from")
            {
                if (user.stateType == UserStateType.BuyerGiveGoods)
                {
                    var buyerData = TradingManager.Instance.GetBuyerDataAndRemove(user.UID);
                    var goods = buyerData.goods;

                    TradingManager.Instance.InsertTradeHistory(goods, TcpMsg.TradeStatus.Soldout, buyerData.uuid, buyerData.nickName);

                    var fromUser = NetServer.WalletAddressToFindUser(fromAddr);
                    if (null != fromUser)
                    {
                        NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_BuyerGiveGoods()
                        {
                            uid = fromUser.UID,
                            guid = fromUser.Id.ToString(),
                            goodsSeq = goods.seq,
                            touuid = user.UID
                        });
                    }

                    user.NonFungibleToken_Add(tokenInfos);

                    var ackData = new TcpMsg.AckTradeBuyerGiveGoods()
                    {
                        goodsSeq = goods.seq,
                        fromuuid = goods.uuid,
                        touuid = user.UID,
                        cardInfo = User.TokenInfoToCardInfo(tokenInfos[0]),
                        currencyinfos = user.ToCurrencyInfoList()
                    };

                    user.Send(new Packet(ackData));
                }
            }
        }

        void FungibleTransferUserStateProcess(string fromAddr, string toAddr, List<LBD.TokenInfo> tokenInfos)
        {
            if (null != tokenInfos && tokenInfos.Count > 0)
            {
                switch (user.stateType)
                {
                    case UserStateType.NewUserConnect:
                        {
                            tokenInfos.ForEach(x =>
                            {
                                user.FungibleTokenTransfer(x.tokenType, x.amount);
                                user.Currency_SetPrice(x.tokenType, x.amount);
                            });

                            user.OperatorServiceTokenMint(1000);
                        }
                        break;
                    case UserStateType.RegisterTradeGoodsFees:
                        {
                            tokenInfos.ForEach(x =>
                            {
                                user.FungibleTokenTransfer(x.tokenType, -x.amount);
                                user.Currency_SetPrice(x.tokenType, -x.amount);
                            });

                            var tblTrade = TradingManager.Instance.RegisterTradeGoods(user.UID);
                            user.RegisterTradeGoodsSucess(tblTrade);
                        }
                        break;
                    case UserStateType.SellerPayments:
                        {
                            tokenInfos.ForEach(x =>
                            {
                                user.FungibleTokenTransfer(x.tokenType, -x.amount);
                                user.Currency_SetPrice(x.tokenType, -x.amount);
                            });

                            var toUser = NetServer.WalletAddressToFindUser(toAddr);
                            if (null != toUser)
                            {
                                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_SellerPayments()
                                {
                                    uid = toUser.UID,
                                    guid = toUser.Id.ToString(),
                                    fromUserUid = user.UID,
                                    tokenType = tokenInfos[0].tokenType,
                                    amount = tokenInfos[0].amount
                                });
                            }

                            var buyerData = TradingManager.Instance.GetBuyerData(user.UID);
                            var goods = buyerData.goods;

                            user.stateType = UserStateType.BuyerGiveGoods;
                            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_UserId_NonFungible_Transfer()
                            {
                                uid = user.UID,
                                guid = user.Id.ToString(),
                                fromUserId = goods.userid,
                                toAddr = string.Empty,
                                toUserId = user.lineUID,
                                tokenType = goods.tokentype,
                                tokenIndex = goods.tokenidx
                            });
                        }
                        break;
                    case UserStateType.BuyFungible:
                        {
                            tokenInfos.ForEach(x => user.FungibleTokenTransfer(x.tokenType, x.amount));

                            var currency = tokenInfos.ToDictionary(x => x.tokenType, x => (Int64)x.amount);
                            user.OnMintBuyShopSlot(currency);
                        }
                        break;
                    case UserStateType.BuyShopSlot:
                        {
                            tokenInfos.ForEach(x => user.FungibleTokenTransfer(x.tokenType, -x.amount));

                            user.OnBurnShopSlot(tokenInfos);
                        }
                        break;
                    case UserStateType.RefreshShopCardSlot:
                        {
                            tokenInfos.ForEach(x => user.FungibleTokenTransfer(x.tokenType, -x.amount));

                            user.OnBurnRefreshShopCardSlot(tokenInfos);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                logger.Warn($"transfer_ft userUid={user.UID} tokenInfo is null~~");
            }
        }
    }
}
