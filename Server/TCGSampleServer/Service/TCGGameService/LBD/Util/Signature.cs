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
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TCGGameService.LBD
{
    public class Signature
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static void Sort(JObject jObj)
        {
            var props = jObj.Properties().ToList();
            foreach (var prop in props)
            {
                prop.Remove();
            }

            foreach (var prop in props.OrderBy(p => p.Name))
            {
                jObj.Add(prop);
                if (prop.Value is JObject)
                {
                    Sort((JObject)prop.Value);
                }
                else if (prop.Value is JArray)
                {
                    foreach (var prop2 in (JArray)prop.Value)
                    {
                        if (prop2 is JObject)
                        {
                            Sort((JObject)prop2);
                        }
                    }
                }
            }
        }

        static string BodyGenerator(JObject jObj)
        {
            var props = jObj.Properties().ToList();
            var dataStr = new List<string>();
            var arrData = new Dictionary<string, List<string>>();

            foreach (var prop in props)
            {
                if (prop.Value.Type == JTokenType.Array)
                {
                    foreach (var obj in prop.Value.Children<JObject>())
                    {
                        foreach (var arrProp in obj.Properties())
                        {
                            if (arrProp.Value.Type == JTokenType.Null || arrProp.Value.Type == JTokenType.String)
                            {
                                var key = $"{prop.Name}.{arrProp.Name}";
                                if (arrData.ContainsKey(key))
                                {
                                    var value = string.Empty;
                                    if (JTokenType.Null != arrProp.Value.Type)
                                        value = arrProp.Value.ToString();
                                    arrData[key].Add(value);
                                }
                                else
                                {
                                    var strList = new List<string>();
                                    var value = string.Empty;
                                    if (JTokenType.Null != arrProp.Value.Type)
                                        value = arrProp.Value.ToString();

                                    strList.Add(value);
                                    arrData.Add(key, strList);
                                }
                            }
                            else
                            {
                                
                            }
                        }
                    }

                    foreach (var iter in arrData)
                    {
                        var value = string.Join(",", iter.Value);
                        var tempstr = value.Replace(",", "");
                        if (string.IsNullOrEmpty(tempstr))
                            continue;

                        var str = $"{iter.Key}={value}";
                        dataStr.Add(str);
                    }
                }
                else if (prop.Value.Type == JTokenType.String)
                {
                    var str = $"{prop.Name}={prop.Value.ToString()}";
                    dataStr.Add(str);
                }
            }

            var result = $"{string.Join("&", dataStr)}";

            logger.Debug($"{result}");

            return result;
        }

        public static string GetSignature(UriData data, string uri, string body)
        {
            var bodyGen = string.Empty;
            if (!string.IsNullOrEmpty(body))
            {
                var jObj = (JObject)JsonConvert.DeserializeObject(body);
                Sort(jObj);
                bodyGen = BodyGenerator(jObj);
            }

            var result = $"{data.nonce}{data.timestamp}{data.method}{uri}";

            if (!string.IsNullOrEmpty(data.quary))
            {
                result = $"{result}?{data.quary}";
                if (!string.IsNullOrEmpty(bodyGen))
                {
                    result = $"{result}&{bodyGen}";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(bodyGen))
                {
                    result = $"{result}?{bodyGen}";
                }
            }

            logger.Debug($"{result}");

            return CreateToken(result, data.serviceApiSecret);
        }

        static string CreateToken(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha = new HMACSHA512(keyByte))
            {
                byte[] hashmessage = hmacsha.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
