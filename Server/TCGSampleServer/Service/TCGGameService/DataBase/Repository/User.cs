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
using DataBase;

namespace TCGGameService.Repository
{
	public class User : IUser
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public long Insert(Table.TblUser tblUser)
		{
			long ret;
			using (IDbConnection conn = Database.GetDbConnection())
				ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblUser);

			return ret;
		}

		public bool Update(Table.TblUser tblUser)
		{
			bool ret = false;
			using (IDbConnection conn = Database.GetDbConnection())
				ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Update(conn, tblUser);

			return ret;
		}

		public void Delete(Table.TblUser tblUser)
		{

		}

		public void Delete(Int64 uuid)
		{

		}

		public bool ExistsUserUId(Int64 uuid)
		{
			string query = $"select EXISTS(select* from {Table.TblUser.Name} where uuid=@UUID) as success";

			bool ret = false;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				ret = Dapper.SqlMapper.Query<bool>(conn, query, new
				{
					UUID = uuid
				}).FirstOrDefault();
			}

			return ret;
		}

        public bool ExistsUserNickName(string nickName)
        {
            string query = $"select EXISTS(select* from {Table.TblUser.Name} where nickname=@NICKNAME) as success";

            bool ret = false;
            using (IDbConnection conn = Database.GetDbConnection())
            {
                ret = Dapper.SqlMapper.Query<bool>(conn, query, new
                {
                    NICKNAME = nickName
                }).FirstOrDefault();
            }

            return ret;
        }

        public Table.TblUser GetUserFromUid(Int64 uuid)
		{
			string query = $"select * from {Table.TblUser.Name} where uuid=@UUID";

			Table.TblUser record;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				record = Dapper.SqlMapper.Query<Table.TblUser>(conn, query, new
				{
					UUID = uuid
				}).FirstOrDefault();
			}
			return record;
		}

        public Table.TblUser GetUserFromAddress(string addr)
        {
            string query = $"select * from {Table.TblUser.Name} where address=@ADDR";

            Table.TblUser record;
            using (IDbConnection conn = Database.GetDbConnection())
            {
                record = Dapper.SqlMapper.Query<Table.TblUser>(conn, query, new
                {
                    ADDR = addr
                }).FirstOrDefault();
            }
            return record;
        }

        public Table.TblUser GetUserFromLineUserId(string lineuserid)
        {
            string query = $"select * from {Table.TblUser.Name} where lineuserid=@USERID";

            Table.TblUser record;
            using (IDbConnection conn = Database.GetDbConnection())
            {
                record = Dapper.SqlMapper.Query<Table.TblUser>(conn, query, new
                {
                    USERID = lineuserid
                }).FirstOrDefault();
            }
            return record;
        }

        public List<Table.TblUser> GetUser()
		{
			string query = $"select * from {Table.TblUser.Name}";

			List<Table.TblUser> record;
			using (IDbConnection conn = Database.GetDbConnection())
			{
				record = Dapper.SqlMapper.Query<Table.TblUser>(conn, query).ToList();
			}
			return record;
		}

        public void InsertDeck(Table.TblDeck[] tblDecks)
        {
            using (IDbConnection conn = Database.GetDbConnection())
                Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblDecks);
        }

        public long InsertDeck(Table.TblDeck tblDeck)
        {
            long ret;
            using (IDbConnection conn = Database.GetDbConnection())
                ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblDeck);

            return ret;
        }

        public bool UpdateDeck(Table.TblDeck tblDeck)
        {
            bool ret;
            using (IDbConnection conn = Database.GetDbConnection())
                ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Update(conn, tblDeck);

            return ret;
        }

        public Table.TblDeck GetDeck(string tokenid)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                return Dapper.SqlMapper.Query<Table.TblDeck>(conn,
                $"select * from {Table.TblDeck.Name} where tokenid=@TOKENID",
                new
                {
                    TOKENID = tokenid
                }).FirstOrDefault();
            }
        }

        public List<Table.TblDeck> GetDeckList(string[] tokenids)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                string ids = string.Format("tokenid in ({0})", string.Join(",", tokenids));
                return Dapper.SqlMapper.Query<Table.TblDeck>(conn,
                string.Format($"select * from {Table.TblDeck.Name} where {ids}")).ToList();
            }
        }

        public List<Table.TblShopSlot> GetShopSlotInfo(Int64 uuid)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                return Dapper.SqlMapper.Query<Table.TblShopSlot>(conn,
                            $"select * from {Table.TblShopSlot.Name} where uuid = {uuid}")
                            .ToList();
            }
        }
        public List<Table.TblShopSlotCard> GetShopSlotCardInfo(Int64[] suids)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                return Dapper.SqlMapper.Query<Table.TblShopSlotCard>(conn,
                            $"select * from {Table.TblShopSlotCard.Name} where suid in @SUIDS",
                            new
                            {
                                SUIDS = suids
                            }).ToList();
            }
        }
        public List<Table.TblShopSlotOpen> GetShopSlotOpenInfo(Int64 uuid)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                return Dapper.SqlMapper.Query<Table.TblShopSlotOpen>(conn,
                            $"select * from {Table.TblShopSlotOpen.Name} where uuid = {uuid}")
                            .ToList();
            }
        }
        public void InsertShopSlot(Table.TblShopSlot tblShopSlot)
        {
            using (IDbConnection conn = Database.GetDbConnection())
                Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblShopSlot);
        }
        public bool DeleteShopSlot(Table.TblShopSlot tblShopSlot)
        {
            bool ret;
            using (IDbConnection conn = Database.GetDbConnection())
                ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Delete(conn, tblShopSlot);

            return ret;
        }
        public (List<Table.TblShopSlotCard>, List<Table.TblShopSlotCard>) InsertShopSlotCard(Int64 suid, Int32[] cardids)
        {
            var insert = new List<Table.TblShopSlotCard>();
            var insert_failed = new List<Table.TblShopSlotCard>();

            using (IDbConnection conn = Database.GetDbConnection())
            {
                foreach (var cardid in cardids)
                {
                    var tblShopSlotCard = new Table.TblShopSlotCard();
                    tblShopSlotCard.suid = suid;
                    tblShopSlotCard.cardid = cardid;
                    try
                    {
                        Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblShopSlotCard);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"ex={ex.ToString()}");
                        insert_failed.Add(tblShopSlotCard);
                        continue;
                    }
                    insert.Add(tblShopSlotCard);
                }
            }
            return (insert, insert_failed);
        }
        public void InsertShopSlotOpen(Table.TblShopSlotOpen tblShopSlotOpen)
        {
            using (IDbConnection conn = Database.GetDbConnection())
                Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, tblShopSlotOpen);
        }
        public bool DeleteShopSlotCard(List<Table.TblShopSlotCard> tblShopCard)
        {
            bool ret;
            using (IDbConnection conn = Database.GetDbConnection())
                ret = Dapper.Contrib.Extensions.SqlMapperExtensions.Delete(conn, tblShopCard);

            return ret;
        }
        public List<Table.TblCurrency> GetCurrency(Int64 uuid)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                return Dapper.SqlMapper.Query<Table.TblCurrency>(conn,
                            $"select * from {Table.TblCurrency.Name} where uuid = @UUID",
                            new
                            {
                                UUID = uuid
                            }).ToList();
            }
        }
        public void InsertCurrency(Table.TblCurrency[] tblCurrencies)
        {
            using (IDbConnection conn = Database.GetDbConnection())
            {
                foreach (var it in tblCurrencies)
                {
                    Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(conn, it);
                }
            }
        }
        public void UpdateCurrency(Table.TblCurrency[] tblCurrencies)
        {
            using (IDbConnection conn = Database.GetDbConnection())
                Dapper.Contrib.Extensions.SqlMapperExtensions.Update(conn, tblCurrencies);
        }
    }
}
