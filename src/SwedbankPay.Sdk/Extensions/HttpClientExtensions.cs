﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SwedbankPay.Sdk.Exceptions;

namespace SwedbankPay.Sdk.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetAsJsonAsync<T>(this HttpClient httpClient, Uri uri)
        {
            var apiResponse = await httpClient.GetAsync(uri);

            var responseString = await apiResponse.Content.ReadAsStringAsync();

            if (!apiResponse.IsSuccessStatusCode)
                throw new HttpResponseException(
                    apiResponse,
                    JsonConvert.DeserializeObject<ProblemResponse>(responseString),
                    BuildErrorMessage(responseString, uri, apiResponse));

            
            return JsonConvert.DeserializeObject<T>(responseString, JsonSerialization.JsonSerialization.Settings);
        }

        internal static async Task<T> SendAndProcessAsync<T>(this HttpClient httpClient, HttpMethod httpMethod, Uri uri, object payload)
            where T : class
        {
            using var httpRequestMessage = new HttpRequestMessage(httpMethod, uri);

            if (payload != null)
            {
                var content = JsonConvert.SerializeObject(payload, JsonSerialization.JsonSerialization.Settings);
                httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            string BuildErrorMessage(string httpResponseBody)
            {
                return $"{httpRequestMessage.Method}: {httpRequestMessage.RequestUri} failed with error code {httpResponseMessage.StatusCode} using bearer token {httpClient.DefaultRequestHeaders?.Authorization?.Parameter}. Response body: {httpResponseBody}";
            }

            string httpResponseContent = string.Empty;
            try
            {
                httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    throw new HttpResponseException(
                        httpResponseMessage,
                        JsonConvert.DeserializeObject<ProblemResponse>(httpResponseContent),
                        BuildErrorMessage(httpResponseContent));
                }

                return JsonConvert.DeserializeObject<T>(httpResponseContent, JsonSerialization.JsonSerialization.Settings);
            }
            catch (HttpResponseException ex)
            {
                ex.Data.Add(nameof(httpResponseContent), httpResponseContent);
                throw ex;
            }
        }

        private static string BuildErrorMessage(string httpResponseBody, Uri uri, HttpResponseMessage httpResponse)
        {
            return
                $"GET: {uri} failed with error code {httpResponse.StatusCode}. Response body: {httpResponseBody}";
        }

        public static Task<T> PostAsJsonAsync<T>(this HttpClient httpClient, Uri uri, object payload)
            where T : class
        {
            return httpClient.SendAndProcessAsync<T>(HttpMethod.Post, uri, payload);
        }

        public static Task<T> SendAsJsonAsync<T>(this HttpClient httpClient, HttpMethod httpMethod, Uri uri, object payload = null)
            where T: class
        {
            return httpClient.SendAndProcessAsync<T>(httpMethod, uri, payload);
        }
    }
}
