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
    [Dapper.Contrib.Extensions.Table("tbl_trade")]
    public class TblTrade
    {
        public static string Name = "tbl_trade";
        [Dapper.Contrib.Extensions.Key]
        [DataBase.DbColumn(autoincrement: true)]
        public Int64 seq { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Int64 uuid { get; set; }
        [DataBase.DbColumn(size: 100)]
        public string nickname { get; set; }
        [DataBase.DbColumn(size: 100)]
        public string userid { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Int32 cardid { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Int16 jobtype { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Int16 cardtype { get; set; }
        [DataBase.DbColumn(size: 50)]
        public string tokentype { get; set; }
        [DataBase.DbColumn(size: 50)]
        public string tokenidx { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Int64 tokenidxnum { get; set; }
        public Int16 grade { get; set; }
        public Int32 sale_price { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Int32 status { get; set; }
        [DataBase.DbColumn(size: 100)]
        public string comment { get; set; }
        [Dapper.Contrib.Extensions.ExplicitKey]
        public DateTime regdate { get; set; }
    }
}
