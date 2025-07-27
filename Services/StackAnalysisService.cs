using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using StackAnalyzer.Models;
using System.Text.Json;

namespace StackAnalyzer.Services
{


    public class StackAnalysisService : IStackAnalysisService
    {
        private readonly IChatClient _chatClient;

        public StackAnalysisService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<StackAnalysisResult> AnalyzeDomDataAsync(DomExtractionResult domData)
        {
            var prompt = BuildAnalysisPrompt(domData);

            var response = await _chatClient.GetResponseAsync(
                [new ChatMessage(ChatRole.User, prompt)]);

            return ParseAnalysisResult(response.Messages.FirstOrDefault()?.Text!, domData);
        }

        private string BuildAnalysisPrompt(DomExtractionResult domData)
        {
            return $@"Analyze this website's technology stack and return ONLY a JSON response matching this exact structure:
{{
    ""Framework"": ""detected framework or 'Unknown'"",
    ""Libraries"": [""lib1"", ""lib2""],
    ""CssFrameworks"": [""bootstrap"", ""tailwind""],
    ""BuildTools"": [""webpack"", ""vite""],
    ""HostingProvider"": ""provider name or 'Unknown'"",
    ""Analytics"": [""google analytics""],
    ""CdnServices"": [""cloudflare"", ""jsdelivr""],
    ""Confidence"": ""high/medium/low"",
    ""RawDomData"": ""key technical indicators found""
}}

Website data:
Scripts: {JsonSerializer.Serialize(domData.Scripts)}
Stylesheets: {JsonSerializer.Serialize(domData.Stylesheets)}
Meta Tags: {JsonSerializer.Serialize(domData.MetaTags)}
Headers: {JsonSerializer.Serialize(domData.Headers)}

When you are detecting a framework, library, or other technology, take into account the entire overall picture. Return ONLY the JSON, no other text.";
        }

        private StackAnalysisResult ParseAnalysisResult(string rawResponse, DomExtractionResult domData)
        {
            try
            {
                var cleanedResponse = rawResponse.Trim();
                if (cleanedResponse.StartsWith("```json"))
                    cleanedResponse = cleanedResponse.Substring(7);
                if (cleanedResponse.EndsWith("```"))
                    cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
                cleanedResponse = cleanedResponse.Trim();

                var result = JsonSerializer.Deserialize<StackAnalysisResult>(cleanedResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? CreateFallbackResult(domData);
            }
            catch
            {
                return CreateFallbackResult(domData);
            }
        }

        private StackAnalysisResult CreateFallbackResult(DomExtractionResult domData)
        {
            return new StackAnalysisResult(
                Framework: "Analysis Failed",
                Libraries: new List<string>(),
                CssFrameworks: new List<string>(),
                BuildTools: new List<string>(),
                HostingProvider: "Unknown",
                Analytics: new List<string>(),
                CdnServices: new List<string>(),
                Confidence: "low",
                RawDomData: $"Scripts: {domData.Scripts.Length}, Stylesheets: {domData.Stylesheets.Length}"
            );
        }
    }
}