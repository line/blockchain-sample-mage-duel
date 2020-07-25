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

namespace TCGGameService
{
    public class InternalMessageDispatcher
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        Dictionary<Type, Action<InternalMsg.InternalBaseMsg>> messageHandler = new Dictionary<Type, Action<InternalMsg.InternalBaseMsg>>();
        InternalHandler handler = null;

        ConcurrentQueue<InternalMsg.InternalBaseMsg> receivedQueue = new ConcurrentQueue<InternalMsg.InternalBaseMsg>();

        public InternalMessageDispatcher(User user)
        {
            handler = new InternalHandler(user);

            var objectType = Assembly.GetExecutingAssembly().GetTypes()
                         .Where(t => t.Namespace == "InternalMsg")
                         .Where(t => t.IsClass && t.Name.IndexOf("IntlMsg_") == 0)
                         .Select(t => t)
                         .ToList();

            foreach (MethodInfo item in typeof(InternalHandler).GetMethods())
            {
                var type = objectType.Find(x => x.Name == item.Name);
                if (null == type || !type.IsClass)
                    continue;
                var action = (Action<InternalMsg.InternalBaseMsg>)Delegate.CreateDelegate(typeof(Action<InternalMsg.InternalBaseMsg>), handler, item.Name);

                messageHandler.Add(type, action);
            }
        }

        public void HandleMessage(InternalMsg.InternalBaseMsg msg)
        {
            receivedQueue.Enqueue(msg);
        }

        public void Update()
        {
            while (true)
            {
                if (receivedQueue.Count <= 0)
                    break;

                InternalMsg.InternalBaseMsg msg = null;
                if (receivedQueue.TryDequeue(out msg))
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
                    logger.Warn($"receivedQueue Dequeue fail QueueCount={receivedQueue.Count}");
                    break;
                }
            }
        }
    }
}
