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

namespace TCGGameService.LBD
{
    public class ResponesDataHeader
    {
        public string responseTime;
        public Int32 statusCode;
        public string statusMessage;
    }

    public class TransactionHashData
    {
        public string txHash;
    }

    public class GetUserData
    {
        public string userId;
        public string walletAddress;
    }

    public class Respones_GetUserInfio : ResponesDataHeader
    {
        public GetUserData responseData;
    }

    public class Respones_TransactionHash : ResponesDataHeader
    {
        public TransactionHashData responseData;
    }

    public class TokenTypeData
    {
        public string tokenType;
        public string name;
        public string meta;
        public Int64 createdAt;
        public string totalSupply;
        public string totalMint;
        public string totalBurn;
    }

    public class Respones_GetTokenTypeInfos : ResponesDataHeader
    {
        public List<TokenTypeData> responseData;
    }

    public class FungibleTokenTypeBalancesData
    {
        public string tokenType;
        public string name;
        public string meta;
        public string amount;
    }

    public class Respones_GetFungibleTokenTypeBalances : ResponesDataHeader
    {
        public List<FungibleTokenTypeBalancesData> responseData;
    }

    public class NonFungibleTokenTypeBalancesData
    {
        public string tokenType;
        public string name;
        public string meta;
        public string numberOfIndex;
    }

    public class Respones_GetNonFungibleTokenTypeBalances : ResponesDataHeader
    {
        public List<NonFungibleTokenTypeBalancesData> responseData;
    }

    public class NonFungibleTokenIndexBalancesData
    {
        public string tokenIndex;
        public string name;
        public string meta;
    }

    public class Respones_GetNonFungibleTokenIndexBalances : ResponesDataHeader
    {
        public List<NonFungibleTokenIndexBalancesData> responseData;
    }

    public class GetComposableData
    {
        public string tokenId;
        public string name;
        public string meta;
        public Int64 createdAt;
        public Int64 burneAt;
    }

    public class Respones_GetComosableInfo : ResponesDataHeader
    {
        public GetComposableData responseData;
    }

    public class Respones_GetComosableListInfo : ResponesDataHeader
    {
        public List<GetComposableData> responseData;
    }

    public class SessionTokenData
    {
        public string requestSessionToken;
        public string redirectUrl;
    }

    public class Respones_IssueServiceTokenTransferInfo : ResponesDataHeader
    {
        public SessionTokenData responseData;
    }

    public class IsApproved
    {
        public bool isApproved;
    }

    public class Respones_GetProxyInfo : ResponesDataHeader
    {
        public IsApproved responseData;
    }

    public class Respones_RequestProxy : ResponesDataHeader
    {
        public SessionTokenData responseData;
    }

    public class ServiceTokenTypeData
    {
        public string contractId;
        public string ownerAddress;
        public string name;
        public string symbol;
        public string imgUri;
        public string meta;
        public Int32 decimals;
        public Int64 createdAt;
        public string totalSupply;
        public string totalMint;
        public string totalBurn;
        public string serviceId;
    }

    public class Respones_GetServiceTokens : ResponesDataHeader
    {
        public List<ServiceTokenTypeData> responseData;
    }

    public class Respones_GetServiceTokenByContractId : ResponesDataHeader
    {
        public ServiceTokenTypeData responseData;
    }

    public class ServiceTokenData
    {
        public string contractId;
        public string name;
        public string symbol;
        public string imageUri;
        public Int32 decimals;
        public Int32 amount;
    }

    public class Respones_GetServiceTokenBalance : ResponesDataHeader
    {
        public List<ServiceTokenData> responseData;
    }

    public class Respones_GetServiceTokenBalancByContractId : ResponesDataHeader
    {
        public ServiceTokenData responseData;
    }

    public class TxAttribute
    {
        public string key;
        public string value;
    }

    public class TxEvent
    {
        public string type;
        public List<TxAttribute> attributes;
    }

    public class TxLogData
    {
        public Int32 msgIndex;
        public bool success;
        public string log;
        public List<TxEvent> events;
    }

    public class TxMsgType
    {
        public string type;
    }

    public class IssueNFTValue
    {
        public string owner;
        public string contractId;
        public string name;
        public string meta;
    }

    public class IssueNFTMsg : TxMsgType
    {
        public IssueNFTValue value;
    }

    public class IssueFTValue_Target
    {
        public string type;
        public string body;
    }

    public class IssueFTValue
    {
        public IssueFTValue_Target owner;
        public string contractId;
        public IssueFTValue_Target to;
        public string name;
        public string meta;
        public Int32 amount;
        public bool mintable;
        public Int32 decimals;
    }

    public class IssueFTMsg : TxMsgType
    {
        public IssueFTValue value;
    }
}
