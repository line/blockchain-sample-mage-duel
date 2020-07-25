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
using TcpNet.Packet;

namespace TCGGameService
{
	public partial class User : TcpNet.Common.NetUser
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override string GetUserInfo()
        {
            return $"{base.GetUserInfo()}, UID={UID}, nickName={nickName}, LineUid={tblUser?.uuid}";
        }
        public User()
        {
            tcpDispatcher = new UserMessageDispatcher(this);
            internalDispatcher = new InternalMessageDispatcher(this);

            lptimer.OnTImer = OnTImer;
            lptimer.AddTimer(TIMER_ID.TIMER_ID_KeepAliveCheck, Constants.TIME_5_SEC, true);
        }

        public override void HandleMessage(INetPacketStream packet)
        {
            tcpDispatcher.HandleMessage(packet);
        }

        public void InternalHandleMessage(InternalMsg.InternalBaseMsg msg)
        {
            internalDispatcher.HandleMessage(msg);
        }

        public void Update()
		{
            tcpDispatcher.Update();
            internalDispatcher.Update();
            lptimer.Update();
		}

		void KeepAliveCheck()
		{
			var nowDateTime = DateTime.Now;

			var time = nowDateTime.Subtract(keep_Alive);

			if (time.TotalSeconds > sendingCycle)
			{
                var ackData = new TcpMsg.AckKeepAlive();
                Send(new Packet(ackData));
                keep_Alive = nowDateTime;
            }

			time = nowDateTime.Subtract(check_Time);

			if (time.TotalSeconds > checkTime)
			{
                Server.DisconnectClient(Id);
                logger.Warn($"Keep_Alive Check DisconnectClient UserInfo={GetUserInfo()}");
            }
		}

        public void OnConnected()
        {
            logger.Info($"userInfo={GetUserInfo()}");

            var ackData = new TcpMsg.AckConnectPass();
            Send(new Packet(ackData));
        }

        public void OnDisconnected()
        {
            logger.Info($"userInfo={GetUserInfo()}");
        }
	}
}
