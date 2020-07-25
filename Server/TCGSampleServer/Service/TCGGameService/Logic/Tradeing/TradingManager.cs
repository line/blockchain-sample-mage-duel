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

namespace TCGGameService
{
	public class TradingManager
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		static readonly Lazy<TradingManager> instance = new Lazy<TradingManager>(() => new TradingManager());

		public static TradingManager Instance
		{
			get
			{
				return instance.Value;
			}
		}

		Dictionary<Int64, SellerData> sellerDatas;
		Dictionary<Int64, BuyerData> buyerDatas;

		public TradingManager()
		{
			sellerDatas = new Dictionary<Int64, SellerData>();
			buyerDatas = new Dictionary<Int64, BuyerData>();
		}

		public TcpMsg.TradeGoodsInfo TblTradeToTradeGoodsInfo(Table.TblTrade tblTrade)
		{
			var goodsInfo = new TcpMsg.TradeGoodsInfo()
			{
				seq = tblTrade.seq,
				uuid = tblTrade.uuid,
				card = new TcpMsg.CardInfo()
                {
                    tokenType = tblTrade.tokentype,
                    tokenIndex = tblTrade.tokenidx,
                    meta = tblTrade.cardid
                },
				comment = tblTrade.comment,
				expirationTime = Define.expirationTime,
				regdate = tblTrade.regdate,
				sale_price = tblTrade.sale_price
			};

			return goodsInfo;
		}

		public TcpMsg.TradeGoodsHistory TblTradeHistoryToTradeGoodsHistory(Int64 uuid, Table.TblTradeHistory tblTradeHistory)
		{
			TcpMsg.TradeType tradeType = TcpMsg.TradeType.None;
			string targetName = string.Empty;

			if (tblTradeHistory.status == (Int32)TcpMsg.TradeStatus.Expiration)
			{
				tradeType = TcpMsg.TradeType.Expiration;
			}
			else if (tblTradeHistory.status == (Int32)TcpMsg.TradeStatus.Soldout && uuid == tblTradeHistory.seller_uuid)
			{
				tradeType = TcpMsg.TradeType.Saleing;
				targetName = tblTradeHistory.buyer_nickname;
			}
			else if (tblTradeHistory.status == (Int32)TcpMsg.TradeStatus.Soldout && uuid == tblTradeHistory.buyer_uuid)
			{
				tradeType = TcpMsg.TradeType.Buying;
				targetName = tblTradeHistory.seller_nickname;
			}
			else
			{
				logger.Warn($"TradeHistory Error uuid={uuid} Status={((TcpMsg.TradeStatus)tblTradeHistory.status).ToString()} Seller={tblTradeHistory.seller_uuid} Buyer={tblTradeHistory.buyer_uuid} CardId={tblTradeHistory.cardid}");
			}

			var tradeGoodsHistory = new TcpMsg.TradeGoodsHistory()
			{
				seq = tblTradeHistory.seq,
				selleruuid = tblTradeHistory.seller_uuid,
				tradeType = tradeType,
				targetName = targetName,
				card = new TcpMsg.CardInfo()
                {
                    tokenType = LBD.LBDApiManager.TokenIdToTokenType(tblTradeHistory.tokenid),
                    tokenIndex = LBD.LBDApiManager.TokenIdToTokenIndex(tblTradeHistory.tokenid),
                    meta = tblTradeHistory.cardid
                },
				sale_price = tblTradeHistory.sale_price,
				comment = tblTradeHistory.comment,
				regdate = tblTradeHistory.regdate
			};

			return tradeGoodsHistory;
		}

		public bool AddSellerData(SellerData sellerData)
		{
			if (sellerDatas.ContainsKey(sellerData.uuid))
			{
				logger.Warn($"SaleData is Already registered UserUid={sellerData.uuid} TokenId={sellerData.card.GetTokenId()}");
				return false;
			}

			sellerDatas.Add(sellerData.uuid, sellerData);
			return true;
		}

		public void InsertTradeHistory(Table.TblTrade tblTrade, TcpMsg.TradeStatus status, Int64 buyeruuid, string buyernickName)
		{
            var repoTrade = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();

			if (!repoTrade.TradeDelete(tblTrade.seq))
			{
				logger.Warn($"Trade Delete fail seq={tblTrade.seq} status={status.ToString()}");
			}
			else
			{
				var tblTradeHistory = new Table.TblTradeHistory()
                {
                    seller_uuid = tblTrade.uuid,
                    seller_nickname = tblTrade.nickname,
                    buyer_uuid = buyeruuid,
                    buyer_nickname = buyernickName,
                    tokenid = $"{tblTrade.tokentype}{tblTrade.tokenidx}",
					cardid = tblTrade.cardid,
                    tokenidxnum = tblTrade.tokenidxnum,
                    sale_price = tblTrade.sale_price,
					status = (Int32)status,
					comment = tblTrade.comment,
					regdate = DateTime.Now
				};

				tblTradeHistory.seq = repoTrade.HistoryInsert(tblTradeHistory);
			}
		}

