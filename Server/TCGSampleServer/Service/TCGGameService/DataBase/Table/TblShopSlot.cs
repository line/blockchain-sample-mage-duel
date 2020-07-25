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

namespace TCGGameService.Table
{
    [Dapper.Contrib.Extensions.Table("tbl_shopslot")]
    public class TblShopSlot
    {
        public static string Name = "tbl_shopslot";

        [Dapper.Contrib.Extensions.Key]
        [DataBase.DbColumn(autoincrement: true)]
        public Int64 suid { get; set; }
        [DataBase.DbIndex(group: 1, unique: true)]
        public Int64 uuid { get; set; }
        [DataBase.DbIndex(group: 1, unique: true)]
        public Int32 shopType { get; set; }
        [DataBase.DbIndex(group: 1, unique: true)]
        public Int32 slotIndex { get; set; }
        public DateTime resetTime { get; set; }
        public Int32 productType { get; set; }
        public Int32 productSubType { get; set; }
        public Int32 sellCount { get; set; }
        public Int32 priceType { get; set; }
        public Int32 priceValue { get; set; }
        public string itemName { get; set; }
        public Int32 extraValue { get; set; }
    }
}
