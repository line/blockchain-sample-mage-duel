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

namespace TCGGameService.LBD
{
	public enum ApiUriType
	{
		None = 0,

        GetUser,

        IssueFungible,
        IssueNonFungible,

		TokenTypeInfo_NonFungible,
		TokenTypeInfo_Fungible,

		CreateWallet,

		MintNonFungible,
        MultiMintNonFungible,
		MintFungible,

		BurnNonFungible,
		BurnFungible,

		TransferNonFungible,
		TransferFungible,

		BatchTransfer,

        TokenBalancesOf_NonFungible,
        TokenBalancesOf_Fungible,

        TokenTypeBalancesOf_NonFungible,
        TokenTypeBalancesOf_Fungible,

		TransactionHash,

		GetTokenParent,
		AddTokenParent,
		RemoveTokenParent,
		GetTokenChildren,
		GetTokenRoot,

        TokenTypeInfo_ServiceToken,
        TokenTypeInfo_ServiceTokenByContractId,

        TokenBalancesOf_ServiceToken,
        TokenBalancesOf_ServiceTokenByContractId,

        MintServiceToken,
        BurnServiceToken,

        TransferServiceToken,
        IssueServiceTokenTransfer,

        GetProxy,
        RequestProxy,

        CommitUserRequest,
	};
}
