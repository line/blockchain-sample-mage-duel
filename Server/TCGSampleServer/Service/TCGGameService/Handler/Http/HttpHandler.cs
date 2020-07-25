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
using Newtonsoft.Json.Linq;

namespace TCGGameService
{
	public partial class HttpHandler
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public HttpMsg.MessageResponse ReqEcho(HttpMsg.MessageType reqType, DispatchData request)
        {
            logger.Debug($"\n{JValue.Parse(request.ReqData).ToString(Formatting.Indented)}");

            var reqData = request.GetData<HttpMsg.ReqEcho>();

            var ackData = new HttpMsg.AckEcho();

            ackData.uid = reqData.uid;
            ackData.message = reqData.message;

            return ackData;
        }

        public HttpMsg.MessageResponse ReqAuthorizationCallBack(HttpMsg.MessageType reqType, DispatchData request)
        {
            logger.Debug($"\n{JValue.Parse(request.ReqData).ToString(Formatting.Indented)}");

            var reqData = request.GetData<HttpMsg.ReqAuthorizationCallBack>();

            var intlData = new InternalMsg.IntlMsg_Authorization();
            intlData.guid = reqData.state;

            intlData.state = reqData.state;
            intlData.code = reqData.code;
            intlData.friendship_status_changed = reqData.friendship_status_changed;

            intlData.error = reqData.error;
            intlData.description = reqData.description;

            NetServer.InternalHandleMessageByGuid(intlData);

            return null;
        } 
    }
}
