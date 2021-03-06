﻿/*
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
    [Dapper.Contrib.Extensions.Table("tbl_deck")]
    public class TblDeck
    {
        public static string Name = "tbl_deck";
        [Dapper.Contrib.Extensions.Key]
        [DataBase.DbColumn(autoincrement: true)]
        public Int64 seq { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public string tokenid { get; set; }

        [DataBase.DbColumn(size: 50)]
        public string deckname { get; set; }
        public DateTime regDate { get; set; }
    }
}
