namespace BookAtrium.PluginContracts;

[Flags]
public enum MetadataSourceCapabilities
{
    None = 0,
    SearchByTitleAuthor = 1 << 0,
    SearchByIsbn = 1 << 1,
    SearchByExternalIdentifier = 1 << 2,
    CoverSearch = 1 << 3,
    MetadataEnrichment = 1 << 4,
    MultipleLanguages = 1 << 5,
    SeriesMetadata = 1 << 6,
    Description = 1 << 7,
    Publisher = 1 << 8,
    PublicationDate = 1 << 9,
    TagsSubjects = 1 << 10,
    Ratings = 1 << 11,
    Identifiers = 1 << 12
}
