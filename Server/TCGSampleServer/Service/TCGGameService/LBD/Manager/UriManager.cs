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
using System.IO;
using System.Net;

namespace TCGGameService.LBD
{
	public class UriData
	{
		public ApiUriType type;
		public string method;
		public string serviceApiKey;
        public string serviceApiSecret;
        public string signature;
        public Int64 timestamp;
        public string nonce;

		public string itemContractId;
        public string serviceContractId;
        public string walletAddr;
        public string userId;
		public string tokenType;
		public string tokenIdx;
        public string requestSessionToken;

		public string txHash;

		public string quary;
	}

	public class UriManager
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        string baseUri = string.Empty;
		string version = string.Empty;

		public UriManager(string uri)
		{
			baseUri = uri;
			version = "/v1";
		}

        bool Check_Alphabet_Case_Numeric(string type, string name, string str)
        {
            var ret = System.Text.RegularExpressions.Regex.IsMatch(str, @"^[a-zA-Z0-9]+$");
            if (!ret)
                logger.Warn($"{type} {name} Not Case_Numeric {str}");

            return ret;
        }

        bool CheckAlphabet_Lowercase_Numeric(string type, string name, string str)
        {
            var ret = System.Text.RegularExpressions.Regex.IsMatch(str, @"^[a-z0-9]+$");
            if (!ret)
                logger.Warn($"{type} {name} Not Lowercase_Numeric {str}");

            return ret;
        }

        bool CheckAlphabet_Uppercase_Numeric(string type, string name, string str)
        {
            var ret = System.Text.RegularExpressions.Regex.IsMatch(str, @"^[A-Z0-9]+$");
            if (!ret)
                logger.Warn($"{type} {name} Not Uppercase_Numeric {str}");

            return ret;
        }

        bool CheckNumeric(string type, string name, string str)
        {
            var ret = System.Text.RegularExpressions.Regex.IsMatch(str, @"^[0-9]+$");
            if (!ret)
                logger.Warn($"{type} {name} Not Numeric {str}");

            return ret;
        }

