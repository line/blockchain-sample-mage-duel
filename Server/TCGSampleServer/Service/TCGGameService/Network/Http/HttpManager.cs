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
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TCGGameService
{
    public class HttpManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static readonly Lazy<HttpManager> instance = new Lazy<HttpManager>(() => new HttpManager());
        public static HttpManager Instance
        {
            get { return instance.Value; }
        }

        public async Task<(Int32, string)> Request(HttpWebRequest webRequest)
        {
            Task<(Int32, string)> task = Task<(Int32, string)>.Factory.StartNew(() => Send(webRequest));
            return await task;
        }

        public (Int32, string) Send(HttpWebRequest webRequest)
        {
            try
            {
                logger.Debug(webRequest.ToString());

                var reqdata = string.Empty;
                Int32 ret = 0;
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                reqdata = reader.ReadToEnd();
                            }
                        }
                    }

                    ret = (Int32)response.StatusCode;
                }

                return (ret, reqdata);
            }
            catch (WebException ex)
            {
                Int32 ret = 0;
                var reqdata = string.Empty;
                using (var stream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        reqdata = reader.ReadToEnd();
                        logger.Warn(reqdata);
                    }
                }

                ret = (Int32)((HttpWebResponse)ex.Response).StatusCode;

                return (ret, reqdata);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message);
            }

            return (0, string.Empty);
        }
    }
}
