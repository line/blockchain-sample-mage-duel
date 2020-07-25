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
        public void ReqGenerateUserKey(NetMessage message)
        {
            var ackData = new TcpMsg.AckGenerateUserKey();

            if (null != user)
            {
                NetServer.OnAuth(user);

                ackData.loginUserKey = user.Id.ToString();
                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqVerifyAccessToken(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqVerifyAccessToken>();

            if (null != user)
            {
                if (!string.IsNullOrEmpty(reqData.accessToken))
                {
                    user.SetAccessToken(reqData.accessToken);
                    LDC.LDCApiManager.Instance.AddLDCCmd(new LDC.Msg.LDCMsg_VerifyAccessToken()
                    {
                        guid = user.Id.ToString(),
                        accessToken = reqData.accessToken
                    });
                }
                else
                {
                    var ackData = new TcpMsg.AckVerifyAccessToken();
                    ackData.errCode = TcpMsg.Error.AccessToken_NullOrEmpty;
                    ackData.authType = TcpMsg.AuthType.AuthFail;

                    user.Send(new Packet(ackData));
                    logger.Warn($"GUID={user.Id.ToString()} Error Code={ackData.errCode.ToString()}");
                }
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqCreateNickName(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqCreateNickName>();

            if (null != user)
            {
                user.CreateNickName(reqData.nickName);
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqLBWJoinCompleted(NetMessage message)
        {
            if (null != user)
            {
                user.GetWalletAddress();
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqUserData(NetMessage message)
        {
            if (null != user)
            {
                LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TokenBalancesOf_Fungible()
                {
                    uid = user.UID,
                    guid = user.Id.ToString(),
                    toAddr = string.Empty,
                    toUserId = user.lineUID,

                    limit = 50,
                    orderBy = string.Empty,
                    page = 1
                });

                user.stateType = UserStateType.InitGetFungibleToken;
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqLobbyEntry(NetMessage message)
        {
            if (null != user)
            {
                user.LobbyEntry();
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }
    }
}
