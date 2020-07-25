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
using System.Text.RegularExpressions;

namespace TCGGameService.LBD
{
    public partial class LBDApiManager
    {
        public List<TokenTypeInfo> nonFungibleTypes = new List<TokenTypeInfo>();
        public List<TokenTypeInfo> fungibleTypes = new List<TokenTypeInfo>();
        public List<ServiceTokenTypeInfo> serviceTypes = new List<ServiceTokenTypeInfo>();

        List<Msg.LBDMsg_TokenIssue> issues;

        public static string TokenIdToTokenType(string tokenId)
        {
            return tokenId.Substring(0, tokenId.Length - 8);
        }

        public static string TokenIdToTokenIndex(string tokenId)
        {
            return tokenId.Substring(tokenId.Length - 8);
        }

        public TokenTypeInfo GetFungibleTypeToMeta(string meta)
        {
            return fungibleTypes.Find(x => x.meta == meta);
        }

        public TokenTypeInfo GetFungibleTypeToTokenType(string tokenType)
        {
            return fungibleTypes.Find(x => x.tokenType == tokenType);
        }

        public TokenTypeInfo GetNonFungibleTypeToMeta(string meta)
        {
            return nonFungibleTypes.Find(x => x.meta == meta);
        }

        public TokenTypeInfo GetNonFungibleTypeToTokenType(string tokenType)
        {
            return nonFungibleTypes.Find(x => x.tokenType == tokenType);
        }

        public ServiceTokenTypeInfo GetServiceTokenType(string contractId)
        {
            return serviceTypes.Find(x => x.contractId == contractId);
        }

        void AddIssueList(bool fungible, string name, string meta)
        {
            if (null == issues)
                issues = new List<Msg.LBDMsg_TokenIssue>();

            if (!issues.Exists(x => x.meta == meta))
            {
                var lbdMsg = new Msg.LBDMsg_TokenIssue()
                {
                    uid = 0,
                    guid ="issue",
                    fungible = fungible,
                    name = name,
                    meta = meta
                };

                issues.Add(lbdMsg);
            }
            else
            {
                logger.Warn($"Issue Duplicate fungible={fungible} name={name} meta={meta}");
            }
        }

        void IssueItemToken()
        {
            if (null == issues || issues.Count <= 0)
            {
                logger.Info("IssueItemToken null or Entity zero");
                logger.Info($"NonFungibleTypeCount={nonFungibleTypes.Count} FungibleCount={fungibleTypes.Count} ServiceTokenTypeCount={serviceTypes.Count}");
                return;
            }

            foreach (var lbdMsg in issues)
            {
                AddLBDCmd(lbdMsg);
            }
        }

        void SetItemTokenType(List<TokenTypeInfo> tokenTypes, List<TokenTypeData> tokenTypeInfos)
        {
            if (tokenTypeInfos.Count > 0)
            {
                foreach(var tokentype in tokenTypeInfos)
                {
                    if (null != tokenTypes.Find(x => x.meta == tokentype.meta))
                        continue;

                    tokenTypes.Add(new TokenTypeInfo()
                    {
                        tokenType = tokentype.tokenType,
                        name = tokentype.name,
                        meta = tokentype.meta
                    });
                }
            }
        }

        void SetItemTokenType(List<TokenTypeInfo> tokenTypes, TokenTypeInfo tokenTypeInfo)
        {
            if (null == tokenTypes.Find(x => x.meta == tokenTypeInfo.meta))
            {
                tokenTypes.Add(tokenTypeInfo);
            }
            else
            {
                logger.Warn($"Duplication TokenType TokenType={tokenTypeInfo.tokenType} meta={tokenTypeInfo.meta}");
            }
            
        }

        Int32[] FungibleExcept()
        {
            var first = TCGGameSrv.ResourceDataLoader.Data_Price_List.Select(x => (int)x.priceType);
            var second = fungibleTypes.Select(x => Convert.ToInt32(x.meta));

            return first.Except(second).ToArray();
        }

        Int32[] NonFungibleExcept()
        {
            var first = TCGGameSrv.ResourceDataLoader.Data_Card_List.Select(x => x.ID).ToArray();
            var second = nonFungibleTypes.Select(x => Convert.ToInt32(x.meta)).ToArray();

            return first.Except(second).ToArray();
        }

