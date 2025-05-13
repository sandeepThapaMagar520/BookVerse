namespace BookLibraryStore.Utility
{
    public class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Staff = "Staff";

        public const string FormatPaperback = "Paperback";
        public const string FormatHardcover = "Hardcover";
        public const string FormateBook = "eBook";

        public const string EditionTypeSigned = "Signed";
        public const string EditionLimited = "Limited";
        public const string EditionFirst = "First";
        public const string EditionCollector = "Collector";

        public const string GenreFiction = "Fiction";
        public const string GenreNonFiction = "Non-Fiction";
        public const string GenreSciFi = "Science Fiction";
        public const string GenreFantasy = "Fantasy";
        public const string GenreMystery = "Mystery";
        public const string GenreRomance = "Romance";
        public const string GenreBiography = "Biography";
        public const string GenreHistory = "History";
        public const string GenreSelfHelp = "Self-Help";
        public const string GenreChildren = "Children";
        public const string GenreThriller = "Thriller";
        public const string GenreHorror = "Horror";
        public const string GenrePoetry = "Poetry";
        public const string GenreGraphicNovel = "Graphic Novel";
        public const string GenreTravel = "Travel";

        // Helper list to loop through genres
        public static List<string> GenreList = new List<string>
        {
            GenreFiction,
            GenreNonFiction,
            GenreSciFi,
            GenreFantasy,
            GenreMystery,
            GenreRomance,
            GenreBiography,
            GenreHistory,
            GenreSelfHelp,
            GenreChildren,
            GenreThriller,
            GenreHorror,
            GenrePoetry,
            GenreGraphicNovel,
            GenreTravel
        };

        // Publishers
        public const string PublisherPenguin = "Penguin Random House";
        public const string PublisherHarperCollins = "HarperCollins";
        public const string PublisherSimonSchuster = "Simon & Schuster";
        public const string PublisherHachette = "Hachette Book Group";
        public const string PublisherMacmillan = "Macmillan Publishers";
        public const string PublisherScholastic = "Scholastic";
        public const string PublisherOxford = "Oxford University Press";
        public const string PublisherCambridge = "Cambridge University Press";
        public const string PublisherPearson = "Pearson Education";
        public const string PublisherBloomsbury = "Bloomsbury Publishing";

        // Helper list to loop through publishers
        public static List<string> PublisherList = new List<string>
        {
            PublisherPenguin,
            PublisherHarperCollins,
            PublisherSimonSchuster,
            PublisherHachette,
            PublisherMacmillan,
            PublisherScholastic,
            PublisherOxford,
            PublisherCambridge,
            PublisherPearson,
            PublisherBloomsbury
        };

        public const string OrderPending = "Pending";
        public const string OrderCancelled = "Cancelled";
        public const string OrderCompleted = "Completed";

        public const string AnnouncementGeneral = "General";
        public const string AnnouncementSale = "Sale";
        public const string AnnouncementNewArrival = "NewArrival";
        public const string AnnouncementEvent = "Event";
        public const string AnnouncementImportantInfo = "ImportantInfo";

        public static List<string> AnnouncementType = new List<string>
        {
            AnnouncementGeneral,
            AnnouncementSale,
            AnnouncementNewArrival,
            AnnouncementEvent,
            AnnouncementImportantInfo
        };
    }
}
