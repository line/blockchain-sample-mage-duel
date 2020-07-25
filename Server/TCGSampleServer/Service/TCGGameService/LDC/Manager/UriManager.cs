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

using System.Net;

namespace TCGGameService.LDC
{
	public class UriData
    {
        public ApiUriType type;
        public string method;
        public string authorization;

        public string accessToken;
        public string contentType;
    }

	public class UriManager
	{
		string baseUri = string.Empty;

		public UriManager(string uri)
		{
			baseUri = uri;
		}

		public string GetUri(UriData data)
		{
			var result = string.Empty;
            switch (data.type)
            {
                case ApiUriType.VerifyAccesToken:
                    result = $"/oauth2/v2.1/verify?access_token={data.accessToken}";
                    data.contentType = "application/x-www-form-urlencoded";
                    break;
                case ApiUriType.GetProfile:
                    result = $"/v2/profile";
                    data.authorization = data.accessToken;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(result))
                result = $"{baseUri}{result}";

            return result;
		}

		public HttpWebRequest GetWebRequest(UriData data)
		{
			var uri = GetUri(data);
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
			webRequest.Method = data.method;
			webRequest.Accept = "*/*";

            if (!string.IsNullOrEmpty(data.contentType))
                webRequest.ContentType = data.contentType;

            if (!string.IsNullOrEmpty(data.authorization))
                webRequest.Headers.Add("Authorization", $"Bearer {data.authorization}");



            return webRequest;
		}
	}
}
