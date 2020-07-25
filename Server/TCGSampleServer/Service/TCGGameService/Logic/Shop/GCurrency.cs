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

namespace TCGGameService
{
    public class GCurrency : Currency
    {
        private Table.TblCurrency tblCurrency;
        public GCurrency(User user, Table.TblCurrency tblCurrency)
        {
            currencyType = Currency.CurrencyType.Currency_nonToken;

            this.user = user;
            this.tblCurrency = tblCurrency;
        }
        protected override void SetPrice(long change)
        {
            tblCurrency.priceValue += change;
            UpdateDB();
        }
        public override Resource.PriceType PriceType()
        {
            if (tblCurrency == null)
                return 0;

            return (Resource.PriceType)tblCurrency.priceType;
        }
        public override Int64 PriceValue()
        {
            if (tblCurrency == null)
                return 0;

            return tblCurrency.priceValue;
        }
        private void UpdateDB()
        {
            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();
            repoUser.UpdateCurrency(new List<Table.TblCurrency>() { tblCurrency }.ToArray());
        }
        public override bool CheckConsume(Resource.PriceType priceType, Int64 priceValue)
        {
            if (tblCurrency == null)
            {
                logger.Error($"tblCurrency is null UserID={user.UID} priceType={priceType} priceValue={priceValue}");
                return false;
            }            

            if (priceType != (Resource.PriceType)tblCurrency.priceType)
            {
                logger.Error($"data_shop.priceType != tblCurrency.priceType UserID={user.UID} priceType={priceType} priceValue={priceValue} tblCurrency.priceType={tblCurrency.priceType} tblCurrency.priceValue={tblCurrency.priceValue}");
                return false;
            }

            if (priceValue > tblCurrency.priceValue)
            {
                logger.Error($"data_shop.priceValue > tblCurrency.priceValue UserID={user.UID} priceType={priceType} priceValue={priceValue} tblCurrency.priceType={tblCurrency.priceType} tblCurrency.priceValue={tblCurrency.priceValue}");
                return false;
            }

            return true;
        }
        public override ConsumeState Consume(Resource.PriceType priceType, Int64 priceValue, string walletaddr, UserStateType userStateType, ShopSlot shopSlot)
        {
            if (!CheckConsume(priceType, priceValue))
                return ConsumeState.ConsumeState_Falied;

            if (priceValue == 0)
                return ConsumeState.ConsumeState_Done;

            SetPrice("", -priceValue);

            return ConsumeState.ConsumeState_Done;
        }
    }
}
