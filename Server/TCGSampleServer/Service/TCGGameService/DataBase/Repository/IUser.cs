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
	public interface IUser
	{
		long Insert(Table.TblUser tblUser);
		bool Update(Table.TblUser tblUser);
		void Delete(Table.TblUser tblUser);
		void Delete(Int64 uuid);

		bool ExistsUserUId(Int64 uuid);
        bool ExistsUserNickName(string nickName);

        Table.TblUser GetUserFromUid(Int64 uuid);
        Table.TblUser GetUserFromAddress(string addr);
        Table.TblUser GetUserFromLineUserId(string lineuserid);
        List<Table.TblUser> GetUser();

        void InsertDeck(Table.TblDeck[] tblDecks);
        long InsertDeck(Table.TblDeck tblDeck);
        bool UpdateDeck(Table.TblDeck tblDeck);
        Table.TblDeck GetDeck(string tokenid);
        List<Table.TblDeck> GetDeckList(string[] tokenids);

        List<Table.TblShopSlot> GetShopSlotInfo(Int64 uuid);
        List<Table.TblShopSlotCard> GetShopSlotCardInfo(Int64[] suids);
        List<Table.TblShopSlotOpen> GetShopSlotOpenInfo(Int64 uuid);
        void InsertShopSlot(Table.TblShopSlot tblShopSlot);
        bool DeleteShopSlot(Table.TblShopSlot tblShopSlot);
        (List<Table.TblShopSlotCard>, List<Table.TblShopSlotCard>) InsertShopSlotCard(Int64 suid, Int32[] cardid);
        void InsertShopSlotOpen(Table.TblShopSlotOpen tblShopSlotOpen);
        bool DeleteShopSlotCard(List<Table.TblShopSlotCard> tblShopCard);
        List<Table.TblCurrency> GetCurrency(Int64 uuid);
        void InsertCurrency(Table.TblCurrency[] tblCurrencies);
        void UpdateCurrency(Table.TblCurrency[] tblCurrencies);
    }
}
