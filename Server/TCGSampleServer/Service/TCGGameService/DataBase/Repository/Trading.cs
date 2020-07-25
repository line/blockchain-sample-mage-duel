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
using System.Data;
using System.Linq;
using Dapper;
using DataBase;

namespace TCGGameService.Repository
{
	public class Trading : ITrading
	{
		public long TradeInsert(Table.TblTrade TblTrade)
		{
			long ret = 0 ;
			using (IDbConnection conn = Database.GetDbConnection())
				ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, TblTrade);

			return ret;
		}

		public bool TradeUpdate(Table.TblTrade TblTrade)
		{
			bool ret = false;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Update(conn, TblTrade);
			}

			return ret;
		}

		public bool TradeUpdate(Int64 seq, TcpMsg.TradeStatus setstatus, TcpMsg.TradeStatus wherestatus)
		{
			Int32 ret = 0;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string qry = $"update {Table.TblTrade.Name} set status=@SETSTATUS where seq=@SEQ and status=@WHERESTATUS;";
				ret = conn.Execute(qry, new
				{
					SETSTATUS = (Int32)setstatus,
					SEQ = seq,
					WHERESTATUS = (Int32)wherestatus
				});
			}

			return ret > 0;
		}

		public bool TradeDelete(Table.TblTrade TblTrade)
		{
			bool ret = false;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Delete(conn, TblTrade);
			}

			return ret;
		}

		public bool TradeDelete(Int64 seq)
		{
			Int32 ret = 0;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string qry = $"delete from {Table.TblTrade.Name} where seq=@SEQ;";

				ret = conn.Execute(qry, new { SEQ = seq });
			}

			return ret > 0;
		}

        public bool ExistsTrade(string tokenType, string tokenIdx)
        {
            return true;
        }

        public List<Table.TblTrade> GetTblTrade(Int64 uuid)
		{
			List<Table.TblTrade> ret = null;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string qry = $"select * from {Table.TblTrade.Name} where uuid=@UUID and status=@STATUS;";
				ret =Dapper.SqlMapper.Query<Table.TblTrade>(conn, qry, new
				{
					UUID = uuid,
					STATUS = (Int32)TcpMsg.TradeStatus.Onsale
				}).ToList();
			}
			return ret;
		}

		public Table.TblTrade GetTblTradeSeq(Int64 seq, TcpMsg.TradeStatus status = TcpMsg.TradeStatus.None)
		{
			Table.TblTrade ret = null;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string where = string.Empty;

				if (status != TcpMsg.TradeStatus.None)
					where = $"seq = {seq} and status = {(Int32)status}";
				else
					where = $"seq = {seq}";

				string qry = $"select * from {Table.TblTrade.Name} where {where};";
				ret = Dapper.SqlMapper.Query<Table.TblTrade>(conn, qry).FirstOrDefault();
			}
			return ret;
		}

		string TradingPagingQuery(string where, TcpMsg.TradingSortObject type, Int32 pageNum, Int32 itemCountPerPage, TcpMsg.SortingType sort)
		{
			string qry = string.Empty;
			string orderby = string.Empty;
			string sorttype = "asc";

			if (sort == TcpMsg.SortingType.Desc)
				sorttype = "desc";

			switch (type)
			{
				case TcpMsg.TradingSortObject.CardType:
					orderby = $"order by cardtype {sorttype}, tokenidx {sorttype}";
					break;
				case TcpMsg.TradingSortObject.Index:
					orderby = $"order by tokenidxnum {sorttype}";
					break;
				case TcpMsg.TradingSortObject.Grade:
					orderby = $"order by grade {sorttype}, cardid {sorttype}, tokenidx {sorttype}";
					break;
				case TcpMsg.TradingSortObject.RegDate:
					orderby = $"order by regDate {sorttype}, cardid {sorttype}, tokenidx {sorttype}";
					break;
				case TcpMsg.TradingSortObject.SalePrice:
					orderby = $"order by sale_price {sorttype}, cardid {sorttype}, tokenidx {sorttype}";
					break;
				default:
					return qry;
			}

			string limit = $"limit {pageNum * itemCountPerPage}, {itemCountPerPage}";

			qry = $"select * from {Table.TblTrade.Name} where {where} {orderby} {limit};";

			return qry;
		}

		public (Int32, Int32, List<Table.TblTrade>) GetTblTrade(Int16 jobType, Int32 cardId, Int32 tokenIdxMin, Int32 tokenIdxMax, TcpMsg.TradingSortObject obj, Int32 pageNum, Int32 itemCountPerPage, TcpMsg.SortingType sort)
		{
			var totalCount = 0;
			List<Table.TblTrade> ret = null;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string where = string.Empty;
				if (jobType > 0)
					where = $"jobtype={jobType} and ";

				if (cardId > 0)
					where += $"cardid={cardId} and ";

				if (tokenIdxMin > 0 && tokenIdxMax > 0)
					where += $"tokenidxnum between {tokenIdxMin} and {tokenIdxMax} and ";
				else if (tokenIdxMin < 0 && tokenIdxMax > 0)
					where += $"tokenidxnum <= {tokenIdxMax} and ";
				else if (tokenIdxMin > 0 && tokenIdxMax < 0)
					where += $"tokenidxnum >= {tokenIdxMin} and ";

				where += $"status={(Int32)TcpMsg.TradeStatus.Onsale} and regdate > (now() - interval {Define.EXPIRY_DATE} day)";

				string qry = $"select count(*) from {Table.TblTrade.Name} where {where};";
				totalCount = Dapper.SqlMapper.Query<int>(conn, qry).FirstOrDefault();

				if (totalCount > 0)
				{
					if (totalCount < pageNum * itemCountPerPage)
						pageNum = 0;

					qry = TradingPagingQuery(where, obj, pageNum, itemCountPerPage, sort);
					ret = Dapper.SqlMapper.Query<Table.TblTrade>(conn, qry).ToList();
				}
			}
			return (pageNum, totalCount, ret);
		}

		public int TotalCount()
		{
			int ret = 0;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string qry = $"select count(*) from {Table.TblTrade.Name} where regdate > (now() - interval 10 day);";
				ret = Dapper.SqlMapper.Query<int>(conn, qry).FirstOrDefault();
			}

			return ret;
		}

		public long HistoryInsert(Table.TblTradeHistory tblTradeHistory)
		{
			long ret = 0;
			using (IDbConnection conn = Database.GetDbConnection())
				ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblTradeHistory);

			return ret;
		}

		public (Int32, List<Table.TblTradeHistory>) GetTblTradeHistory(Int64 uuid, Int32 pageNum, Int32 itemCountPerPage)
		{
			var totalCount = 0;
			List<Table.TblTradeHistory> ret = null;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				string where = $"seller_uuid={uuid} or buyer_uuid={uuid}";

				string qry = $"select count(*) from {Table.TblTradeHistory.Name} where {where}";
				totalCount = Dapper.SqlMapper.Query<int>(conn, qry).FirstOrDefault();

				if (totalCount > 0)
				{
					string limit = $"limit {pageNum * itemCountPerPage }, {itemCountPerPage} ";
					qry = $"select * from {Table.TblTradeHistory.Name} where {where} order by regDate desc {limit};";
					ret = Dapper.SqlMapper.Query<Table.TblTradeHistory>(conn, qry).ToList();
				}
			}

			return (totalCount, ret);
		}
	}
}