        public string GetUri(UriData data)
		{
			var result = string.Empty;
			switch (data.type)
			{
				case ApiUriType.IssueNonFungible:
				case ApiUriType.TokenTypeInfo_NonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles";
                    }
                    break;
				case ApiUriType.IssueFungible:
				case ApiUriType.TokenTypeInfo_Fungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/fungibles";
                    }
                    break;
				case ApiUriType.CreateWallet:
					result = "/wallets";
					break;
				case ApiUriType.MintNonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/mint";
                    }
					break;
				case ApiUriType.MultiMintNonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles/multi-mint";
                    }
					break;
				case ApiUriType.MintFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/fungibles/{data.tokenType}/mint";
                    }
					break;
				case ApiUriType.BurnNonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenIdx), data.tokenIdx))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/{data.tokenIdx}/burn";
                    }
					break;
				case ApiUriType.BurnFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/fungibles/{data.tokenType}/burn";
                    }
					break;
				case ApiUriType.TransferNonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenIdx), data.tokenIdx))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/{data.tokenIdx}/transfer";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/{data.tokenIdx}/transfer";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }					
					break;
				case ApiUriType.TransferFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/fungibles/{data.tokenType}/transfer";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/fungibles/{data.tokenType}/transfer";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    
					break;
				case ApiUriType.BatchTransfer:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/non-fungibles/batch-transfer";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/non-fungibles/batch-transfer";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
					break;
				case ApiUriType.TokenBalancesOf_NonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/non-fungibles";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/non-fungibles";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    break;
				case ApiUriType.TokenBalancesOf_Fungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/fungibles";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/fungibles";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    break;
                case ApiUriType.TokenTypeBalancesOf_NonFungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    break;
                case ApiUriType.TokenTypeBalancesOf_Fungible:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/item-tokens/{data.itemContractId}/fungibles/{data.tokenType}";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/fungibles/{data.tokenType}";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    break;
				case ApiUriType.TransactionHash:
                    {
                        if (!CheckAlphabet_Uppercase_Numeric(data.type.ToString(), nameof(data.txHash), data.txHash))
                            break;

                        result = $"/transactions/{data.txHash}";
                    }
					break;
				case ApiUriType.GetTokenParent:
				case ApiUriType.AddTokenParent:
				case ApiUriType.RemoveTokenParent:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenIdx), data.tokenIdx))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/{data.tokenIdx}/parent";
                    }
                    break;
				case ApiUriType.GetTokenChildren:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenIdx), data.tokenIdx))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/{data.tokenIdx}/children";
                    }
                    break;
				case ApiUriType.GetTokenRoot:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenType), data.tokenType))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.tokenIdx), data.tokenIdx))
                            break;

                        result = $"/item-tokens/{data.itemContractId}/non-fungibles/{data.tokenType}/{data.tokenIdx}/root";
                    }
					break;
                case ApiUriType.TokenTypeInfo_ServiceToken:
                    result = $"/service-tokens";
                    break;
                case ApiUriType.TokenTypeInfo_ServiceTokenByContractId:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.serviceContractId), data.serviceContractId))
                            break;

                        result = $"/service-tokens/{data.serviceContractId}";
                    }
                    break;
                case ApiUriType.TokenBalancesOf_ServiceToken:
                    {
                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/service-tokens";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/service-tokens";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    break;
                case ApiUriType.TokenBalancesOf_ServiceTokenByContractId:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.serviceContractId), data.serviceContractId))
                            break;

                        if (!string.IsNullOrEmpty(data.walletAddr))
                        {
                            if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                                break;

                            result = $"/wallets/{data.walletAddr}/service-tokens/{data.serviceContractId}";
                        }
                        else if (!string.IsNullOrEmpty(data.userId))
                        {
                            if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                                break;

                            result = $"/users/{data.userId}/service-tokens/{data.serviceContractId}";
                        }
                        else
                            logger.Warn($"{data.type.ToString()} No UserID or Address");
                    }
                    break;
                case ApiUriType.MintServiceToken:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.serviceContractId), data.serviceContractId))
                            break;

                        result = $"/service-tokens/{data.serviceContractId}/mint";
                    }
                    break;
                case ApiUriType.BurnServiceToken:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.serviceContractId), data.serviceContractId))
                            break;

                        result = $"/service-tokens/{data.serviceContractId}/burn";
                    }
                    break;
                case ApiUriType.TransferServiceToken:
                    {
                        if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.walletAddr), data.walletAddr))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.serviceContractId), data.serviceContractId))
                            break;

                        result = $"/wallets/{data.walletAddr}/service-tokens/{data.serviceContractId}/transfer";
                    }
                    break;
                case ApiUriType.IssueServiceTokenTransfer:
                    {
                        if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.serviceContractId), data.serviceContractId))
                            break;

                        result = $"/users/{data.userId}/service-tokens/{data.serviceContractId}/request-transfer";
                    }
                    break;
                case ApiUriType.GetUser:
                    {
                        if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                            break;

                        result = $"/users/{data.userId}";
                    }
                    break;
                case ApiUriType.GetProxy:
                    {
                        if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/proxy";
                    }
                    break;
                case ApiUriType.RequestProxy:
                    {
                        if (!Check_Alphabet_Case_Numeric(data.type.ToString(), nameof(data.userId), data.userId))
                            break;
                        else if (!CheckAlphabet_Lowercase_Numeric(data.type.ToString(), nameof(data.itemContractId), data.itemContractId))
                            break;

                        result = $"/users/{data.userId}/item-tokens/{data.itemContractId}/request-proxy";
                    }
                    break;
                case ApiUriType.CommitUserRequest:
                    {
                        if (string.IsNullOrEmpty(data.requestSessionToken))
                        {
                            logger.Warn($"{data.type.ToString()} SessionToken Is null or empty");
                            break;
                        }

                        result = $"/user-requests/{data.requestSessionToken}/commit";
                    }
                    break;
				default:
					break;
			}

			if (!string.IsNullOrEmpty(result))
				result = $"{version}{result}";

			return result;
		}

		public HttpWebRequest GetWebRequest(UriData data, string body)
		{
			var uri = GetUri(data);

            if (string.IsNullOrEmpty(uri))
            {
                logger.Warn("GetUri Is Null or Empty");
                return null;
            }

            data.signature = Signature.GetSignature(data, uri, body);

            var requestUriString = $"{baseUri}{uri}";
            if (!string.IsNullOrEmpty(data.quary))
            {
                requestUriString = $"{requestUriString}?{data.quary}";
            }

            logger.Debug($"RequestUri={requestUriString}");

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create($"{requestUriString}");
			webRequest.Method = data.method;
			webRequest.Accept = "*/*";

			if (!string.IsNullOrEmpty(data.serviceApiKey))
				webRequest.Headers.Add("service-api-key", data.serviceApiKey);

			if (!string.IsNullOrEmpty(data.signature))
				webRequest.Headers.Add("Signature", data.signature);

            if (data.timestamp > 0)
                webRequest.Headers.Add("Timestamp", data.timestamp.ToString());

            if (!string.IsNullOrEmpty(data.nonce))
                webRequest.Headers.Add("Nonce", data.nonce);

            if (!string.IsNullOrEmpty(body))
			{
				webRequest.ContentType = "application/json";
				TextWriter writer = new StreamWriter(webRequest.GetRequestStream());
				writer.Write(body);
				writer.Close();
			}

            logger.Debug($"uri={uri} method={data.method} apikey={data.serviceApiKey}");
            logger.Debug($"sign={data.signature} tstamp={data.timestamp} nonce={data.nonce}");
            logger.Debug($"body={body}");

			return webRequest;
		}
	}
}
