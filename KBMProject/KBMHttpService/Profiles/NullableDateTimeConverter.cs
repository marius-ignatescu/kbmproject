using AutoMapper;

namespace KBMHttpService.Profiles
{
    public class NullableDateTimeConverter : ITypeConverter<string?, DateTime?>
    {
        public DateTime? Convert(string? source, DateTime? destination, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            return DateTime.Parse(source);
        }
    }
}
