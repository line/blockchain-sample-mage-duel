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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TCGGameService.LBD
{
    public partial class LBDApiManager
    {
        public string LBDMsg_GetUser(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_GetUser;

            var uriData = GetUriData(ApiUriType.GetUser);
            uriData.userId = lbdMsg.userid;

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"GetUser fail~~ UserId={lbdMsg.userid}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_GetUser()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    result = result
                });

                logger.Debug("GetUser");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TokenIssue(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenIssue;

            UriData uriData;
            if (lbdMsg.fungible)
                uriData = GetUriData(ApiUriType.IssueFungible);
            else
                uriData = GetUriData(ApiUriType.IssueNonFungible);

            var issueTokenBody = new IssueTokenBbody()
            {
                ownerAddress = lbdInfo.operatorAddr,
                ownerSecret = lbdInfo.secretKey,
                name = lbdMsg.name,
                meta = lbdMsg.meta
            };

            var body = JsonConvert.SerializeObject(issueTokenBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenIssue fail Fungible={lbdMsg.fungible.ToString()} Name={lbdMsg.name} Meta={lbdMsg.meta}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("TokenIssue");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"TokenIssue StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        string PageQuary(Int32 limit, string orderBy, Int32 page)
        {
            var query = limit > 0 ? $"limit={limit}" : string.Empty;
            query += !string.IsNullOrEmpty(orderBy) ? $"&orderBy={orderBy}" : string.Empty;
            query += page > 0 ? $"&page={page}" : string.Empty;

            return query;
        }

        public string LBDMsg_TokenTypeInfo_Fungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenTypeInfo_Fungible;

            var uriData = GetUriData(ApiUriType.TokenTypeInfo_Fungible);
            uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenTypeInfo_Fungible fail~~");
            else
            {
                GetLBDItemTokenType(true, result, lbdMsg.page);
                logger.Debug("TokenTypeInfo_Fungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TokenTypeInfo_NonFungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenTypeInfo_NonFungible;

            var uriData = GetUriData(ApiUriType.TokenTypeInfo_NonFungible);
            uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenTypeInfo_NonFungible fail~~");
            else
            {
                GetLBDItemTokenType(false, result, lbdMsg.page);
                logger.Debug("TokenTypeInfo_NonFungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_MultiMintNonFungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_MultiMintNonFungible;

            var uriData = GetUriData(ApiUriType.MultiMintNonFungible);

            MultiMintNonFungibleBody multiMintBody = null;
            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                multiMintBody = new ToWallet_MultiMintNonFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toAddress = lbdMsg.toAddr,
                    mintList = lbdMsg.mints
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                multiMintBody = new ToUserId_MultiMintNonFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toUserId = lbdMsg.toUserId,
                    mintList = lbdMsg.mints
                };
            }

            if (null == multiMintBody)
            {
                logger.Warn($"MultiMintNonFungible No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(multiMintBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"MultiMintNonFungible fail ToAddr={lbdMsg.toAddr} ToUserId={lbdMsg.toUserId}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("MultiMintNonFungible");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"MultiMintNonFungible StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string MintNonFungible(string toAddr, string toUserId, string tokenType, string name, string meta)
        {
            var uriData = GetUriData(ApiUriType.MintNonFungible);
            uriData.tokenType = tokenType;

            MintNonFungibleBody mintBody = null;
            if (!string.IsNullOrEmpty(toAddr))
            {
                mintBody = new ToWallet_MintNonFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    name = name,
                    meta = meta,
                    toAddress = toAddr,
                };
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                mintBody = new ToUserId_MintNonFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    name = name,
                    meta = meta,
                    toUserId = toAddr,
                };
            }

            if (null == mintBody)
            {
                logger.Warn($"MintNonFungible No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(mintBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"MintNonFungible fail ToAddr={toAddr}");
            else
            {
                logger.Debug("MintNonFungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_MintFungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_MintFungible;

            var uriData = GetUriData(ApiUriType.MintFungible);
            uriData.tokenType = lbdMsg.tokenType;

            MintFungibleBody mintBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                mintBody = new ToWallet_MintFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toAddress = lbdMsg.toAddr,
                    amount = lbdMsg.amount.ToString(),
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                mintBody = new ToUserId_MintFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toUserId = lbdMsg.toUserId,
                    amount = lbdMsg.amount.ToString(),
                };
            }

            if (null == mintBody)
            {
                logger.Warn($"MintFungible No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(mintBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"MintFungible fail ToAddr={lbdMsg.toAddr}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("MintFungible");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"MintFungible StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            logger.Debug($"uriData={JsonConvert.SerializeObject(uriData)}");
            logger.Debug($"body={body}");

            return result;
        }

        public string BurnNonFungible(string fromAddr, string fromUserId, string tokenType, string tokenIdx)
        {
            var uriData = GetUriData(ApiUriType.BurnNonFungible);
            uriData.tokenType = tokenType;
            uriData.tokenIdx = tokenIdx;

            BurnNonFungibleBody burnBody = null;

            if (!string.IsNullOrEmpty(fromAddr))
            {
                burnBody = new FromWallet_BurnNonFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    fromAddress = fromAddr
                };
            }
            else if (!string.IsNullOrEmpty(fromUserId))
            {
                burnBody = new FromUserId_BurnNonFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    fromUserId = fromUserId
                };
            }

            if (null == burnBody)
            {
                logger.Warn($"BurnNonFungible No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(burnBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"BurnNonFungible fail FromAddr={fromAddr}");
            else
            {
                logger.Debug("BurnNonFungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string BurnFungible(string fromAddr, string fromUserId, string tokenType, Int32 amount, Int64 uid, string guid)
        {
            var uriData = GetUriData(ApiUriType.BurnFungible);
            uriData.tokenType = tokenType;

            BurnFungibleBody burnBody = null;

            if (!string.IsNullOrEmpty(fromAddr))
            {
                burnBody = new FromWallet_BurnFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    fromAddress = fromAddr,
                    amount = amount.ToString()
                };
            }
            else if (!string.IsNullOrEmpty(fromUserId))
            {
                burnBody = new FromUserId_BurnFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    fromUserId = fromUserId,
                    amount = amount.ToString()
                };
            }

            if (null == burnBody)
            {
                logger.Warn($"BurnFungible No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(burnBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"BurnFungible fail FromAddr={fromAddr}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = uid,
                        guid = guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("BurnFungible");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"BurnFungible StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string Wallet_NonFungible_Transfer(string fromAddr, string fromSecret, string toAddr, string toUserId, string tokenType, string tokenIndex)
        {
            var uriData = GetUriData(ApiUriType.TransferNonFungible);
            uriData.walletAddr = fromAddr;
            uriData.tokenType = tokenType;
            uriData.tokenIdx = tokenIndex;

            Wallet_NonFungibleTransferBody transferBody = null;
            if (!string.IsNullOrEmpty(toAddr))
            {
                transferBody = new Wallet_ToWallet_NonFungibleTransferBody()
                {
                    walletSecret = fromSecret,
                    toAddress = toAddr,
                };
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                transferBody = new Wallet_ToUserId_NonFungibleTransferBody()
                {
                    walletSecret = fromSecret,
                    toUserId = toUserId,
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"NonFungible_Transfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"NonFungible_Transfer fail ToAddr={toAddr}");
            else
            {
                logger.Debug("NonFungible_Transfer");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_UserId_NonFungible_Transfer(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_UserId_NonFungible_Transfer;

            var uriData = GetUriData(ApiUriType.TransferNonFungible);
            uriData.userId = lbdMsg.fromUserId;
            uriData.tokenType = lbdMsg.tokenType;
            uriData.tokenIdx = lbdMsg.tokenIndex;

            UserId_NonFungibleTransferBody transferBody = null;
            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                transferBody = new UserId_ToWallet_NonFungibleTransferBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toAddress = lbdMsg.toAddr
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                transferBody = new UserId_ToUserId_NonFungibleTransferBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toUserId = lbdMsg.toUserId,
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"NonFungible_Transfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"NonFungible_Transfer fail ToAddr={lbdMsg.toAddr} ToUseruid={lbdMsg.toUserId}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("NonFungible_Transfer");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"NonFungible_Transfer StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_Wallet_Fungible_Transfer(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_Wallet_Fungible_Transfer;

            var uriData = GetUriData(ApiUriType.TransferFungible);
            uriData.walletAddr = lbdMsg.fromAddr;
            uriData.tokenType = lbdMsg.tokenType;

            Wallet_FungibleTransferBody transferBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                transferBody = new Wallet_ToWallet_FungibleTransferBody()
                {
                    walletSecret = lbdMsg.fromSecret,
                    toAddress = lbdMsg.toAddr,
                    amount = lbdMsg.amount.ToString(),
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                transferBody = new Wallet_ToUserId_FungibleTransferBody()
                {
                    walletSecret = lbdMsg.fromSecret,
                    toUserId = lbdMsg.toUserId,
                    amount = lbdMsg.amount.ToString(),
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"Fungible_Transfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"Fungible_Transfer fail ToAddr={lbdMsg.toAddr} ToUseruid={lbdMsg.toUserId}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("Fungible_Transfer");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"Fungible_Transfer StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_UserId_Fungible_Transfer(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_UserId_Fungible_Transfer;

            var uriData = GetUriData(ApiUriType.TransferFungible);
            uriData.userId = lbdMsg.fromUserId;
            uriData.tokenType = lbdMsg.tokenType;

            UserId_FungibleTransferBody transferBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                transferBody = new UserId_ToWallet_FungibleTransferBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toAddress = lbdMsg.toAddr,
                    amount = lbdMsg.amount.ToString()
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                transferBody = new UserId_ToUserId_FungibleTransferBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toUserId = lbdMsg.toUserId,
                    amount = lbdMsg.amount.ToString()
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"Fungible_Transfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"Fungible_Transfer fail From={lbdMsg.fromUserId} ToAddr={lbdMsg.toAddr} ToUserId={lbdMsg.toUserId}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("Fungible_Transfer");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"Fungible_Transfer StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string Wallet_Batch_Transfer(string fromAddr, string fromSecret, string toAddr, string toUserId, List<TokenIdInfo> tokenIds)
        {
            var uriData = GetUriData(ApiUriType.BatchTransfer);
            uriData.walletAddr = fromAddr;

            Wallet_BatchTransferBody transferBody = null;

            if (!string.IsNullOrEmpty(toAddr))
            {
                transferBody = new Wallet_ToWallet_BatchTransferBody()
                {
                    walletSecret = fromSecret,
                    toAddress = toAddr,
                    transferList = tokenIds
                };
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                transferBody = new Wallet_ToUserId_BatchTransferBody()
                {
                    walletSecret = fromSecret,
                    toUserId = toUserId,
                    transferList = tokenIds
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"Batch_Transfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (null == result)
                logger.Warn($"Batch_Transfer fail ToAddr={toAddr}");
            else
            {
                logger.Debug("Batch_Transfer");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string UserId_Batch_Transfer(string fromUserId, string toAddr, string toUserId, List<TokenIdInfo> tokenIds)
        {
            var uriData = GetUriData(ApiUriType.BatchTransfer);
            uriData.userId = fromUserId;

            UserId_BatchTransferBody transferBody = null;

            if (!string.IsNullOrEmpty(toAddr))
            {
                transferBody = new UserId_ToWallet_BatchTransferBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toAddress = toAddr,
                    transferList = tokenIds
                };
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                transferBody = new UserId_ToUserId_BatchTransferBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toUserId = toUserId,
                    transferList = tokenIds
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"Batch_Transfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (null == result)
                logger.Warn($"Batch_Transfer fail ToAddr={toAddr}");
            else
            {
                logger.Debug("Batch_Transfer");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TokenBalancesOf_NonFungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenBalancesOf_NonFungible;

            var uriData = GetUriData(ApiUriType.TokenBalancesOf_NonFungible);
            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                uriData.walletAddr = lbdMsg.toAddr;
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                uriData.userId = lbdMsg.toUserId;
            }
            else
            {
                logger.Warn($"TokenBalancesOf_NonFungible No destination address");
                return null;
            }

            uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenBalancesOf_NonFungible fail owner={lbdMsg.toAddr} or userId={lbdMsg.toUserId}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TokenBalancesOf_NonFungible()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    result = result,
                    page = lbdMsg.page
                });

                logger.Debug("TokenBalancesOf_NonFungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TokenBalancesOf_Fungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenBalancesOf_Fungible;

            var uriData = GetUriData(ApiUriType.TokenBalancesOf_Fungible);

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                uriData.walletAddr = lbdMsg.toAddr;
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                uriData.userId = lbdMsg.toUserId;
            }
            else
            {
                logger.Warn($"TokenBalancesOf_Fungible No destination address");
                return null;
            }

            uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenBalancesOf_Fungible fail owner={lbdMsg.toAddr} or userId={lbdMsg.toUserId}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TokenBalancesOf_Fungible()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    result = result,
                    page = lbdMsg.page
                });

                logger.Debug("TokenBalancesOf_Fungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TokenTypeBalancesOf_NonFungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenTypeBalancesOf_NonFungible;

            var uriData = GetUriData(ApiUriType.TokenTypeBalancesOf_NonFungible);
            uriData.tokenType = lbdMsg.tokenType;

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                uriData.walletAddr = lbdMsg.toAddr;
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                uriData.userId = lbdMsg.toUserId;
            }
            else
            {
                logger.Warn($"TokenTypeBalancesOf_NonFungible No destination address");
                return null;
            }

            uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenTypeBalancesOf_NonFungible fail owner={lbdMsg.toAddr} or userId={lbdMsg.toUserId}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TokenTypeBalancesOf_NonFungible()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    tokenType = lbdMsg.tokenType,
                    result = result,
                    page = lbdMsg.page
                });

                logger.Debug("TokenTypeBalancesOf_NonFungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string TokenTypeBalancesOf_Fungible(string toAddr, string toUserId, string tokenType)
        {
            var uriData = GetUriData(ApiUriType.TokenTypeBalancesOf_Fungible);
            uriData.tokenType = tokenType;

            if (!string.IsNullOrEmpty(toAddr))
            {
                uriData.walletAddr = toAddr;
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                uriData.userId = toUserId;
            }
            else
            {
                logger.Warn($"TokenTypeBalancesOf_Fungible No destination address");
                return null;
            }

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenTypeBalancesOf_Fungible fail owner={toAddr} or userId={toUserId}");
            else
            {
                logger.Debug("TokenTypeBalancesOf_Fungible");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_GetComposableToken(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_GetComposableToken;

            UriData uriData = null;
            switch (lbdMsg.type)
            {
                case TcpMsg.GetComposableType.GetTokenChildren:
                    uriData = GetUriData(ApiUriType.GetTokenChildren);
                    uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);
                    break;
                case TcpMsg.GetComposableType.GetTokenParent:
                    uriData = GetUriData(ApiUriType.GetTokenParent);
                    break;
                case TcpMsg.GetComposableType.GetTokenRoot:
                    uriData = GetUriData(ApiUriType.GetTokenRoot);
                    break;
                default:
                    logger.Warn($"GetComposableToken ComposableType NotFound {lbdMsg.type.ToString()}");
                    break;
            }

            if (null == uriData)
            {
                logger.Warn($"GetComposableToken UriData is null");
                return null;
            }

            uriData.tokenType = lbdMsg.tokenType;
            uriData.tokenIdx = lbdMsg.tokenIdx;

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"GetComposableToken {lbdMsg.type.ToString()} fail tokenType={lbdMsg.tokenType} tokenIdx={lbdMsg.tokenIdx}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_GetComposableToken()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    type = lbdMsg.type,
                    tokenType = lbdMsg.tokenType,
                    tokenIndex = lbdMsg.tokenIdx,
                    result = result,
                    value = lbdMsg.value
                });

                logger.Debug("GetComposableToken");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_AddTokenParent(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_AddTokenParent;

            var uriData = GetUriData(ApiUriType.AddTokenParent);
            uriData.tokenType = lbdMsg.cTokenType;
            uriData.tokenIdx = lbdMsg.cTokenIdx;

            AddTokenParentBody transferBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.holderAddr))
            {
                transferBody = new ToWallet_AddTokenParentBody()
                {
                    parentTokenId = $"{lbdMsg.pTokenType}{lbdMsg.pTokenIdx}",
                    serviceWalletAddress = lbdInfo.operatorAddr,
                    serviceWalletSecret = lbdInfo.secretKey,
                    tokenHolderAddress = lbdMsg.holderAddr
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.holderUserId))
            {
                transferBody = new ToUserId_AddTokenParentBody()
                {
                    parentTokenId = $"{lbdMsg.pTokenType}{lbdMsg.pTokenIdx}",
                    serviceWalletAddress = lbdInfo.operatorAddr,
                    serviceWalletSecret = lbdInfo.secretKey,
                    tokenHolderUserId = lbdMsg.holderUserId
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"AddTokenParent No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
            {
                logger.Warn("AddTokenParent fail");
                logger.Warn($"                    ParentTokenType={lbdMsg.pTokenType} ParentTokenIndex={lbdMsg.pTokenIdx}");
                logger.Warn($"                    ChildernTokenType={lbdMsg.cTokenType} ChildernTokenIndex={lbdMsg.cTokenIdx}");
            }
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("AddTokenParent");
                    logger.Debug($"           {JValue.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
                }
                else
                {
                    NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TransactionError()
                    {
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        errorCode = InternalMsg.Error.AddTokenParent,
                        statusCode = txHashResult.statusCode,
                        statusMessage = txHashResult.statusMessage
                    });

                    logger.Warn($"AddTokenParent StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_RemoveTokenParent(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_RemoveTokenParent;

            var uriData = GetUriData(ApiUriType.RemoveTokenParent);
            uriData.tokenType = lbdMsg.cTokenType;
            uriData.tokenIdx = lbdMsg.cTokenIdx;

            RemoveTokenParentBody transferBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.holderAddr))
            {
                transferBody = new ToWallet_RemoveTokenParentBody()
                {
                    serviceWalletAddress = lbdInfo.operatorAddr,
                    serviceWalletSecret = lbdInfo.secretKey,
                    tokenHolderAddress = lbdMsg.holderAddr
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.holderUserId))
            {
                transferBody = new ToUserId_RemoveTokenParentBody()
                {
                    serviceWalletAddress = lbdInfo.operatorAddr,
                    serviceWalletSecret = lbdInfo.secretKey,
                    tokenHolderUserId = lbdMsg.holderUserId
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"RemoveToken No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
            {
                logger.Warn($"RemoveToken fail ");
                logger.Warn($"                    ChildernTokenType={lbdMsg.cTokenType} ChildernTokenIndex={lbdMsg.cTokenIdx}");
            }
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("RemoveToken");
                    logger.Debug($"           {JValue.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
                }
                else
                {
                    NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TransactionError()
                    {
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        errorCode = InternalMsg.Error.RemoveTokenParent,
                        statusCode = txHashResult.statusCode,
                        statusMessage = txHashResult.statusMessage
                    });

                    logger.Warn($"RemoveToken StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_TokenTypeInfo_ServiceToken(Msg.LBDBaseMsg msg)
        {
            var uriData = GetUriData(ApiUriType.TokenTypeInfo_ServiceToken);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenTypeInfo_ServiceToken fail");
            else
            {
                GetLBDServiceTokenType(result);

                logger.Debug("TokenTypeInfo_ServiceToken");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string TokenTypeInfo_ServiceTokenByContractId(string contractId)
        {
            var uriData = GetUriData(ApiUriType.TokenTypeInfo_ServiceTokenByContractId);
            uriData.serviceContractId = contractId;

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenTypeInfo_ServiceTokenByContractId fail");
            else
            {
                logger.Debug("TokenTypeInfo_ServiceTokenByContractId");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TokenBalancesOf_ServiceToken(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TokenBalancesOf_ServiceToken;

            var uriData = GetUriData(ApiUriType.TokenBalancesOf_ServiceToken);

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                uriData.walletAddr = lbdMsg.toAddr;
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                uriData.userId = lbdMsg.toUserId;
            }
            else
            {
                logger.Warn($"TokenBalancesOf_Service_Token No destination address");
                return null;
            }

            uriData.quary = PageQuary(lbdMsg.limit, lbdMsg.orderBy, lbdMsg.page);

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenBalancesOf_Service_Token fail owner={lbdMsg.toAddr} or userId={lbdMsg.toUserId}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TokenBalancesOf_ServiceToken()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    result = result,
                    page = lbdMsg.page
                });

                logger.Debug("TokenBalancesOf_Service_Token");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string TokenBalancesOf_ServiceTokenByContractId(string toAddr, string toUserId)
        {
            var uriData = GetUriData(ApiUriType.TokenBalancesOf_ServiceTokenByContractId);

            if (!string.IsNullOrEmpty(toAddr))
            {
                uriData.walletAddr = toAddr;
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                uriData.userId = toUserId;
            }
            else
            {
                logger.Warn($"TokenBalancesOf_Service_TokenByContractId No destination address");
                return null;
            }

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TokenBalancesOf_Service_TokenByContractId fail owner={toAddr} or userId={toUserId}");
            else
            {
                logger.Debug("TokenBalancesOf_Service_TokenByContractId");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string MintServiceToken(string toAddr, string toUserId, Int32 amount)
        {
            var uriData = GetUriData(ApiUriType.MintServiceToken);

            MintFungibleBody mintBody = null;

            if (!string.IsNullOrEmpty(toAddr))
            {
                mintBody = new ToWallet_MintFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toAddress = toAddr,
                    amount = amount.ToString(),
                };
            }
            else if (!string.IsNullOrEmpty(toUserId))
            {
                mintBody = new ToUserId_MintFungibleBody()
                {
                    ownerAddress = lbdInfo.operatorAddr,
                    ownerSecret = lbdInfo.secretKey,
                    toUserId = toUserId,
                    amount = amount.ToString(),
                };
            }

            if (null == mintBody)
            {
                logger.Warn($"MintServiceToken No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(mintBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"MintServiceToken fail ToAddr={toAddr}");
            else
            {
                logger.Debug("MintServiceToken");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string BurnServiceToken(Int32 amount)
        {
            var uriData = GetUriData(ApiUriType.BurnServiceToken);

            var burnBody = new BurnFungibleBody()
            {
                ownerAddress = lbdInfo.operatorAddr,
                ownerSecret = lbdInfo.secretKey,
                amount = amount.ToString()
            };

            var body = JsonConvert.SerializeObject(burnBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"BurnServiceToken fail");
            else
            {
                logger.Debug("BurnServiceToken");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_TransferServiceToken(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TransferServiceToken;
            var uriData = GetUriData(ApiUriType.TransferServiceToken);
            uriData.walletAddr = lbdMsg.fromAddr;
            uriData.serviceContractId = lbdMsg.contractId;

            Wallet_FungibleTransferBody transferBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                transferBody = new Wallet_ToWallet_FungibleTransferBody()
                {
                    walletSecret = lbdMsg.fromSecret,
                    toAddress = lbdMsg.toAddr,
                    amount = lbdMsg.amount.ToString(),
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                transferBody = new Wallet_ToUserId_FungibleTransferBody()
                {
                    walletSecret = lbdMsg.fromSecret,
                    toUserId = lbdMsg.toUserId,
                    amount = lbdMsg.amount.ToString(),
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"TransferServiceToken No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TransferServiceToken fail From={lbdMsg.fromAddr} ToAddr={lbdMsg.toAddr} ToUserId={lbdMsg.toUserId}");
            else
            {
                var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                if (txHashResult.statusCode == 1002)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("TransferServiceToken");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TransactionError()
                    {
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        errorCode = InternalMsg.Error.TransferServiceToken,
                        statusCode = txHashResult.statusCode,
                        statusMessage = txHashResult.statusMessage
                    });

                    logger.Warn($"TransferServiceToken StatusCode={txHashResult.statusCode} StatusMessage={txHashResult.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_IssueServiceTokenTransfer(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_IssueServiceTokenTransfer;
            var uriData = GetUriData(ApiUriType.IssueServiceTokenTransfer);
            uriData.userId = lbdMsg.fromUserId;
            uriData.serviceContractId = lbdMsg.contractId;
            uriData.quary = "requestType=aoa";

            IssueServiceTokenTransferBody transferBody = null;

            if (!string.IsNullOrEmpty(lbdMsg.toAddr))
            {
                transferBody = new ToWallet_IssueServiceTokenTransferBody()
                {
                    toAddress = lbdMsg.toAddr,
                    amount = lbdMsg.amount.ToString(),
                    landingUri = lbdMsg.landingUri
                };
            }
            else if (!string.IsNullOrEmpty(lbdMsg.toUserId))
            {
                transferBody = new ToUserId_IssueServiceTokenTransferBody()
                {
                    toUserId = lbdMsg.toUserId,
                    amount = lbdMsg.amount.ToString(),
                    landingUri = lbdMsg.landingUri
                };
            }

            if (null == transferBody)
            {
                logger.Warn($"IssueServiceTokenTransfer No destination address");
                return null;
            }

            var body = JsonConvert.SerializeObject(transferBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"IssueServiceTokenTransfer fail From={lbdMsg.fromUserId} ToAddr={lbdMsg.toAddr} ToUserId={lbdMsg.toUserId}");
            else
            {
                var requestProxy = JsonConvert.DeserializeObject<Respones_IssueServiceTokenTransferInfo>(result);

                if (requestProxy.statusCode == 1000)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.RequestCommit,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = requestProxy.responseData.requestSessionToken,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("IssueServiceTokenTransfer");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"IssueServiceTokenTransfer StatusCode={requestProxy.statusCode} StatusMessage={requestProxy.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_GetProxy(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_GetProxy;

            var uriData = GetUriData(ApiUriType.GetProxy);
            uriData.userId = lbdMsg.lineuserid;

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"GetProxy fail userId={lbdMsg.lineuserid}");
            else
            {
                NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_GetProxy()
                {
                    uid = lbdMsg.uid,
                    guid = lbdMsg.guid,
                    result = result
                });

                logger.Debug("GetProxy");
                logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
            }

            return result;
        }

        public string LBDMsg_RequestProxy(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_RequestProxy;
            var uriData = GetUriData(ApiUriType.RequestProxy);
            uriData.userId = lbdMsg.fromUserId;
            uriData.quary = "requestType=aoa";

            var reqProxyBody = new RequestProxyBody()
            {
                ownerAddress = lbdInfo.operatorAddr,
                landingUri = lbdMsg.landingUri
            };

            var body = JsonConvert.SerializeObject(reqProxyBody);

            var result = RequestResult(uriData, body);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"RequestProxy fail userId={lbdMsg.fromUserId}");
            else
            {
                var requestProxy = JsonConvert.DeserializeObject<Respones_RequestProxy>(result);

                if (requestProxy.statusCode == 1000)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.RequestCommit,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = requestProxy.responseData.requestSessionToken,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("RequestProxy");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"RequestProxy StatusCode={requestProxy.statusCode} StatusMessage={requestProxy.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_CommitUserRequest(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_CommitUserRequest;
            var uriData = GetUriData(ApiUriType.CommitUserRequest);
            uriData.requestSessionToken = lbdMsg.requestSessionToken;

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"CommitUserRequest fail");
            else
            {
                var requestProxy = JsonConvert.DeserializeObject<ResponesDataHeader>(result);

                if (requestProxy.statusCode == 4036)
                {
                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.RequestCommit,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = lbdMsg.requestSessionToken,
                        receiveTime = DateTime.Now,
                        count = lbdMsg.count + 1
                    });

                    logger.Debug("CommitUserRequest");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else if (requestProxy.statusCode == 1002)
                {
                    var txHashResult = JsonConvert.DeserializeObject<Respones_TransactionHash>(result);

                    AddtransactionResult(new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = txHashResult.responseData.txHash,
                        receiveTime = DateTime.Now,
                        count = 1
                    });

                    logger.Debug("CommitUserRequest");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
                else
                {
                    logger.Warn($"CommitUserRequest StatusCode={requestProxy.statusCode} StatusMessage={requestProxy.statusMessage}");
                }
            }

            return result;
        }

        public string LBDMsg_TransactionHash(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_TransactionHash;

            var uriData = GetUriData(ApiUriType.TransactionHash);
            uriData.txHash = lbdMsg.txHash;

            var result = RequestResult(uriData, string.Empty);
            if (string.IsNullOrEmpty(result))
                logger.Warn($"TransactionHash Result fail");
            else
            {
                var status = JsonConvert.DeserializeObject<ResponesDataHeader>(result);

                if (status.statusCode == 1002)
                {
                    var data = new TransactionResultData()
                    {
                        trType = TransactionResultType.TxHash,
                        uid = lbdMsg.uid,
                        guid = lbdMsg.guid,
                        TR_result = lbdMsg.txHash,
                        receiveTime = DateTime.Now,
                        count = lbdMsg.count + 1
                    };

                    AddtransactionResult(data);
                    logger.Debug("TransactionHash ReAddtransactionResult ");
                }
                else
                {
                    if (lbdMsg.uid == 0 && lbdMsg.guid == "issue")
                    {
                        GetLBDItemTokenIssue(result);
                    }
                    else
                    {
                        NetServer.InternalHandleMessageByUid(new InternalMsg.IntlMsg_TransactionHash()
                        {
                            uid = lbdMsg.uid,
                            guid = lbdMsg.guid,
                            result = result
                        });
                    }
                    logger.Debug("TransactionHash");
                    logger.Debug($"           {JValue.Parse(result).ToString(Formatting.Indented)}");
                }
            }

            return result;
        }

        public string LBDMsg_BurnFungible(Msg.LBDBaseMsg msg)
        {
            var lbdMsg = msg as Msg.LBDMsg_BurnFungible;

            return BurnFungible(lbdMsg.toAddr, lbdMsg.toUserId, lbdMsg.tokenType, lbdMsg.amount, lbdMsg.uid, lbdMsg.guid);
        }
    }
}
