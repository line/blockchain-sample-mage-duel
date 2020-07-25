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
using System.Globalization;

namespace TcpMsg
{
    public class DeckInfo
    {
        public string name;
        public CardInfo heroCard;  
        public List<CardInfo> unitList;

        public string GetDeckUid()
        {
            return heroCard.GetCardID();
        }
    }

    public class CardInfo
    {
        // Card information is not stored in Db.

        public string tokenType;
        public string tokenIndex;

        public Int32 meta;          // ResourceId

        public string GetCardID()   // Card UId
        {
            return $"{tokenType}{tokenIndex}";
        }

        public Int32 GetCardIndex()
        {
            Int32 strnum;

            if (!string.IsNullOrEmpty(tokenIndex))
            {
                if (!Int32.TryParse(tokenIndex, 
                                    NumberStyles.HexNumber, 
                                    CultureInfo.InvariantCulture, 
                                    out strnum))
                {
                    strnum = 0;
                }
            }
            else
            {
                strnum = 0;
            }

            return strnum;
        }
    }
}