        void GetLBDItemTokenType_Fungible(List<TokenTypeData> responseData, Int32 page)
        {
            if (responseData.Count <= 0)
            {
                foreach (var resource in TCGGameSrv.ResourceDataLoader.Data_Price_List)
                {
                    if (resource.fungibleType != Resource.FungibleType.Fungible)
                        continue;

                    var tokenName = TCGGameSrv.ResourceDataLoader.Data_String_Dictionary_ID[resource.tokenName].eng;
                    tokenName = Regex.Replace(tokenName, "[^0-9A-Za-z]+", "");
                    AddIssueList(true, tokenName, ((Int32)resource.priceType).ToString());
                }
            }
            else
            {
                SetItemTokenType(fungibleTypes, responseData);

                if (responseData.Count >= 50)
                {
                    AddLBDCmd(new Msg.LBDMsg_TokenTypeInfo_Fungible()
                    {
                        guid = "tokentypeinfo",
                        limit = 50,
                        orderBy = string.Empty,
                        page = page + 1
                    });

                    return;
                }

                var except = FungibleExcept();

                foreach (var meta in except)
                {
                    var resource = TCGGameSrv.ResourceDataLoader.Data_Price_Dictionary_priceType[(Resource.PriceType)meta];
                    if (resource.fungibleType != Resource.FungibleType.Fungible)
                        continue;

                    var tokenName = TCGGameSrv.ResourceDataLoader.Data_String_Dictionary_ID[resource.tokenName].eng;
                    tokenName = Regex.Replace(tokenName, "[^0-9A-Za-z]+", "");
                    AddIssueList(true, tokenName, ((Int32)resource.priceType).ToString());
                }
            }

            AddLBDCmd(new Msg.LBDMsg_TokenTypeInfo_ServiceToken());
        }

        void GetLBDItemTokenType_NonFungible(List<TokenTypeData> responseData, Int32 page)
        {
            if (responseData.Count <= 0)
            {
                foreach (var resource in TCGGameSrv.ResourceDataLoader.Data_Card_List)
                {
                    var tokenName = $"Card{TCGGameSrv.ResourceDataLoader.Data_String_Dictionary_ID[resource.cardNameString].eng}";
                    tokenName = Regex.Replace(tokenName, "[^0-9A-Za-z]+", "");
                    AddIssueList(false, tokenName, resource.ID.ToString());
                }
            }
            else
            {
                SetItemTokenType(nonFungibleTypes, responseData);

                if (responseData.Count >= 50)
                {
                    AddLBDCmd(new Msg.LBDMsg_TokenTypeInfo_NonFungible()
                    {
                        guid = "tokentypeinfo",
                        limit = 50,
                        orderBy = string.Empty,
                        page = page + 1
                    });

                    return;
                }

                var except = NonFungibleExcept();

                foreach (var meta in except)
                {
                    var resource = TCGGameSrv.ResourceDataLoader.Data_Card_Dictionary_ID[meta];
                    var tokenName = $"Card{TCGGameSrv.ResourceDataLoader.Data_String_Dictionary_ID[resource.cardNameString].eng}";
                    tokenName = Regex.Replace(tokenName, "[^0-9A-Za-z]+", "");
                    AddIssueList(false, tokenName, resource.ID.ToString());
                }
            }

            IssueItemToken();
        }

