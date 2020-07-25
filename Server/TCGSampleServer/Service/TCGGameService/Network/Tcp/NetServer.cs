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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TcpNet.Packet;

namespace TCGGameService
{
	public partial class NetServer : TcpNet.Server.NetServer<User>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static readonly IPacketProcessor NPacketProcessor = new NetPacketProcessor();
		private static readonly ConcurrentDictionary<Guid, User> UserDictionary = new ConcurrentDictionary<Guid, User>();

        protected override IPacketProcessor PacketProcessor => NPacketProcessor;

		readonly CancellationTokenSource updateTaskCancelTokenSource = new CancellationTokenSource();
		readonly CancellationToken updateCancelToken;

		Task processUpater;

		ConcurrentBag<User> disconnectedBag = new ConcurrentBag<User>();

		public NetServer(string host, int port)
		{
			this.Configuration.Backlog = 100;
			this.Configuration.Port = port;
			this.Configuration.MaximumNumberOfConnection = 1000;
			this.Configuration.Host = host;
			this.Configuration.BufferSize = 20;
			this.Configuration.Blocking = false;

            lptimer.OnTImer = OnTImer;
            lptimer.AddTimer(TIMER_ID.TIMER_ID_Statistics, Constants.TIME_60_SEC, true);

            processUpater = Task.Run(async () =>
			{
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();
				
				while (true)
				{
                    try
                    {
                        var elapsed = stopWatch.ElapsedMilliseconds;

                        if (updateCancelToken.IsCancellationRequested)
                            break;

                        Parallel.ForEach(ClientDictionary, (x) =>
                        {
                            x.Value.Update();
                        });

                        while (!disconnectedBag.IsEmpty)
                        {
                            disconnectedBag.TryTake(out User user);
                            if (user.UID != 0)
                            {
                                UserDictionary.TryRemove(user.Id, out User removeUser);
                            }
                            user.OnDisconnected();
                        }

                        await Task.Delay(10);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                    }
                }
			});
		}

		protected override void Initialize()
		{
            logger.Info("Server Network READY!!!");
		}
        protected override bool OnClientConnected(User connection)
        {
            var result = base.OnClientConnected(connection);
            if (result)
                connection.OnConnected();

            return result;
        }
        protected override bool OnClientDisconnected(User connection)
        {
            var result = base.OnClientDisconnected(connection);
            if (result)
                disconnectedBag.Add(connection);

            return result;
        }
        protected override void OnDebugSend(User connection, byte[] buffer)
		{
            var json = System.Text.Encoding.UTF8.GetString(buffer);
            var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<TcpMsg.MessageResponse>(json);
            if(msg.type != TcpMsg.MessageType.AckKeepAlive)
                logger.Trace($"user={connection.GetUserInfo()}, type={msg.type.ToString()}, json={json}");
		}
		protected override void OnSend(Guid id, byte[] buffer)
		{
// 			logger.Debug($"UserGuid={id}");
// 			logger.Debug($"AfterBuffer={BitConverter.ToString(buffer).Replace("-", "")}");
		}
		protected override void OnError(Guid id, string error)
		{
			logger.Debug($"UserGuid={id} Error={error}");
		}

		public static void OnAuth(User connection)
		{
			UserDictionary.TryAdd(connection.Id, connection);
		}

		public static User FindUser(Int64 uid)
		{
            return UserDictionary.Where(x => x.Value.UID == uid).Select(x => x.Value).FirstOrDefault();
        }

		public static User[] FindUsers(Int64[] uids)
		{
			User[] ret = new User[uids.Length];

			for (int i = 0; i < uids.Length; i++)
				ret[i] = FindUser(uids[i]);

			return ret;
		}

		public static User FindUser(string nickname)
		{
			return UserDictionary.Where(x => x.Value.nickName == nickname).Select(x => x.Value).FirstOrDefault();
		}

        public static User WalletAddressToFindUser(string address)
        {
            return UserDictionary.Where(x => x.Value.walletAddr == address).Select(x => x.Value).FirstOrDefault();
        }

        public static List<TcpMsg.UserInfo> GetUserInfos()
		{
			return UserDictionary.Select(x => 
											new TcpMsg.UserInfo()
											{
												uuid = x.Value.UID,
												nickName = x.Value.nickName
											}
										).ToList();
		}

        public static void InternalHandleMessageByGuid(InternalMsg.InternalBaseMsg msg)
        {
            var user = UserDictionary.Where(x => x.Value.Id.ToString() == msg.guid).Select(x => x.Value).FirstOrDefault();

            if (null != user)
            {
                user.InternalHandleMessage(msg);
            }
            else
            {
                logger.Warn($"Not Found User Guid = {msg.guid}");
            }
        }

        public static void InternalHandleMessageByUid(InternalMsg.InternalBaseMsg msg)
        {
            var user = UserDictionary.Where(x => x.Value.UID == msg.uid).Select(x => x.Value).FirstOrDefault();

            if (null != user)
            {
                user.InternalHandleMessage(msg);
            }
            else
            {
                logger.Warn($"Not Found User UID = {msg.uid}");
            }
        }

        public static List<TcpMsg.UserInfo> GetUserinfos(long uuid)
		{
			var userList = GetUserInfos();

			userList.RemoveAll(x => x.uuid == uuid);
			return userList;
		}

		public static IEnumerable<TcpNet.Common.INetUser> GetNetUsers(long uid)
		{
			return UserDictionary.Where(x => x.Value.UID != uid).Select(x => x.Value).ToList();
		}

		protected override void OnError(Exception exception)
		{
		}
        private void Statistics()   
        {
            var count = UserDictionary.Count;
			Console.Title = $"WritePoolCount()={WritePoolCount()} ReadPoolCount()={ReadPoolCount()} TotUserCount={ClientDictionary.Count}";
            foreach (var it in UserDictionary)
            {
            }
        }
	}
}
