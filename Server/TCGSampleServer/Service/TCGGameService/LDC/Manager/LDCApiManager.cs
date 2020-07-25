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
using Newtonsoft.Json.Linq;

namespace TCGGameService.LDC
{
    public partial class LDCApiManager
    {
        static readonly Lazy<LDCApiManager> instance = new Lazy<LDCApiManager>(() => new LDCApiManager());
        public static LDCApiManager Instance
        {
            get { return instance.Value; }
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<Msg.LDCBaseMsg> ldcmdQueue;

        Dictionary<Type, Action<Msg.LDCBaseMsg>> messageHandler = new Dictionary<Type, Action<Msg.LDCBaseMsg>>();

        Setting.LDCInfo ldcInfo;
        UriManager uriManager;

        public LDCApiManager()
        {
            var objectType = Assembly.GetExecutingAssembly().GetTypes()
                         .Where(t => t.Namespace == "TCGGameService.LDC.Msg")
                         .Where(t => t.IsClass && t.Name.IndexOf("LDCMsg_") == 0)
                         .Select(t => t)
                         .ToList();

            foreach (MethodInfo item in typeof(LDCApiManager).GetMethods())
            {
                var type = objectType.Find(x => x.Name == item.Name);
                if (null == type || !type.IsClass)
                    continue;
                var action = (Action<Msg.LDCBaseMsg>)Delegate.CreateDelegate(typeof(Action<Msg.LDCBaseMsg>), this, item.Name);

                messageHandler.Add(type, action);
            }

            ldcmdQueue = new ConcurrentQueue<Msg.LDCBaseMsg>();

            Task.Factory.StartNew(ProcessLDCCommandQuene);
        }

        public void Initialize()
        {
            ldcInfo = Setting.ProgramSetting.Instance.ldcInfo;
            uriManager = new UriManager(ldcInfo.uri);
        }

        public void AddLDCCmd(Msg.LDCBaseMsg data)
        {
            ldcmdQueue.Enqueue(data);
        }

        public void ProcessLDCCommandQuene()
        {
            while (true)
            {
                if (ldcmdQueue.Count > 0)
                {
                    Msg.LDCBaseMsg msg = null;
                    if (ldcmdQueue.TryDequeue(out msg))
                    {
                        try
                        {
                            if (messageHandler.ContainsKey(msg.msgType))
                                messageHandler[msg.msgType](msg);
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
                        logger.Warn($"lbcmdQueue Dequeue fail QueueCount={ldcmdQueue.Count}");
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
                case ApiUriType.VerifyAccesToken:
                case ApiUriType.GetProfile:
                    {
                        result.type = type;
                        result.method = "GET";
                    }
                    break;
            }

            return result;
        }

        public string RequestResult(UriData data)
        {
            var webRequest = uriManager.GetWebRequest(data);

            var tResult = HttpManager.Instance.Request(webRequest);

            var result = tResult.Result;

            if (result.Item1 == 200)
            {
                if (!string.IsNullOrEmpty(result.Item2))
                    logger.Info($"           {JValue.Parse(result.Item2).ToString(Formatting.Indented)}");
                else
                    logger.Warn($"Type={data.type.ToString()} Result IsNullOrEmpt");
            }
            else
                logger.Warn($"Type={data.type.ToString()} StatusCode={result.Item1}");

            return result.Item2;
        }
    }
}
