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

namespace TcpMsg
{
    public enum AuthType
    {
        None,

        CreateNickname,
        AuthSucess,
        AuthFail,

        IsNot_LBW_Member,
        IsNot_ProxyApproved
    }

    public enum GetComposableType
    {
        None,
        GetTokenChildren,
        GetTokenParent,
        GetTokenRoot
    }

    public enum TradingSortObject
    {
        None,
        Name,
        CardType,
        Index,
        Grade,
        RegDate,
        SalePrice
    }

    public enum SortingType
    {
        None,
        Asc,
        Desc
    }

    public enum TradeStatus
    {
        None,
        Onsale,
        Trading,
        Soldout,
        Reject,
        Cancel,
        Expiration
    }

    public enum TradeType
    {
        None,
        Saleing,
        Buying,
        Reject,
        Expiration
    }

    public enum TradeTargetType
    {
        None,
        Seller,
        Buyer
    }

}
