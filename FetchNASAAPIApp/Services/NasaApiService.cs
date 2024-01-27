﻿using FetchNASAAPIApp.Models;
using FetchNASAAPIApp.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FetchNASAAPIApp.Services
{
    public class NasaApiService : INasaApiService
    {
        private HttpClient _httpClient;
        private const string _baseUrl = "https://api.nasa.gov/planetary/apod?";

        public NasaApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Picture>> FetchData(NasaApiRequestParams nasaApiRequestParams)
        {
            var requestParams = new List<string>();

            foreach (var prop in nasaApiRequestParams.GetType().GetProperties())
            {
                var value = prop.GetValue(nasaApiRequestParams);

                if (value is DateOnly dateValue && dateValue != DateOnly.MinValue)
                {
                    requestParams.Add($"{prop.Name.ToLower()}={dateValue:yyyy-MM-dd}");
                }
                else if (value is bool)
                {
                    requestParams.Add($"{prop.Name.ToLower()}={value.ToString().ToLower()}");
                }
                else if (value is int intValue && intValue > 0)
                {
                    requestParams.Add($"{prop.Name.ToLower()}={intValue}");
                }
            }

            requestParams.Add($"api_key={nasaApiRequestParams.ApiKey}");

            string queryString = string.Join("&", requestParams);
            string url = $"{_baseUrl}{queryString}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var nasaResponse = new List<Picture>();

                var token = JToken.Parse(responseContent);

                if (token is JArray)
                {
                    nasaResponse = JsonConvert.DeserializeObject<List<Picture>>(responseContent);
                }
                else if (token is JObject)
                {
                    nasaResponse.Add(JsonConvert.DeserializeObject<Picture>(responseContent));
                }

                return nasaResponse;
            }
            catch (HttpRequestException httpRequestException)
            {
                throw new Exception($"Error getting data from NASA API: {httpRequestException.Message}");
            }
        }
    }
}
