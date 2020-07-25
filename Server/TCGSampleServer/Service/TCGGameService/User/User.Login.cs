using System;
using System.Collections.Generic;

namespace TCGGameService
{
    public partial class User
    {
        public void Login(InternalMsg.IntlMsg_GetProfile profile)
        {
            logger.Debug($"Profile UserId={profile.userId} displayName={profile.displayName}");

            displayName = profile.displayName;
            pictureUrl = profile.pictureUrl;
            statusMessage = profile.statusMessage;

            if (stateType == UserStateType.None)
            {
                var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();

                tblUser = repoUser.GetUserFromLineUserId(profile.userId);
                var IsCreate = false;
                if (null == tblUser)
                {
                    Table.TblUser newUser = new Table.TblUser()
                    {
                        lineuserid = profile.userId,
                        nickname = string.Empty,
                        address = string.Empty,
                        regDate = DateTime.Now
                    };

                    newUser.uuid = repoUser.Insert(newUser);
                    tblUser = newUser;
                    IsCreate = true;
                }

                if (IsCreate)
                {
                    stateType = UserStateType.NewUserConnect;

                    LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_MultiMintNonFungible()
                    {
                        uid = UID,
                        guid = Id.ToString(),
                        toAddr = string.Empty,
                        toUserId = lineUID,
                        mints = DefaultCardToTokenTypeInfo()
                    });

                    IsCreate = false;
                    logger.Debug($"UserUID={UID} UserStateType={stateType.ToString()}");
                }
                else
                {
                    if (string.IsNullOrEmpty(tblUser.nickname))
                    {
                        stateType = UserStateType.CreateNickName;

                        var ackData = new TcpMsg.AckVerifyAccessToken();
                        ackData.authType = TcpMsg.AuthType.CreateNickname;

                        Send(new Packet(ackData));
                    }
                    else
                    {
                        stateType = UserStateType.ExistingUserConnect;
                        GetWalletAddress();
                    }                    
                }
                
                logger.Debug($"UserUID={UID} UserStateType={stateType.ToString()}");
            }
        }

        public void GetWalletAddress()
        {
            LBD.LBDApiManager.Instance.AddLBDCmd(new LBD.Msg.LBDMsg_GetUser()
            {
                uid = UID,
                guid = Id.ToString(),
                userid = lineUID
            });
        }

        public void CreateNickName(string nickname)
        {
            if (stateType == UserStateType.CreateNickName)
            {
                var repoUser = TCGGameSrv.IocContainer.Resolve<Repository.IUser>();

                logger.Debug($"UserID={UID}, CreateNickName={nickname}");

                var ackData = new TcpMsg.AckCreateNickName();

                if (string.IsNullOrEmpty(nickname))
                {
                    ackData.errCode = TcpMsg.Error.NickNameNullOrEmpty;
                }
                else
                {
                    var isNick = repoUser.ExistsUserNickName(nickname);

                    if (isNick)
                    {
                        ackData.errCode = TcpMsg.Error.NickName_Already_Exists;
                    }
                    else
                    {
                        var tblUser = repoUser.GetUserFromUid(UID);
                        tblUser.nickname = nickname;
                        repoUser.Update(tblUser);

                        GetWalletAddress();
                        logger.Info($"UserID={UID}, CreateNickName={nickName} Success");

                        return;
                    }
                }

                if (ackData.errCode != TcpMsg.Error.None)
                {
                    logger.Warn($"UID={UID} Error Code={ackData.errCode.ToString()}");
                }

                Send(new Packet(ackData));
                stateType = UserStateType.None;
            }
        }

        public void UserData()
        {
            var ackData = new TcpMsg.AckUserData();

            ackData.userInfoFullData = new TcpMsg.UserInfoFullData()
            {
                owner = new TcpMsg.UserInfo()
                {
                    uuid = UID,
                    nickName = tblUser.nickname
                },
                cardList = NonFungiblesToCardInfo(),
                deckList = null != deckInfos ? deckInfos : new List<TcpMsg.DeckInfo>(),
                currencyInfos = ToCurrencyInfoList(),
                serviceTokenInfos = ToServiceTokenInfoList(),
                tradeGoodsInfos = GetTradeGoods()
            };

            logger.Info($"UserID={UID}, NickName={nickName} Success");

            Send(new Packet(ackData));
            stateType = UserStateType.None;
        }

        public void LobbyEntry()
        {
            // Enter the lobby
            // What to do after entering here
            var ackLobbyEntry = new TcpMsg.AckLobbyEntry();

            Send(new Packet(ackLobbyEntry));
        }
    }
}
