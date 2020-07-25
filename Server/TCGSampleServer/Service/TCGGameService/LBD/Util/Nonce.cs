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

using System.Collections.Generic;
using System.Linq;

namespace TCGGameService.LBD
{
    public class Nonce
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        string[] numstr = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        string[] lowerstr = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        
        List<string> strList;

        public Nonce()
        {
            strList = new List<string>();
            strList.AddRange(numstr);
            strList.AddRange(lowerstr);
            strList.AddRange(lowerstr.Select(x => x.ToUpper()).ToArray());
        }

        public string GetNonce()
        {
            var temp = strList.PickRandom(8);

            var result = string.Empty;
            foreach(var a in temp)
            {
                result += a;
            }

            return result;
        }
    }
}
