namespace ThreeChartsAPI.Models.Dtos
{
    public class AuthorizationUrlDto
    {
        public string Url { get; }

        public AuthorizationUrlDto(string url)
        {
            Url = url;
        }
    }
}