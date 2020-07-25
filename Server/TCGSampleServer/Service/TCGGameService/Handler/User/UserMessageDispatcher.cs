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
using Newtonsoft.Json;

using TcpNet.Packet;

namespace TCGGameService
{
    class UserMessageDispatcher
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        Dictionary<TcpMsg.MessageType, Action<NetMessage>> messageHandler = new Dictionary<TcpMsg.MessageType, Action<NetMessage>>();

        ConcurrentQueue<INetPacketStream> receivedQueue = new ConcurrentQueue<INetPacketStream>();

        UserHandler handler = null;

        public UserMessageDispatcher(User user)
        {
            handler = new UserHandler(user);

            var enumNames = Enum.GetNames(typeof(TcpMsg.MessageType))
                                .Where(g => g.IndexOf("Req") >= 0)
                                .ToList();

            foreach (MethodInfo item in typeof(UserHandler).GetMethods())
            {
                string strReq = enumNames.Find(x => x == item.Name);
                if (string.IsNullOrEmpty(strReq))
                    continue;
                var enumValue = (TcpMsg.MessageType)Enum.Parse(typeof(TcpMsg.MessageType), strReq);
                var action = (Action<NetMessage>)Delegate.CreateDelegate(typeof(Action<NetMessage>), handler, item.Name);

                messageHandler.Add(enumValue, action);
            }
        }

        public void HandleMessage(INetPacketStream packet)
        {
            receivedQueue.Enqueue(packet);
        }

        public void Update()
        {
            while (true)
            {
                if (receivedQueue.Count <= 0)
                    break;

                INetPacketStream packet = null;
                if (receivedQueue.TryDequeue(out packet))
                {
                    using (packet)
                    {
                        string json = "";
                        try
                        {
                            json = packet.Read<string>();
                            var msg = JsonConvert.DeserializeObject<TcpMsg.MessageResponse>(json);
                            if (messageHandler.ContainsKey(msg.type))
                            {
                                if (msg.type != TcpMsg.MessageType.ReqKeepAlive)
                                    logger.Trace($"recv type={msg.type.ToString()}, json={json}");

                                messageHandler[msg.type](new NetMessage(json));
                            }
                            else
                            {
                                logger.Error($"messageHandler.ContainsKey(msg.type) is false, type={msg.type.ToString()}, json={json}");
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"exception={e.ToString()}, json={json}");
                            break;
                        }
                    }
                }
                else
                {
                    logger.Warn($"receivedQueue Dequeue fail QueueCount={receivedQueue.Count}");
                    break;
                }
            }
        }
    }
}
