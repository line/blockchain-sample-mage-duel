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

namespace TcpMsg
{
	public class ShopSlotInfo
	{
        public enum eState : int
        {
            eState_Open,
            eState_Ready,
            eState_Lock,
            eState_LockOpen,
        };

        public Int32 shopType;
        public Int32 slotIndex;
        public Int32 unlockLevel;
        public Int32 state;             // 0:open, 1:ready, 2:Lock, 3:Unlock
        public Int64 remainSec;         // Time available for purchase (seconds) -1 there is no limit
        public Int32 productType;
        public Int32 productSubType;
        public List<Int32> cardids;
        public Int32 sellCount;
        public Int32 priceType;
        public Int32 priceValue;
        public string itemName;
        public Int32 extraValue;        // productType이 currency, etc when is used to display icons
        public CurrencyInfo readyPrice; 
        public DateTime time;           
    }
}
