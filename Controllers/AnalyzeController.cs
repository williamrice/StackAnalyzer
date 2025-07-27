using Microsoft.AspNetCore.Mvc;
using StackAnalyzer.Models;
using StackAnalyzer.Services;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly IStackAnalyzerOrchestrationService _orchestrationService;
    private readonly ILogger<AnalyzeController> _logger;

    public AnalyzeController(
        IStackAnalyzerOrchestrationService orchestrationService,
        ILogger<AnalyzeController> logger)
    {
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<StackAnalysisResult>> Analyze([FromBody] StackAnalysisRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest("URL is required");
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("Invalid URL format");
        }

        try
        {
            _logger.LogInformation("Analyzing website: {Url}", request.Url);

            var result = await _orchestrationService.AnalyzeWebsiteAsync(request.Url);

            _logger.LogInformation("Analysis completed for: {Url}", request.Url);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing website: {Url}", request.Url);

            return StatusCode(500, new { error = "Analysis failed", message = ex.Message });
        }
    }
}