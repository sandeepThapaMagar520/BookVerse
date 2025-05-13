namespace BookLibraryStore.Utility
{
    public class MembershipHelper
    {
        public static string GenerateMembershipId(string firstName, string lastName)
        {
            // Get initials
            var firstInitial = string.IsNullOrEmpty(firstName) ? 'X' : char.ToUpper(firstName[0]);
            var lastInitial = string.IsNullOrEmpty(lastName) ? 'X' : char.ToUpper(lastName[0]);

            // Generate random 6-char alphanumeric (A-Z, 0-9)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomPart = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Combine
            return $"{firstInitial}{lastInitial}-{randomPart}";
        }
    }
}