		public (Int32, List<TcpMsg.TradeGoodsHistory>) GetTradeHistory(Int64 uuid, Int32 pageNum, Int32 itemCountPerPage)
		{
			var repoTrade = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();
			var historys = repoTrade.GetTblTradeHistory(uuid, pageNum, itemCountPerPage);
			return (historys.Item1,
					historys.Item1 > 0 ? 
					historys.Item2.Select(x => TblTradeHistoryToTradeGoodsHistory(uuid, x)).ToList() : 
					null);
		}

		public List<Table.TblTrade> CheckGetTblTrades(Int64 uuid)
		{
			var repoTrade = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();
			var tblTrades = repoTrade.GetTblTrade(uuid);

			if (null != tblTrades && tblTrades.Count > 0)
			{
				var delTrades = new List<Table.TblTrade>();
				var temp = DateTime.Now.AddHours(-Define.expirationTime);

				foreach (var tblTrade in tblTrades)
				{
					if (temp > tblTrade.regdate)
						delTrades.Add(tblTrade);
				}

				foreach (var delTrade in delTrades)
				{
					var tblTrade = tblTrades.Find(x => x.seq == delTrade.seq);
					tblTrades.Remove(tblTrade);

					InsertTradeHistory(delTrade, TcpMsg.TradeStatus.Expiration, 0, string.Empty);					
				}
				delTrades.Clear();
			}

			if (null == tblTrades)
				tblTrades = new List<Table.TblTrade>();

			return tblTrades;
		}

		public (Int32, Int32, List<TcpMsg.TradeGoodsInfo>) GetTradeGoodsList(TcpMsg.ReqTradeInfo reqData)
		{
			var repoTrading = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();
			var goods = repoTrading.GetTblTrade(reqData.jobType, reqData.cardId, reqData.tokenIdxMin, reqData.tokenIdxMax, reqData.sortObj, reqData.pageNum, reqData.itemCountPerPage, reqData.sortType);
			return (goods.Item1, goods.Item2, goods.Item2 > 0 ?
					goods.Item3.Select(x => TradingManager.Instance.TblTradeToTradeGoodsInfo(x)).ToList() :
					null);
		}

		public Table.TblTrade GetTradeGoods(Int64 seq, TcpMsg.TradeStatus status = TcpMsg.TradeStatus.None)
		{
			var repoTrading = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();

			return repoTrading.GetTblTradeSeq(seq, status);
		}

		public bool DeleteTradeGoods(Int64 seq)
		{
			var repoTrading = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();

			return repoTrading.TradeDelete(seq);
		}

		public bool AddBuyerData(BuyerData buyerData)
		{
			var repoTrading = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();
			repoTrading.TradeUpdate(buyerData.goods.seq, TcpMsg.TradeStatus.Trading, TcpMsg.TradeStatus.Onsale);

			var tblTrade = repoTrading.GetTblTradeSeq(buyerData.goods.seq);

			if (null != tblTrade && tblTrade.status == (Int32)TcpMsg.TradeStatus.Trading)
			{
				buyerData.goods.status = tblTrade.status;
				buyerDatas.Add(buyerData.uuid, buyerData);

				return true;
			}

			return false;
		}

        public BuyerData GetBuyerData(Int64 uuid)
		{
			BuyerData ret = null;
			if (buyerDatas.ContainsKey(uuid))
			{
				ret = buyerDatas[uuid];
			}

			return ret;
		}

		public BuyerData GetBuyerDataAndRemove(Int64 uuid)
		{
			BuyerData ret = GetBuyerData(uuid);

			if (null != ret)
			{
				buyerDatas.Remove(uuid);
			}

			return ret;
		}

		public Table.TblTrade RegisterTradeGoods(Int64 uuid)
		{
			Table.TblTrade tblTrade = null;
			if (sellerDatas.ContainsKey(uuid))
			{
				var saleData = sellerDatas[uuid];

                var cardData = TCGGameSrv.ResourceDataLoader.Data_Card_List.Find(x => x.ID == saleData.card.GetResourceId());

                if (null != cardData)
                {
                    tblTrade = new Table.TblTrade()
                    {
                        uuid = saleData.uuid,
                        nickname = saleData.nickName,
                        userid = saleData.userid,
                        cardid = saleData.card.GetResourceId(),
                        jobtype = (Int16)cardData.jobType,
                        cardtype = (Int16)cardData.cardType,
                        tokentype = saleData.card.tokenType,
                        tokenidx = saleData.card.tokenIdx,
                        tokenidxnum = saleData.card.GetTokenIdxNum(),
                        grade = (Int16)cardData.grade,
                        sale_price = saleData.sale_price,
                        status = (Int32)TcpMsg.TradeStatus.Onsale,
                        comment = saleData.comment,
                        regdate = DateTime.Now
                    };

                    var repoTrade = TCGGameSrv.IocContainer.Resolve<Repository.ITrading>();

                    tblTrade.seq = repoTrade.TradeInsert(tblTrade);

                    sellerDatas.Remove(uuid);
                }
			}

			return tblTrade;
		}
	}
}
