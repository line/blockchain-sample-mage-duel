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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TCGGameService
{
    public partial class TCGGameSrv
    {
        public Setting.ServerWebHostInfo serverWebHostInfo;

        public void InitializeWebServer()
        {
            serverWebHostInfo = Setting.ProgramSetting.Instance.serverWebHostInfo;
        }

        public void Get_Authorize(string code, string state, bool friendship_status_changed, string error, string description, string ip = "")
        {
            try
            {
                var reqData = JsonConvert.SerializeObject(new HttpMsg.ReqAuthorizationCallBack()
                {
                    code = string.IsNullOrEmpty(code) ? string.Empty : code,
                    state = string.IsNullOrEmpty(state) ? string.Empty : state,
                    friendship_status_changed = friendship_status_changed,
                    error = string.IsNullOrEmpty(error) ? string.Empty : error,
                    description = string.IsNullOrEmpty(description) ? string.Empty : description
                });

                var response = messageDispatcher.Dispatch(new DispatchData()
                {
                    messageType = HttpMsg.MessageType.ReqAuthorizationCallBack,
                    ReqData = reqData,
                    IP = ip
                });

                //logger.Debug($"\n{JValue.Parse(JsonConvert.SerializeObject(response)).ToString(Formatting.Indented)}");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
            }
        }

        public HttpMsg.MessageResponse Invoke(string reqData, string ip = "")
        {
            try
            {
                var message = JsonConvert.DeserializeObject<HttpMsg.Message>(reqData);

                var response = messageDispatcher.Dispatch(new DispatchData()
                {
                    messageType = message.type,
                    ReqData = reqData,
                    IP = ip
                });

                logger.Debug($"\n{JValue.Parse(JsonConvert.SerializeObject(response)).ToString(Formatting.Indented)}");
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);

                return new HttpMsg.AckError()
                {
                    errCode = (int)HttpMsg.Error.InternalError
                };
            }
        }
    }
}
