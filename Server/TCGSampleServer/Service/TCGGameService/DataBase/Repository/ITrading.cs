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

namespace TCGGameService.Repository
{
	public interface ITrading
	{
		long TradeInsert(Table.TblTrade TblTrade);
		bool TradeUpdate(Table.TblTrade TblTrade);
		bool TradeUpdate(Int64 seq, TcpMsg.TradeStatus setstatus, TcpMsg.TradeStatus wherestatus);
		bool TradeDelete(Table.TblTrade TblTrade);
		bool TradeDelete(Int64 seq);

        bool ExistsTrade(string tokenType, string tokenIdx);

        List<Table.TblTrade> GetTblTrade(Int64 uuid);
		Table.TblTrade GetTblTradeSeq(Int64 seq, TcpMsg.TradeStatus status = TcpMsg.TradeStatus.None);
		(Int32, Int32, List<Table.TblTrade>) GetTblTrade(Int16 jobType, Int32 cardId, Int32 tokenIdxMin, Int32 tokenIdxMax, TcpMsg.TradingSortObject obj, Int32 pageNum, Int32 itemCountPerPage, TcpMsg.SortingType sort);

		int TotalCount();

		// Trade History
		long HistoryInsert(Table.TblTradeHistory tblTradeHistory);
		(Int32, List<Table.TblTradeHistory>) GetTblTradeHistory(Int64 uuid, Int32 pageNum, Int32 itemCountPerPage);
	}
}
