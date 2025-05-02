namespace BusinessLogic.Authorization;

public enum Permissions
{
    //User profile
    ViewProfileInfo = 0,
    EditProfileInfo,
    ChangePassword,

    //Advertisement bookmarks
    ViewAdvertisementBookmarks,
    BookmarkAdvertisement,
    EditAdvertisementBookmark,
    
    //Advertisement
    CreateAdvertisement,
    ViewOwnedAdvertisements,
    EditOwnedAdvertisement,
    DeleteOwnedAdvertisement,

    //Messages
    ViewMessages,
    SendMessage,

    //Advertisement notification subscriptions
    ViewAdvertisementNotificationSubscriptions,
    CreateAdvertisementNotificationSubscription,
    EditAdvertisementNotificationSubscriptions,
    DeleteAdvertisementNotificationSubscriptions,

    //Payment
    ViewPayments,
    MakePayment,

    //Manage all users
    ViewUsers
}
