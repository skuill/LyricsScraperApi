namespace LyricsScraperApi.Models.Errors
{
    public class ValidationErrorResponseDto
    {
        public string Title { get; set; }
        public int Status { get; set; }
        public List<FieldErrorDto> Errors { get; set; } = new();
    }

    public class FieldErrorDto
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}
