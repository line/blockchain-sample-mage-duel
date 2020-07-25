
namespace TCGGameService
{
    public enum UserStateType
    {
        None = 0,
        Login,
        RequestProxy,
        NewUserConnect,
        ExistingUserConnect,
        DefaultCardCreate,
        CreateNickName,

        InitGetFungibleToken,
        InitGetNonFungibleTokenType,

        InitGetDeck,
        AuthSucess,


        DeckCardAdd,
        DeckCardRemove,

        BuyNonFungible,
        BuyFungible,
        BuyShopSlot,
        RefreshShopCardSlot,
    }
}
