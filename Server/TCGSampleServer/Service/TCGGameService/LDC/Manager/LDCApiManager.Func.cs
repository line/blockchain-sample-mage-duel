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

using Newtonsoft.Json;

namespace TCGGameService.LDC
{
    public partial class LDCApiManager
    {
        public void LDCMsg_VerifyAccessToken(Msg.LDCBaseMsg msg)
        {
            var ldcData = msg as Msg.LDCMsg_VerifyAccessToken;

            UriData uriData;
            uriData = GetUriData(ApiUriType.VerifyAccesToken);
            uriData.accessToken = ldcData.accessToken;

            var result = RequestResult(uriData);

            if (!string.IsNullOrEmpty(result))
            {
                var respones = JsonConvert.DeserializeObject<Respones_VerifyAccessToken>(result);

                var intlMsg = new InternalMsg.IntlMsg_VerifyAccessToken()
                {
                    guid = ldcData.guid,
                };

                if (string.IsNullOrEmpty(respones.error))
                {
                    intlMsg.result = true;
                    intlMsg.scope = respones.scope;
                    intlMsg.client_id = respones.error;
                    intlMsg.expires_in = respones.expires_in;
                }
                else
                {
                    intlMsg.result = false;
                    intlMsg.error = respones.error;
                    intlMsg.description = respones.error_description;
                }

                NetServer.InternalHandleMessageByGuid(intlMsg);
            }
            else
            {
                logger.Warn($"HttpRequest fail Uid={msg.guid}");
            }
        }

        public void LDCMsg_GetProfile(Msg.LDCBaseMsg msg)
        {
            var ldcData = msg as Msg.LDCMsg_GetProfile;

            UriData uriData;
            uriData = GetUriData(ApiUriType.GetProfile);
            uriData.accessToken = ldcData.accessToken;

            var result = RequestResult(uriData);

            if (!string.IsNullOrEmpty(result))
            {
                var respones = JsonConvert.DeserializeObject<Respones_GetProfile>(result);

                var intlMsg = new InternalMsg.IntlMsg_GetProfile()
                {
                    guid = ldcData.guid,

                    userId = respones.userId,
                    displayName = respones.displayName,
                    pictureUrl = respones.pictureUrl,
                    statusMessage = respones.statusMessage
                };

                 NetServer.InternalHandleMessageByGuid(intlMsg);
            }
            else
            {
                logger.Warn($"HttpRequest fail Uid={msg.guid}");
            }
        }
    }
}
