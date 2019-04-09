namespace Ombi.Helpers
{
    public static class OmbiRoles
    {
        // DONT FORGET TO ADD TO IDENTITYCONTROLLER.CREATEROLES AND THE UI!

        public const string Admin = nameof(Admin);
        public const string AutoApproveMovie = nameof(AutoApproveMovie);
        public const string AutoApproveTv = nameof(AutoApproveTv);
        public const string AutoApproveMusic = nameof(AutoApproveMusic);
        public const string PowerUser = nameof(PowerUser);
        public const string RequestTv = nameof(RequestTv);
        public const string RequestMovie = nameof(RequestMovie);
        public const string RequestMusic = nameof(RequestMusic);
        public const string Disabled = nameof(Disabled);
        public const string ReceivesNewsletter = nameof(ReceivesNewsletter);
        public const string ManageOwnRequests = nameof(ManageOwnRequests);
        public const string EditCustomPage = nameof(EditCustomPage);
    }
}