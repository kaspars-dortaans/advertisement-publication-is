namespace api.Constants
{
    public static class CustomErrorCodes
    {
        public const string InvalidLoginCredentials = "InvalidLoginCredentials";
        public const string UserWithSuchEmailAlreadyExist = "UserWithSuchEmailAlreadyExist";
        public const string MissingRequired = "Required";
        public const string NotAnEmail = "NotAnEmail";
        public const string NotAPhoneNumber = "NotAPhoneNumber";
        public const string PropertyWasNotFound = "PropertyWasNotFound";
        public const string ComparablePropertyDidNotMatch = "ComparablePropertyDidNotMatch";
    }
}
