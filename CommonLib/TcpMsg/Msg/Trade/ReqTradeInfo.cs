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

namespace TcpMsg
{
	public class ReqTradeInfo : Message
	{
		public ReqTradeInfo()
		{
			type = MessageType.ReqTradeInfo;
		}

		public Int16 jobType;

		public Int32 cardId;
		public Int32 tokenIdxMin;
		public Int32 tokenIdxMax;

		public TradingSortObject sortObj;
		public SortingType sortType;
		public Int32 pageNum;
		public Int32 itemCountPerPage;
	}
}
