using System.Net.Http.Headers;
using System.Text.Json;

namespace SA_Builders.Services
{
    public interface ISupabaseStorageService
    {
        // Uploads to the PUBLIC bucket (project-images) and returns the
        // permanent public URL — used for portfolio photos.
        Task<string> UploadPublicAsync(string path, Stream fileStream, string contentType);

        // Uploads to the PRIVATE bucket (project-documents). Returns only
        // the storage path, NOT a URL — private files need a freshly
        // signed URL generated on each request, not a permanent one.
        Task<string> UploadPrivateAsync(string path, Stream fileStream, string contentType);

        // Generates a time-limited signed URL for a private file.
        Task<string> GetSignedUrlAsync(string path, int expiresInSeconds = 600);

        Task DeleteImageAsync(string path);
        Task DeleteDocumentAsync(string path);
    }

    public class SupabaseStorageService : ISupabaseStorageService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _imagesBucket;
        private readonly string _documentsBucket;

        public SupabaseStorageService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = (config["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url not configured")).TrimEnd('/');
            _imagesBucket = config["Supabase:ImagesBucket"] ?? "project-images";
            _documentsBucket = config["Supabase:DocumentsBucket"] ?? "project-documents";

            var serviceRoleKey = config["Supabase:ServiceRoleKey"]
                ?? throw new InvalidOperationException("Supabase:ServiceRoleKey not configured");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
            _http.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        }

        public async Task<string> UploadPublicAsync(string path, Stream fileStream, string contentType)
        {
            await UploadToBucketAsync(_imagesBucket, path, fileStream, contentType);
            return $"{_baseUrl}/storage/v1/object/public/{_imagesBucket}/{path}";
        }

        public async Task<string> UploadPrivateAsync(string path, Stream fileStream, string contentType)
        {
            await UploadToBucketAsync(_documentsBucket, path, fileStream, contentType);
            return path; // caller stores this path, not a URL
        }

        private async Task UploadToBucketAsync(string bucket, string path, Stream fileStream, string contentType)
        {
            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await _http.PostAsync(
                $"{_baseUrl}/storage/v1/object/{bucket}/{path}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Supabase upload failed ({response.StatusCode}): {error}");
            }
        }

        public async Task<string> GetSignedUrlAsync(string path, int expiresInSeconds = 600)
        {
            var body = JsonSerializer.Serialize(new { expiresIn = expiresInSeconds });
            using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(
                $"{_baseUrl}/storage/v1/object/sign/{_documentsBucket}/{path}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to generate signed URL ({response.StatusCode}): {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var signedPath = doc.RootElement.GetProperty("signedURL").GetString();

            return $"{_baseUrl}/storage/v1{signedPath}";
        }

        public async Task DeleteImageAsync(string path)
        {
            await _http.DeleteAsync($"{_baseUrl}/storage/v1/object/{_imagesBucket}/{path}");
        }

        public async Task DeleteDocumentAsync(string path)
        {
            await _http.DeleteAsync($"{_baseUrl}/storage/v1/object/{_documentsBucket}/{path}");
        }
    }
}