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
    public partial class User
    {
        public List<LBD.TokenInfo> fungibles = new List<LBD.TokenInfo>();
        public List<LBD.ServiceTokenInfo> serviceTokens = new List<LBD.ServiceTokenInfo>();

        private Dictionary<Resource.PriceType, Currency> currencies { get; set; } = new Dictionary<Resource.PriceType, Currency>();

        public void LoadCurrency()
        {
            if (currencies != null)
            {
                currencies.Clear();
            }

            var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();
            var tblCurrencies = repoUser.GetCurrency(UID);

            List<Table.TblCurrency> insert = new List<Table.TblCurrency>();

            foreach (var it_price in TCGGameSrv.ResourceDataLoader.Data_Price_List)
            {
                if (it_price.fungibleType == Resource.FungibleType.None)
                {
                    if (tblCurrencies.Where(x => x.priceType == (int)it_price.priceType).Count() == 0)
                    {
                        insert.Add(new Table.TblCurrency()
                        {
                            uuid = UID,
                            priceType = (int)it_price.priceType,
                            // TODO LSH Test Coin
                            priceValue = 9999999999
                        });
                    }
                }
                else if (it_price.fungibleType == Resource.FungibleType.Fungible)
                {
                    var tokenType = LBD.LBDApiManager.Instance.GetFungibleTypeToMeta(((Int32)it_price.priceType).ToString());

                    if (null != tokenType)
                    {
                        var fungible = fungibles.Where(x => x.tokenType == tokenType.tokenType).FirstOrDefault();

                        if (fungible != null)
                        {
                            var currency = new TCurrency(this, it_price.fungibleType, tokenType.tokenType, it_price.priceType, fungible.amount);
                            currencies.Add(currency.PriceType(), currency);
                        }
                        else
                        {
                            var currency = new TCurrency(this, it_price.fungibleType, tokenType.tokenType, it_price.priceType, 0);
                            currencies.Add(currency.PriceType(), currency);
                        }
                    }
                    else
                    {
                        logger.Error($"Data_Price invalid, tokentype is null, UserID={UID} priceType={it_price.priceType} fungibleType={it_price.fungibleType}");
                    }

                    /*
                     * TODO : Belldan 
                     * 엑셀 테이블에 tokenType을 추가 하였는데
                     * 엑셀을 읽어 it_price.priceType 으로 Token을 Create 한것으로
                     * 엑셀의 데이터를 기반으로 생성된것이어서 priceType으로 tokenType를 얻어오는 함수가 있음
                     * LBD.LBDApiManager.Instance.GetFungibleTypeToMeta
                     * 
                     * 아래함수는 엑셀테이블로 Token을 생성하는 함수임
                     * LBD.LBDApiManager.Instance.GetLBDItemTokenType_Fungible 참조
                     
                    if (!string.IsNullOrEmpty(it_price.tokenType))
                    {
                        if (tokenInfos.ContainsKey(it_price.tokenType))
                        {
                            var currency = new TCurrency(this, it_price.tokenType, it_price.priceType, tokenInfos[it_price.tokenType]);
                            currencies.Add(currency.PriceType(), currency);
                        }
                        else
                        {
                            var currency = new TCurrency(this, it_price.tokenType, it_price.priceType, 0);
                            currencies.Add(currency.PriceType(), currency);
                        }
                    }
                    else
                    {
                        logger.Error($"Data_Price invalid, tokentype is null, UserID={UID} priceType={it_price.priceType} fungibleType={it_price.fungibleType}");
                    }
                    */
                }
                else if (it_price.fungibleType == Resource.FungibleType.ServiceToken)
                {
                    var tokenType = LBD.LBDApiManager.Instance.GetServiceTokenType(Setting.ProgramSetting.Instance.lbdInfo.serviceToeknContractId);

                    if (null != tokenType)
                    {
                        var serviceToken = serviceTokens.Where(x => x.contractId == tokenType.contractId).FirstOrDefault();
                        if (serviceToken != null)
                        {
                            var currency = new TCurrency(this, it_price.fungibleType, tokenType.contractId, it_price.priceType, serviceToken.amount);
                            currencies.Add(currency.PriceType(), currency);
                        }
                        else
                        {
                            var currency = new TCurrency(this, it_price.fungibleType, tokenType.contractId, it_price.priceType, 0);
                            currencies.Add(currency.PriceType(), currency);
                        }
                    }
                    else
                    {
                        logger.Error($"Data_Price invalid, tokentype is null, UserID={UID} priceType={it_price.priceType} fungibleType={it_price.fungibleType}");
                    }
                }
                else
                {
                    logger.Error($"Data_Price invalid UserID={UID} priceType={it_price.priceType} fungibleType={it_price.fungibleType}");
                }
            }
            if (insert.Count > 0)
            {
                repoUser.InsertCurrency(insert.ToArray());
                tblCurrencies.AddRange(insert);
            }

            foreach (var tblCurrency in tblCurrencies)
            {
                var currency = new GCurrency(this, tblCurrency);
                currencies.Add(currency.PriceType(), currency);
            }
        }

        public Currency GetCurrency(Resource.PriceType priceType)
        {
            if (currencies == null)
                return null;

            if (!currencies.ContainsKey(priceType))
                return null;

            return currencies[priceType];
        }
        
        public Currency GetCurrency(string tokenType)
        {
            if (currencies == null)
                return null;

            return currencies.Where(x => x.Value.TokenType().CompareTo(tokenType) == 0).FirstOrDefault().Value;
        }

        public List<TcpMsg.CurrencyInfo> ToCurrencyInfoList()
        {
            var currencyInfos = new List<TcpMsg.CurrencyInfo>();

            currencies.ForEach(x => currencyInfos.Add(new TcpMsg.CurrencyInfo()
            {
                priceType = (int)x.Value.PriceType(),
                priceValue = x.Value.PriceValue()
            }));

            return currencyInfos;
        }

        public void Currency_SetPrice(string tokenType, Int32 amount)
        {
            var currency = GetCurrency(tokenType);
            if (null != currency)
            {
                currency.SetPrice(tokenType, amount);
            }
            else
            {
                logger.Error($"Currency is null UserID={UID} tokenType={tokenType}");
            }
        }

        public (TcpMsg.Error, Currency) CheckFungibleToken(string tokenType, Int32 amount)
        {
            var result = TcpMsg.Error.None;
            var currency = GetCurrency(tokenType);

            if (null == currency)
                return (TcpMsg.Error.NotFoundTokenInfo, null);

            if (currency.PriceValue() < amount)
                return (TcpMsg.Error.Lack_the_currency, null);

            return (result, currency);
        }

        public void WithdrawFungible(string tokenType, Int32 amount)
        {
            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_UserId_Fungible_Transfer()
            {
                uid = UID,
                guid = Id.ToString(),
                fromUserId = lineUID,
                toAddr = Setting.ProgramSetting.Instance.lbdInfo.operatorAddr,
                toUserId = string.Empty,
                tokenType = tokenType,
                amount =amount
            });
        }

        public TcpMsg.Error OperatorFungibleMint(Resource.PriceType priceType, Int32 amount)
        {
            var result = TcpMsg.Error.None;
            var tokenTypeInfo = LBD.LBDApiManager.Instance.GetFungibleTypeToMeta(((Int32)priceType).ToString());

            if (null == tokenTypeInfo)
                return TcpMsg.Error.NotFoundTokenType;

            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_MintFungible()
            {
                uid = UID,
                guid = Id.ToString(),
                toAddr = string.Empty,
                toUserId = lineUID,
                tokenType = tokenTypeInfo.tokenType,
                amount = amount
            });

            /* TODO : belldan
             * Operator Addr 에 Mint 후 User에게 지급 할때 사용
            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_Wallet_Fungible_Transfer()
            {
                uid = UID,
                guid = Id.ToString(),
                fromAddr = Setting.ProgramSetting.Instance.lbdInfo.operatorAddr,
                fromSecret = Setting.ProgramSetting.Instance.lbdInfo.secretKey,
                toAddr = string.Empty,
                toUserId = lineUID,
                tokenType = tokenTypeInfo.tokenType,
                amount = amount
            });
            */

            return result;
        }

        public (TcpMsg.Error, LBD.ServiceTokenInfo) CheckServiceToken(Int32 amount, string symbol = "")
        {
            var result = TcpMsg.Error.None;

            LBD.ServiceTokenInfo serviceToken = null;

            if (string.IsNullOrEmpty(symbol))
            {
                serviceToken = serviceTokens.Find(x => x.contractId == Setting.ProgramSetting.Instance.lbdInfo.serviceToeknContractId);
            }
            else
            {
                serviceToken = serviceTokens.Find(x => x.symbol == symbol);
            }

            if (null == serviceToken)
                return (TcpMsg.Error.NotFoundTokenInfo, null);

            if (serviceToken.amount < amount)
                return (TcpMsg.Error.Lack_the_currency, null);

            return (result, serviceToken);
        }

        public TcpMsg.Error WithdrawServiceToken(Int32 amount)
        {
            var result = TcpMsg.Error.None;
            var tokenTypeInfo = LBD.LBDApiManager.Instance.GetServiceTokenType(Setting.ProgramSetting.Instance.lbdInfo.serviceToeknContractId);

            if (null == tokenTypeInfo)
                return TcpMsg.Error.NotFoundTokenType;

            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_IssueServiceTokenTransfer()
            {
                uid = UID,
                guid = Id.ToString(),
                contractId = tokenTypeInfo.contractId,
                fromUserId = lineUID,
                toAddr = Setting.ProgramSetting.Instance.lbdInfo.operatorAddr,
                toUserId = string.Empty,
                amount = amount,
                landingUri = string.Empty
            });

            return result;
        }

        public TcpMsg.Error OperatorServiceTokenMint(Int32 amount)
        {
            var result = TcpMsg.Error.None;
            var tokenTypeInfo = LBD.LBDApiManager.Instance.GetServiceTokenType(Setting.ProgramSetting.Instance.lbdInfo.serviceToeknContractId);

            if (null == tokenTypeInfo)
                return TcpMsg.Error.NotFoundTokenType;

            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_TransferServiceToken()
            {
                uid = UID,
                guid = Id.ToString(),
                contractId = tokenTypeInfo.contractId,
                fromAddr = Setting.ProgramSetting.Instance.lbdInfo.operatorAddr,
                fromSecret = Setting.ProgramSetting.Instance.lbdInfo.secretKey,
                toAddr = string.Empty,
                toUserId = lineUID,
                amount = amount
            });

            return result;
        }

        public void SetFungibleTokenMint(List<LBD.TokenInfo> tokenInfos)
        {
            tokenInfos.ForEach(x => logger.Debug($"Fungible TokenType={x.tokenType} Name={x.name} Meta={x.meta} Amount={x.amount}"));

            if (fungibles.Count == 0)
            {
                fungibles.AddRange(tokenInfos);
            }
            else
            {
                var newTonekInfo = new List<LBD.TokenInfo>();
                foreach (var tokeninfo in tokenInfos)
                {
                    var token = fungibles.Find(x => x.tokenType == tokeninfo.tokenType);
                    if (null == token)
                    {
                        newTonekInfo.Add(token);
                    }
                    else
                    {
                        token.amount += tokeninfo.amount;
                    }
                }

                if (newTonekInfo.Count > 0)
                    fungibles.AddRange(newTonekInfo);
            }
        }

        public void SetFungibleTokensBalances(List<LBD.FungibleTokenTypeBalancesData> tokenDatas)
        {
            tokenDatas.ForEach(x => logger.Debug($"TokenType={x.tokenType} Name={x.name} Meta={x.meta} Amount={x.amount}"));

            if (tokenDatas.Count > 0)
            {
                var tempfungibles = tokenDatas.Select(x => new LBD.TokenInfo()
                {
                    tokenType = x.tokenType,
                    tokenIdx = string.Empty,
                    name = x.name,
                    meta = x.meta,
                    amount = Convert.ToInt32(x.amount)
                }).ToList();

                foreach (var ft in tempfungibles)
                {
                    if (null != ft)
                    {
                        var ftoken = fungibles.Find(x => x.tokenType == ft.tokenType);

                        if (null == ftoken)
                        {
                            fungibles.Add(ft);
                        }
                        else
                        {
                            ftoken.amount = ft.amount;
                        }
                    }
                }
            }
            else
            {
                logger.Info($"There is no Fungible Token Uid={UID}");
            }
        }

        public void FungibleTokenTransfer(string tokenType, Int32 amount)
        {
            logger.Debug($"FungibleTokenTransfer TokenType={tokenType} Amount={amount}");

            if (amount != 0)
            {

                var tokenInfo = fungibles.Find(x => x.tokenType == tokenType);

                if (null != tokenInfo)
                {
                    tokenInfo.amount += amount;
                }
                else
                {
                    var fungibletokenType = LBD.LBDApiManager.Instance.GetFungibleTypeToTokenType(tokenType);
                    fungibles.Add(new LBD.TokenInfo()
                    {
                        tokenType = fungibletokenType.tokenType,
                        tokenIdx = "00000000",
                        name = fungibletokenType.name,
                        meta = fungibletokenType.meta,
                        amount = amount
                    });
                }
            }
            else
            {
                logger.Warn($"FungibleTokenTransfer Amount == 0");
            }
        }

        public void SetServiceTokensBalances(List<LBD.ServiceTokenData> serviceTokenDatas)
        {
            serviceTokenDatas.ForEach(x => logger.Debug($"ServiceToken ContractId={x.contractId} Amount={x.amount}"));

            if (serviceTokenDatas.Count > 0)
            {
                var tokenInfo = serviceTokenDatas.Select(x => new LBD.ServiceTokenInfo()
                {
                    contractId = x.contractId,
                    name = x.name,
                    symbol = x.symbol,
                    amount = x.amount
                }).ToList();

                foreach (var st in tokenInfo)
                {
                    if (null != st)
                    {
                        var stoken = serviceTokens.Find(x => x.contractId == st.contractId);

                        if (null == stoken)
                        {
                            serviceTokens.Add(st);
                        }
                        else
                        {
                            stoken.amount = st.amount;
                        }
                    }
                }
            }
            else
            {
                logger.Info($"There is no Service Token Uid={UID}");
            }
            LoadCurrency();
        }

        public void ServiceTokenTransfer(string contractId, Int32 amount)
        {
            logger.Debug($"ServiceTokenTransfer ContractId={contractId} Amount={amount}");

            if (amount != 0)
            {
                var tokenInfo = serviceTokens.Find(x => x.contractId == contractId);

                if (null != tokenInfo)
                {
                    tokenInfo.amount += amount;
                }
                else
                {
                    var serviceTokenType = LBD.LBDApiManager.Instance.GetServiceTokenType(contractId);
                    serviceTokens.Add(new LBD.ServiceTokenInfo()
                    {
                        contractId = serviceTokenType.contractId,
                        name = serviceTokenType.name,
                        symbol = serviceTokenType.meta,
                        amount = amount
                    });
                }
            }
            else
            {
                logger.Warn($"ServiceTokenTransfer Amount == 0");
            }
        }

        public List<TcpMsg.ServiceTokenInfo> ToServiceTokenInfoList()
        {
            var serviceTokenInfos = new List<TcpMsg.ServiceTokenInfo>();
            serviceTokens.ForEach(x => serviceTokenInfos.Add(new TcpMsg.ServiceTokenInfo()
            {
                symbol = x.symbol,
                amount = x.amount
            }));

            return serviceTokenInfos;
        }
    }
}
