using System.Text.Json;
using Microsoft.Playwright;
using StackAnalyzer.Models;

namespace StackAnalyzer.Services
{
    public class DomExtractionService : IDomExtractionService, IAsyncDisposable
    {
        private readonly IPlaywright _playwright;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DomExtractionService> _logger;
        private IBrowser? _browser;

        public DomExtractionService(IConfiguration configuration, ILogger<DomExtractionService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Set environment variable for Playwright server if configured
            var playwrightUrl = _configuration["PlaywrightUrl"];
            if (!string.IsNullOrEmpty(playwrightUrl))
            {
                var wsEndpoint = playwrightUrl.Replace("http://", "ws://");
                Environment.SetEnvironmentVariable("PLAYWRIGHT_WS_ENDPOINT", wsEndpoint);
                _logger.LogInformation("Set PLAYWRIGHT_WS_ENDPOINT to {Endpoint}", wsEndpoint);
            }

            _playwright = Playwright.CreateAsync().Result;
        }

        public async Task<DomExtractionResult> ExtractDomDataAsync(string url)
        {
            if (_browser == null)
            {
                try
                {
                    _logger.LogInformation("Launching Chromium browser");

                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to launch browser");
                    throw new InvalidOperationException($"Cannot launch browser. Error: {ex.Message}", ex);
                }
            }

            var page = await _browser.NewPageAsync();

            try
            {
                _logger.LogInformation("Navigating to {Url}", url);

                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000
                });

                var html = await page.ContentAsync();
                var scripts = await page.EvaluateAsync<string[]>("() => Array.from(document.scripts).map(s => s.src || s.textContent?.substring(0, 100) || '').filter(s => s.length > 0)");
                var stylesheets = await page.EvaluateAsync<string[]>("() => Array.from(document.querySelectorAll('link[rel=stylesheet]')).map(l => l.href)");
                var metaTags = await page.EvaluateAsync<object[]>("() => Array.from(document.querySelectorAll('meta')).map(m => ({name: m.name || m.property, content: m.content}))");
                var headers = await page.EvaluateAsync<string[]>("() => Array.from(document.querySelectorAll('h1, h2, h3, h4, h5, h6')).map(h => h.textContent?.trim()).filter(h => h && h.length > 0)");
                var cookieNames = await page.EvaluateAsync<string[]>("() => document.cookie.split(';').map(c => c.split('=')[0].trim()).filter(c => c.length > 0)");

                _logger.LogInformation("Successfully extracted DOM data from {Url}", url);

                return new DomExtractionResult(html, scripts, stylesheets, metaTags, headers, cookieNames);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
                await _browser.CloseAsync();
            _playwright?.Dispose();
        }
    }
}