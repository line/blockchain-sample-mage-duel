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

namespace TCGGameService
{
    public partial class InternalHandler
    {
        public void IntlMsg_Authorization(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_Authorization;

            if (null != user)
            {
                var ackData = new TcpMsg.AckAuthorization();

                if (string.IsNullOrEmpty(intlMsg.error))
                {
                    ackData.code = intlMsg.code;
                    ackData.state = intlMsg.state;
                }
                else
                {
                    ackData.errCode = TcpMsg.Error.AuthorizationFail;
                }

                if (ackData.errCode != TcpMsg.Error.None)
                {
                    logger.Warn($"GUID={user.Id.ToString()} Error Code={ackData.errCode.ToString()}");
                }

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void IntlMsg_VerifyAccessToken(InternalMsg.InternalBaseMsg msg)
        {
            var intlMsg = msg as InternalMsg.IntlMsg_VerifyAccessToken;

            if (null != user)
            {
                var ackData = new TcpMsg.AckVerifyAccessToken();
                if (intlMsg.result)
                {
                    var accessToken = user.GetAccessToken();
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var ldcMsg = new LDC.Msg.LDCMsg_GetProfile()
                        {
                            guid = user.Id.ToString(),
                            accessToken = accessToken
                        };

                        LDC.LDCApiManager.Instance.AddLDCCmd(ldcMsg);
                        return;
                    }
                    else
                    {
                        ackData.errCode = TcpMsg.Error.AccessToken_NullOrEmpty;
                    }
                }
                else
                {
                    ackData.errCode = TcpMsg.Error.AccessToken_CheckError;
                }
                
                ackData.authType = TcpMsg.AuthType.AuthFail;

                user.Send(new Packet(ackData));
                logger.Warn($"GUID={user.Id.ToString()} Error Code={ackData.errCode.ToString()}");
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void IntlMsg_GetProfile(InternalMsg.InternalBaseMsg msg)
        {
            // After searching in Db with UserId, it is branched according to whether it is a new account or an existing account.
            var intlMsg = msg as InternalMsg.IntlMsg_GetProfile;

            if (null != user)
            {
                user.Login(intlMsg);
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }
    }
}
