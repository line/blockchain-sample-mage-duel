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
        public void ReqRegisterDeck(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqRegisterDeck>();

            if (null != user)
            {
                user.RegisterDeck(reqData.tokenId, reqData.deckName);
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqDeckNameChange(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqDeckNameChange>();

            if (null != user)
            {
                user.DeckNameChange(reqData.deckuid, reqData.deckName);
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqDeckChildrenCardAdd(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqDeckChildrenCardAdd>();

            if (null != user)
            {
                var errorCode = user.CheckDeckCount(reqData.deckuid);

                if (errorCode == TcpMsg.Error.None)
                {
                    user.stateType = UserStateType.DeckCardAdd;

                    LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_GetComposableToken()
                    {
                        uid = user.UID,
                        guid = user.Id.ToString(),
                        type = TcpMsg.GetComposableType.GetTokenParent,
                        tokenType = LBD.LBDApiManager.TokenIdToTokenType(reqData.tokenid),
                        tokenIdx = LBD.LBDApiManager.TokenIdToTokenIndex(reqData.tokenid),

                        limit = 50,
                        orderBy = string.Empty,
                        page = 1,

                        value = reqData.deckuid
                    });

                    return;
                }

                var ackData = new TcpMsg.AckDeckChildrenCardAdd();
                ackData.errCode = errorCode;

                user.Send(new Packet(ackData));
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }

        public void ReqDeckChildrenCardRemove(NetMessage message)
        {
            var reqData = message.GetData<TcpMsg.ReqDeckChildrenCardRemove>();

            if (null != user)
            {
                user.stateType = UserStateType.DeckCardRemove;

                LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_RemoveTokenParent()
                {
                    uid = user.UID,
                    guid = user.Id.ToString(),
                    holderAddr = string.Empty,
                    holderUserId = user.lineUID,
                    cTokenType = LBD.LBDApiManager.TokenIdToTokenType(reqData.tokenid),
                    cTokenIdx = LBD.LBDApiManager.TokenIdToTokenIndex(reqData.tokenid)
                });
            }
            else
            {
                logger.Warn($"Is User Null~~~");
            }
        }
    }
}
