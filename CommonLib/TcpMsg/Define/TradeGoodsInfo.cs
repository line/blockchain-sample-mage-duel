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
using System.Text;
using System.Threading.Tasks;

namespace TcpMsg
{
    public class TradeGoodsInfo
    {
        public Int64 seq;
        public Int64 uuid;
        public CardInfo card;
        public string comment;
        public Int32 expirationTime;
        public DateTime regdate;
        public Int32 sale_price;
    }

    public class TradeGoodsHistory
    {
        public Int64 seq;
        public Int64 selleruuid;
        public TradeType tradeType;
        public string targetName;
        public CardInfo card;
        public string comment;
        public Int32 sale_price;
        public DateTime regdate;
    }

    public class TradeCost
    {
        public Int32 sale_price;
        public List<CardInfo> cardInfos;
    }
}
