namespace Ombi.Helpers
{
    public static class OmbiRoles
    {
        // DONT FORGET TO ADD TO IDENTITYCONTROLLER.CREATEROLES AND THE UI!

        public const string Admin = nameof(Admin);
        public const string AutoApproveMovie = nameof(AutoApproveMovie);
        public const string AutoApproveTv = nameof(AutoApproveTv);
        public const string PowerUser = nameof(PowerUser);
        public const string RequestTv = nameof(RequestTv);
        public const string RequestMovie = nameof(RequestMovie);
        public const string Disabled = nameof(Disabled);
        public const string RecievesNewsletter = nameof(RecievesNewsletter);
    }
}