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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TCGGameService
{
	public class HttpMessageDispatcher
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		Dictionary<HttpMsg.MessageType, Func<HttpMsg.MessageType, DispatchData, HttpMsg.MessageResponse>> messageHandler = new Dictionary<HttpMsg.MessageType, Func<HttpMsg.MessageType, DispatchData, HttpMsg.MessageResponse>>();
		HttpHandler httpHandler = new HttpHandler();

		public HttpMessageDispatcher()
		{
			var enumNames = Enum.GetNames(typeof(HttpMsg.MessageType)).ToList();

			foreach (MethodInfo item in typeof(HttpHandler).GetMethods())
			{
				string str = enumNames.Find(x => x == item.Name);
				if (string.IsNullOrEmpty(str))
					continue;
				var enumValue = (HttpMsg.MessageType)Enum.Parse(typeof(HttpMsg.MessageType), str);
				var func = (Func<HttpMsg.MessageType, DispatchData, HttpMsg.MessageResponse>)Delegate.CreateDelegate(typeof(Func<HttpMsg.MessageType, DispatchData, HttpMsg.MessageResponse>), httpHandler, item.Name);
				messageHandler.Add(enumValue, func);
			}
		}

		public HttpMsg.MessageResponse Dispatch(DispatchData dispatchData)
		{
			string tmpLogText = JValue.Parse(JsonConvert.SerializeObject(dispatchData)).ToString(Formatting.Indented);
			logger.Debug($"\r\n{tmpLogText}");

			if (messageHandler.ContainsKey(dispatchData.messageType) == false)
			{
				// not support message type;
				return new HttpMsg.MessageResponse();
			}

			return messageHandler[dispatchData.messageType](dispatchData.messageType, dispatchData);
		}

	}
}
