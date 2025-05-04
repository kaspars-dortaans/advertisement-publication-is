export enum Permissions {
  //Manage own profile
  ViewOwnProfileInfo = 0,
  EditOwnProfileInfo,
  ChangeOwnPassword,
  //Manage own advertisement bookmarks
  ViewAdvertisementBookmarks,
  BookmarkAdvertisement,
  EditAdvertisementBookmark,
  //Manage own advertisements
  CreateOwnedAdvertisement,
  ViewOwnedAdvertisements,
  EditOwnedAdvertisement,
  DeleteOwnedAdvertisement,
  //Messages
  ViewMessages,
  SendMessage,
  //Manage own advertisement notification subscriptions
  ViewOwnedAdvertisementNotificationSubscriptions,
  CreateOwnedAdvertisementNotificationSubscription,
  EditOwnedAdvertisementNotificationSubscriptions,
  DeleteOwnedAdvertisementNotificationSubscriptions,
  //Payment
  ViewOwnPayments,
  MakePayment,
  //Manage categories
  CreateCategory,
  EditCategory,
  DeleteCategory,
  //Manage attributes
  ViewAllAttributes,
  CreateAttribute,
  EditAttribute,
  DeleteAttribute,
  //Manage users
  ViewAllUsers,
  CreateUser,
  EditAnyUser,
  DeleteAnyUser,
  //Manage roles
  ViewAllRoles,
  AddRole,
  EditRole,
  DeleteRole,
  //Manage permissions
  ViewAllPermissions,
  AddPermission,
  EditPermission,
  DeletePermission,
  //Manage advertisements
  CreateAdvertisement,
  ViewAllAdvertisements,
  EditAnyAdvertisement,
  DeleteAnyAdvertisement,
  //Manage advertisement notification subscriptions
  CreateAdvertisementNotificationSubscription,
  ViewAllAdvertisementNotificationSubscriptions,
  EditAnyAdvertisementNotificationSubscription,
  DeleteAnyAdvertisementNotificationSubscription,
  //Rule violation reports
  ViewRuleViolationReports,
  ResolveRuleViolationReport,
}