        void GetLBDItemTokenType(bool fungible, string result, Int32 page)
        {
            var resDataHeader = JsonConvert.DeserializeObject<ResponesDataHeader>(result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<Respones_GetTokenTypeInfos>(result);

                if (fungible)
                {
                    GetLBDItemTokenType_Fungible(resData.responseData, page);
                }
                else
                {
                    GetLBDItemTokenType_NonFungible(resData.responseData, page);
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        void GetLBDServiceTokenType(string result)
        {
            var resDataHeader = JsonConvert.DeserializeObject<ResponesDataHeader>(result);

            if (resDataHeader.statusCode == 1000)
            {
                var resData = JsonConvert.DeserializeObject<Respones_GetServiceTokens>(result);

                if (resData.responseData.Count > 0)
                {
                    foreach (var serviceTokenType in resData.responseData)
                    {
                        if (null != serviceTypes.Find(x => x.contractId == serviceTokenType.contractId))
                            continue;

                        serviceTypes.Add(new ServiceTokenTypeInfo()
                        {
                            contractId = serviceTokenType.contractId,
                            name = serviceTokenType.name,
                            symbol = serviceTokenType.symbol,
                            imgUri = serviceTokenType.imgUri,
                            meta = serviceTokenType.meta,
                            serviceId = serviceTokenType.serviceId
                        });
                    }

                    AddLBDCmd(new Msg.LBDMsg_TokenTypeInfo_NonFungible()
                    {
                        guid = "tokentypeinfo",
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

        public static (string, string) GetTxMsg(string result, Int32 msgIndex)
        {
            var jo = JObject.Parse(result);
            var msgTxt = jo["responseData"]["tx"]["value"]["msg"][msgIndex].ToString();
            Console.WriteLine($"{JValue.Parse(msgTxt).ToString(Formatting.Indented)}");

            var typeInfo = JsonConvert.DeserializeObject<TxMsgType>(msgTxt);

            return (typeInfo.type, msgTxt);
        }

        public static Int32 TxHashToCode(string result)
        {
            var jo = JObject.Parse(result);
            var code = jo["responseData"]["code"].Value<Int32>();

            return code;
        }

        public static List<TxLogData> TxHashToLogs(string result)
        {
            var jo = JObject.Parse(result);
            var logtxt = jo["responseData"]["logs"].ToString();

            Console.WriteLine($"{JValue.Parse(logtxt).ToString(Formatting.Indented)}");

            return JsonConvert.DeserializeObject<List<TxLogData>>(logtxt);
        }

        void GetLBDItemTokenIssue(string result)
        {
            var resDataHeader = JsonConvert.DeserializeObject<ResponesDataHeader>(result);

            if (resDataHeader.statusCode == 1000)
            {
                var code = TxHashToCode(result);
                var logs = TxHashToLogs(result);
                foreach (var log in logs)
                {
                    if (code == 0)
                    {
                        foreach (var evnt in log.events)
                        {
                            switch (evnt.type)
                            {
                                case "issue_ft":
                                    {
                                        var tokenTypeInfo = new TokenTypeInfo();

                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "token_id")
                                            {
                                                tokenTypeInfo.tokenType = attr.value.Substring(0, attr.value.Length - 8);
                                            }
                                            else if (attr.key == "name")
                                            {
                                                tokenTypeInfo.name = attr.value;
                                            }
                                        }

                                        var msgInfo = GetTxMsg(result, log.msgIndex);
                                        if (msgInfo.Item1 == "collection/MsgIssueFT")
                                        {
                                            var msg = JsonConvert.DeserializeObject<IssueFTMsg>(msgInfo.Item2);

                                            if (null != msg)
                                            {
                                                tokenTypeInfo.meta = msg.value.meta;
                                                SetItemTokenType(fungibleTypes, tokenTypeInfo);
                                            }
                                        }
                                        else
                                        {
                                            logger.Warn($"{evnt.type} TxMsg Not equal Type={msgInfo.Item1}");
                                        }
                                    }
                                    break;
                                case "issue_nft":
                                    {
                                        var tokenTypeInfo = new TokenTypeInfo();
                                        foreach (var attr in evnt.attributes)
                                        {
                                            if (attr.key == "token_type")
                                            {
                                                tokenTypeInfo.tokenType = attr.value;
                                            }
                                        }

                                        var msgInfo = GetTxMsg(result, log.msgIndex);
                                        if (msgInfo.Item1 == "collection/MsgIssueNFT")
                                        {
                                            var msg = JsonConvert.DeserializeObject<IssueNFTMsg>(msgInfo.Item2);

                                            if (null != msg)
                                            {
                                                tokenTypeInfo.name = msg.value.name;
                                                tokenTypeInfo.meta = msg.value.meta;

                                                SetItemTokenType(nonFungibleTypes, tokenTypeInfo);
                                            }
                                        }
                                        else
                                        {
                                            logger.Warn($"{evnt.type} TxMsg Not equal Type={msgInfo.Item1}");
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        logger.Warn($"Log success fail log");
                        if (!string.IsNullOrEmpty(log.log))
                            logger.Warn($"Code={code} {JValue.Parse(log.log).ToString(Formatting.Indented)}");
                        else
                            logger.Warn($"Code={code} log is Empty");
                    }
                }
            }
            else
            {
                logger.Warn($"Error statusCode={resDataHeader.statusCode} statusMessage={resDataHeader.statusMessage}");
            }
        }

        public TokenTypeInfo NonFungibleTokenMetaToTokenTypeInfo(string meta)
        {
            return nonFungibleTypes.Find(x => x.meta == meta);
        }

        public TokenInfo TokenIdToTokenInfo(bool fungible, string tokenId, Int32 amount)
        {
            var tokenInfo = new TokenInfo();

            tokenInfo.tokenType = tokenId.Substring(0, tokenId.Length - 8);
            tokenInfo.tokenIdx = tokenId.Substring(tokenId.Length - 8);
            tokenInfo.amount = amount;

            TokenTypeInfo tokenTypeInfo = null;
            if (fungible)
            {
                tokenTypeInfo = fungibleTypes.Find(x => x.tokenType == tokenInfo.tokenType);
            }
            else
            {
                tokenTypeInfo = nonFungibleTypes.Find(x => x.tokenType == tokenInfo.tokenType);
            }

            if (null != tokenTypeInfo)
            {
                tokenInfo.name = tokenTypeInfo.name;
                tokenInfo.meta = tokenTypeInfo.meta;
            }

            return tokenInfo;
        }
    }
}
