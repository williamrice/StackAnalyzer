using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackAnalyzer.Models;

namespace StackAnalyzer.Services
{
    public class StackAnalyzerOrchestrationService : IStackAnalyzerOrchestrationService
    {
        private readonly IDomExtractionService _domExtractionService;
        private readonly IStackAnalysisService _stackAnalysisService;

        public StackAnalyzerOrchestrationService(
            IDomExtractionService domExtractionService,
            IStackAnalysisService stackAnalysisService)
        {
            _domExtractionService = domExtractionService;
            _stackAnalysisService = stackAnalysisService;
        }

        public async Task<StackAnalysisResult> AnalyzeWebsiteAsync(string url)
        {
            var domData = await _domExtractionService.ExtractDomDataAsync(url);
            var analysis = await _stackAnalysisService.AnalyzeDomDataAsync(domData);

            return analysis;
        }
    }
}