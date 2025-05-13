namespace BookLibraryStore.Utility
{
    public static class ClaimCodeGenerator
    {
        public static string GenerateClaimCode()
        {
            return "ORD" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
    }
}
