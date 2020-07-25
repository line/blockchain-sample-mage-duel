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

namespace TCGGameService
{
    public partial class UserHandler
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        User user = null;

        public UserHandler(User user)
        {
            this.user = user;
        }

        public void ReqEcho(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqEcho>();

            logger.Debug($"uid={reqData.uid} message={reqData.message}");

            var ackData = new TcpMsg.AckEcho()
            {
                uid = user.UID,
                message = reqData.message
            };

            user.Send(new Packet(ackData));
        }

        public void ReqKeepAlive(NetMessage message)
        {
            user.CheckTimeNowUpdate();
        }
    }
}
