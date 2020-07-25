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

using System.Collections.Generic;

namespace TCGGameService.LBD
{
    public class IssueTokenBbody
    {
        public string ownerAddress;
        public string ownerSecret;
        public string name;
        public string meta;
    }

    public class MultiMintNonFungibleBody
    {
        public string ownerAddress;
        public string ownerSecret;
        public List<TokenTypeInfo> mintList;
    }

    public class ToWallet_MultiMintNonFungibleBody : MultiMintNonFungibleBody
    {
        public string toAddress;
    }

    public class ToUserId_MultiMintNonFungibleBody : MultiMintNonFungibleBody
    {
        public string toUserId;
    }

    public class MintNonFungibleBody
    {
        public string ownerAddress;
        public string ownerSecret;
        public string name;
        public string meta;
    }

    public class ToWallet_MintNonFungibleBody : MintNonFungibleBody
    {
        public string toAddress;
    }

    public class ToUserId_MintNonFungibleBody : MintNonFungibleBody
    {
        public string toUserId;
    }

    public class MintFungibleBody
    {
        public string ownerAddress;
        public string ownerSecret;
        public string amount;
    }

    public class ToWallet_MintFungibleBody : MintFungibleBody
    {
        public string toAddress;
    }

    public class ToUserId_MintFungibleBody : MintFungibleBody
    {
        public string toUserId;
    }

    public class BurnNonFungibleBody
    {
        public string ownerAddress;
        public string ownerSecret;
    }

    public class FromWallet_BurnNonFungibleBody : BurnNonFungibleBody
    {
        public string fromAddress;
    }

    public class FromUserId_BurnNonFungibleBody : BurnNonFungibleBody
    {
        public string fromUserId;
    }

    public class BurnFungibleBody
    {
        public string ownerAddress;
        public string ownerSecret;
        public string amount;
    }

    public class FromWallet_BurnFungibleBody : BurnFungibleBody
    {
        public string fromAddress;
    }

    public class FromUserId_BurnFungibleBody : BurnFungibleBody
    {
        public string fromUserId;
    }

    public class Wallet_NonFungibleTransferBody
    {
        public string walletSecret;
    }

    public class Wallet_ToWallet_NonFungibleTransferBody : Wallet_NonFungibleTransferBody
    {
        public string toAddress;
    }

    public class Wallet_ToUserId_NonFungibleTransferBody : Wallet_NonFungibleTransferBody
    {
        public string toUserId;
    }

    public class UserId_NonFungibleTransferBody
    {
        public string ownerAddress;
        public string ownerSecret;
    }

    public class UserId_ToWallet_NonFungibleTransferBody : UserId_NonFungibleTransferBody
    {
        public string toAddress;
    }

    public class UserId_ToUserId_NonFungibleTransferBody : UserId_NonFungibleTransferBody
    {
        public string toUserId;
    }


    public class Wallet_FungibleTransferBody
    {
        public string walletSecret;
        public string amount;
    }

    public class Wallet_ToWallet_FungibleTransferBody : Wallet_FungibleTransferBody
    {
        public string toAddress;
    }

    public class Wallet_ToUserId_FungibleTransferBody : Wallet_FungibleTransferBody
    {
        public string toUserId;
    }

    public class UserId_FungibleTransferBody
    {
        public string ownerAddress;
        public string ownerSecret;
        public string amount;
    }

    public class UserId_ToWallet_FungibleTransferBody : UserId_FungibleTransferBody
    {
        public string toAddress;
    }

    public class UserId_ToUserId_FungibleTransferBody : UserId_FungibleTransferBody
    {
        public string toUserId;
    }

    public class Wallet_BatchTransferBody
    {
        public string walletSecret;
        public List<TokenIdInfo> transferList;
    }

    public class Wallet_ToWallet_BatchTransferBody : Wallet_BatchTransferBody
	{
        public string toAddress;
    }

    public class Wallet_ToUserId_BatchTransferBody : Wallet_BatchTransferBody
    {
        public string toUserId;
    }

    public class UserId_BatchTransferBody
    {
        public string ownerAddress;
        public string ownerSecret;
        public List<TokenIdInfo> transferList;
    }

    public class UserId_ToWallet_BatchTransferBody : UserId_BatchTransferBody
    {
        public string toAddress;
    }

    public class UserId_ToUserId_BatchTransferBody : UserId_BatchTransferBody
    {
        public string toUserId;
    }

    public class AddTokenParentBody
	{
		public string parentTokenId;
		public string serviceWalletAddress;
		public string serviceWalletSecret;
	}

    public class ToWallet_AddTokenParentBody : AddTokenParentBody
    {
        public string tokenHolderAddress;
    }

    public class ToUserId_AddTokenParentBody : AddTokenParentBody
    {
        public string tokenHolderUserId;
    }

    public class RemoveTokenParentBody
    {
        public string serviceWalletAddress;
        public string serviceWalletSecret;
    }

    public class ToWallet_RemoveTokenParentBody : RemoveTokenParentBody
    {
        public string tokenHolderAddress;
    }

    public class ToUserId_RemoveTokenParentBody : RemoveTokenParentBody
    {
        public string tokenHolderUserId;
    }

    public class IssueServiceTokenTransferBody
    {
        public string amount;
        public string landingUri;
    }

    public class ToWallet_IssueServiceTokenTransferBody : IssueServiceTokenTransferBody
    {
        public string toAddress;
    }

    public class ToUserId_IssueServiceTokenTransferBody : IssueServiceTokenTransferBody
    {
        public string toUserId;
    }

    public class RequestProxyBody
    {
        public string ownerAddress;
        public string landingUri;
    }
}
