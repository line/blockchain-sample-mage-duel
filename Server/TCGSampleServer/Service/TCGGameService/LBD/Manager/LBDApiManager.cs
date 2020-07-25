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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TCGGameService.LBD
{
	public partial class LBDApiManager
	{
        static readonly Lazy<LBDApiManager> instance = new Lazy<LBDApiManager>(() => new LBDApiManager());
        public static LBDApiManager Instance
        {
            get { return instance.Value; }
        }

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<TransactionResultData> transactionResultQueue;
        private readonly ConcurrentQueue<Msg.LBDBaseMsg> lbcmdQueue;

        protected Dictionary<Type, Func<Msg.LBDBaseMsg, string>> messageHandler = new Dictionary<Type, Func<Msg.LBDBaseMsg, string>>();

        Setting.LBDInfo lbdInfo;

        UriManager uriManager;
        Nonce nonce;

        public LBDApiManager()
        {
            var objectType = Assembly.GetExecutingAssembly().GetTypes()
                         .Where(t => t.Namespace == "TCGGameService.LBD.Msg")
                         .Where(t => t.IsClass && t.Name.IndexOf("LBDMsg_") == 0)
                         .Select(t => t)
                         .ToList();

            foreach (MethodInfo item in typeof(LBDApiManager).GetMethods())
            {
                var type = objectType.Find(x => x.Name == item.Name);
                if (null == type || !type.IsClass)
                    continue;
                var func = (Func<Msg.LBDBaseMsg, string>)Delegate.CreateDelegate(typeof(Func<Msg.LBDBaseMsg, string>), this, item.Name);

                messageHandler.Add(type, func);
            }

            transactionResultQueue = new ConcurrentQueue<TransactionResultData>();
            lbcmdQueue = new ConcurrentQueue<Msg.LBDBaseMsg>();

            Task.Factory.StartNew(ProcessTransactionResultQueue);
            Task.Factory.StartNew(ProcessLBDCommandQuene);

            nonce = new Nonce();
        }

        public void Initialize()
        {
            lbdInfo = Setting.ProgramSetting.Instance.lbdInfo;
            uriManager = new UriManager(lbdInfo.uri);

            var lbdMsg = new Msg.LBDMsg_TokenTypeInfo_Fungible()
            {
                guid = "tokentypeinfo",
                limit = 50,
                orderBy = string.Empty,
                page = 1
            };

            AddLBDCmd(lbdMsg);
        }

        void AddtransactionResult(TransactionResultData data)
        {
            transactionResultQueue.Enqueue(data);
        }

        public void AddLBDCmd(Msg.LBDBaseMsg data)
        {
            lbcmdQueue.Enqueue(data);
        }

        public void ProcessLBDCommandQuene()
        {
            while (true)
            {
                if (lbcmdQueue.Count > 0)
                {
                    Msg.LBDBaseMsg msg = null;
                    if (lbcmdQueue.TryDequeue(out msg))
                    {
                        try
                        {
                            if (messageHandler.ContainsKey(msg.msgType))
                            {
                                messageHandler[msg.msgType](msg);
                            }
                            else
                                Console.WriteLine($"MessageHandler Not Found Type {msg.msgType.Name}");
                        }
                        catch (Exception e)
                        {
                            logger.Error($"exception={e.ToString()}");
                            break;
                        }
                    }
                    else
                    {
                        logger.Warn($"lbcmdQueue Dequeue fail QueueCount={lbcmdQueue.Count}");
                    }
                }

                Thread.Sleep(1);
            }
        }

        public void ProcessTransactionResultQueue()
        {
            while (true)
            {

                if (transactionResultQueue.Count > 0)
                {
                    TransactionResultData result = null;
                    if (transactionResultQueue.TryDequeue(out result))
                    {
                        try
                        {
                            var nowDateTime = DateTime.Now;

                            var time = nowDateTime.Subtract(result.receiveTime);

                            Msg.LBDBaseMsg lbmsg;

                            if (result.trType == TransactionResultType.TxHash)
                            {
                                if (time.TotalSeconds > 2)
                                {
                                    lbmsg = new Msg.LBDMsg_TransactionHash()
                                    {
                                        uid = result.uid,
                                        guid = result.guid,
                                        txHash = result.TR_result,
                                        count = result.count
                                    };
                                    AddLBDCmd(lbmsg);
                                }
                                else
                                {
                                    transactionResultQueue.Enqueue(result);
                                }
                            }
                            else if (result.trType == TransactionResultType.RequestCommit)
                            {
                                if (time.TotalSeconds > 5)
                                {
                                    if (result.count <= 36)
                                    {
                                        lbmsg = new Msg.LBDMsg_CommitUserRequest()
                                        {
                                            uid = result.uid,
                                            guid = result.guid,
                                            requestSessionToken = result.TR_result,
                                            count = result.count
                                        };
                                        AddLBDCmd(lbmsg);
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    transactionResultQueue.Enqueue(result);
                                }
                            }
                            else
                            {
                                logger.Warn($"TransactionResultData Type Error TrType={result.trType.ToString()}");
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"exception={e.ToString()}");
                            break;
                        }
                    }
                    else
                    {
                        logger.Warn($"lbcmdQueue Dequeue fail QueueCount={lbcmdQueue.Count}");
                    }
                }

                Thread.Sleep(1);
            }
        }

		UriData GetUriData(ApiUriType type)
		{
			var result = new UriData();
			switch (type)
			{
				case ApiUriType.IssueFungible:
				case ApiUriType.IssueNonFungible:
				case ApiUriType.CreateWallet:
				case ApiUriType.MintNonFungible:
				case ApiUriType.MultiMintNonFungible:
				case ApiUriType.MintFungible:
				case ApiUriType.BurnNonFungible:
				case ApiUriType.BurnFungible:
				case ApiUriType.TransferNonFungible:
				case ApiUriType.TransferFungible:
				case ApiUriType.BatchTransfer:
				case ApiUriType.AddTokenParent:
                case ApiUriType.MintServiceToken:
                case ApiUriType.BurnServiceToken:
                case ApiUriType.TransferServiceToken:
                case ApiUriType.IssueServiceTokenTransfer:
                case ApiUriType.RequestProxy:
                case ApiUriType.CommitUserRequest:
					{
						result.type = type;
						result.method = "POST";
					}
					break;
                case ApiUriType.GetUser:
				case ApiUriType.TokenTypeInfo_NonFungible:
				case ApiUriType.TokenTypeInfo_Fungible:
				case ApiUriType.TokenBalancesOf_NonFungible:
				case ApiUriType.TokenBalancesOf_Fungible:
                case ApiUriType.TokenTypeBalancesOf_NonFungible:
                case ApiUriType.TokenTypeBalancesOf_Fungible:
				case ApiUriType.TransactionHash:
				case ApiUriType.GetTokenParent:
				case ApiUriType.GetTokenChildren:
				case ApiUriType.GetTokenRoot:
                case ApiUriType.TokenTypeInfo_ServiceToken:
                case ApiUriType.TokenTypeInfo_ServiceTokenByContractId:
                case ApiUriType.TokenBalancesOf_ServiceToken:
                case ApiUriType.TokenBalancesOf_ServiceTokenByContractId:
                case ApiUriType.GetProxy:
					{
						result.type = type;
						result.method = "GET";
					}
					break;
				case ApiUriType.RemoveTokenParent:
					{
						result.type = type;
						result.method = "DELETE";
					}
					break;
				default:
					break;
			}

			if (result.type == type)
			{
				result.serviceApiKey = lbdInfo.apiKey;
                result.serviceApiSecret = lbdInfo.apiSecret;
				result.signature = "brown";
                result.nonce = nonce.GetNonce();
                result.timestamp = UnixTime.NowUnixTimeMilliseconds();
                result.itemContractId = lbdInfo.itemTokenContractId;
            }

			return result;
		}

        public string RequestResult(UriData data, string body)
        {
            var webRequest = uriManager.GetWebRequest(data, body);

            if (null == webRequest)
            {
                logger.Warn("GetWebRequest Is null");
                return string.Empty;
            }

            var tResult = HttpManager.Instance.Request(webRequest);

            var result = tResult.Result;

            if (result.Item1 == 200 || result.Item1 == 202)
            {
                if (!string.IsNullOrEmpty(result.Item2))
                {
                    var status = JsonConvert.DeserializeObject<ResponesDataHeader>(result.Item2);
                    logger.Info($"Type={data.type.ToString()} ResponseTime={status.responseTime} StatusCode={status.statusCode} StatusMessage={status.statusMessage}");
                }
                else
                {
                    logger.Warn($"Type={data.type.ToString()} Result IsNullOrEmpt");
                }
            }
            else
            {
                logger.Warn($"Type={data.type.ToString()} StatusCode={result.Item1}");
            }

            return result.Item2;
        }
    }
}
