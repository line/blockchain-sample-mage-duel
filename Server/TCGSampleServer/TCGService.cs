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

using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.Text;
using System.IO;

using TCGGameService;

namespace TCGSampleServer
{
	[ServiceContract]
	public interface IService
    {
        [OperationContract]
        [WebGet]
        void Authorize(string code, string state, bool friendship_status_changed, string error, string error_description);

        [OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "Req", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
		Stream Req(Stream content);
	}

	public class TCGService : IService
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        string GetAddress()
        {
            string address = "";

            OperationContext context = OperationContext.Current;

            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            if (properties.Keys.Contains(HttpRequestMessageProperty.Name))
            {
                HttpRequestMessageProperty messageProperty = properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;

                if (messageProperty != null)
                {
                    if (messageProperty.Headers["X-Forwarded-For"] != null)
                        address = messageProperty.Headers["X-Forwarded-For"];
                }
            }
            if (string.IsNullOrEmpty(address))
            {
                address = endpoint.Address;
            }

            return address;
        }

        public void Authorize(string code, string state, bool friendship_status_changed, string error, string error_description)
        {
            var address = GetAddress();

            logger.Debug($"WebGet Data Code={code} State={state} Friendship Status Changed={friendship_status_changed} Error={error} Description={error_description}");

            TCGGameSrv.Instance.Get_Authorize(code, state, friendship_status_changed, error, error_description, address);
        }

        public Stream Req(Stream content)
		{
            var address = GetAddress();

            string reqData = null;

			using (var sr = new StreamReader(content))
			{
				reqData = sr.ReadToEnd();
			}

			logger.Debug(string.Format("Receive = {0}", reqData));

			var tmp = TCGGameSrv.Instance.Invoke(reqData, address);
			string jsonClient = Newtonsoft.Json.JsonConvert.SerializeObject(tmp);

			WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
			return new MemoryStream(Encoding.UTF8.GetBytes(jsonClient));
		}
	}
}
