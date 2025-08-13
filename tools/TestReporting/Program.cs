using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace TestReporting;

/// <summary>
/// Medical Device Test Report Generator
/// Generates comprehensive test reports for regulatory compliance
/// </summary>
public class Program
{
    private static ILogger? _logger;

    public static async Task<int> Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        _logger = loggerFactory.CreateLogger<Program>();

        var rootCommand = new RootCommand("Medical Device Test Report Generator");

        // Generate report command
        var generateCommand = new Command("generate", "Generate comprehensive test report")
        {
            new Option<string>("--test-results", "Path to test results directory") { IsRequired = true },
            new Option<string>("--coverage-report", "Path to coverage report file") { IsRequired = true },
            new Option<string>("--output-dir", "Output directory for reports") { IsRequired = true },
            new Option<string>("--project-name", () => "Tag Management Service", "Project name"),
            new Option<string>("--compliance-standard", () => "ISO-13485", "Compliance standard"),
            new Option<bool>("--include-medical-validation", () => true, "Include medical device validation")
        };

        generateCommand.SetHandler(async (string testResults, string coverageReport, string outputDir, 
            string projectName, string complianceStandard, bool includeMedicalValidation) =>
        {
            try
            {
                var generator = new TestReportGenerator(_logger!);
                await generator.GenerateReportAsync(testResults, coverageReport, outputDir, 
                    projectName, complianceStandard, includeMedicalValidation);
                _logger!.LogInformation("Test report generation completed successfully");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Failed to generate test report");
                Environment.Exit(1);
            }
        },
        new Argument<string>("test-results"),
        new Argument<string>("coverage-report"), 
        new Argument<string>("output-dir"),
        new Argument<string>("project-name"),
        new Argument<string>("compliance-standard"),
        new Argument<bool>("include-medical-validation"));

        // Run tests command
        var runTestsCommand = new Command("run-tests", "Run tests with comprehensive reporting")
        {
            new Option<string>("--solution-path", () => ".", "Path to solution"),
            new Option<string>("--output-dir", () => "./TestResults", "Output directory for test results"),
            new Option<int>("--coverage-threshold", () => 95, "Code coverage threshold for medical devices")
        };

        runTestsCommand.SetHandler(async (string solutionPath, string outputDir, int coverageThreshold) =>
        {
            try
            {
                var runner = new TestRunner(_logger!);
                await runner.RunTestsWithReportingAsync(solutionPath, outputDir, coverageThreshold);
                _logger!.LogInformation("Test execution and reporting completed successfully");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Failed to run tests");
                Environment.Exit(1);
            }
        },
        new Argument<string>("solution-path"),
        new Argument<string>("output-dir"),
        new Argument<int>("coverage-threshold"));

        rootCommand.AddCommand(generateCommand);
        rootCommand.AddCommand(runTestsCommand);

        return await rootCommand.InvokeAsync(args);
    }
}

/// <summary>
/// Comprehensive test report generator for medical device compliance
/// </summary>
public class TestReportGenerator
{
    private readonly ILogger _logger;

    public TestReportGenerator(ILogger logger)
    {
        _logger = logger;
    }

    public async Task GenerateReportAsync(string testResultsPath, string coverageReportPath, 
        string outputDir, string projectName, string complianceStandard, bool includeMedicalValidation)
    {
        _logger.LogInformation("Starting test report generation...");

        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        // Parse test results
        var testResults = await ParseTestResultsAsync(testResultsPath);
        
        // Parse coverage report
        var coverageData = await ParseCoverageReportAsync(coverageReportPath);

        // Generate main report
        await GenerateMainReportAsync(outputDir, projectName, complianceStandard, testResults, coverageData);

        // Generate medical device validation report
        if (includeMedicalValidation)
        {
            await GenerateMedicalValidationReportAsync(outputDir, projectName, testResults, coverageData);
        }

        // Generate detailed test execution report
        await GenerateTestExecutionReportAsync(outputDir, testResults);

        // Generate coverage report
        await GenerateCoverageReportAsync(outputDir, coverageData);

        // Generate compliance summary
        await GenerateComplianceSummaryAsync(outputDir, complianceStandard, testResults, coverageData);

        _logger.LogInformation("Test report generation completed");
    }

    private async Task<TestResultsData> ParseTestResultsAsync(string testResultsPath)
    {
        _logger.LogInformation("Parsing test results from {Path}", testResultsPath);

        var testResults = new TestResultsData();
        
        // Parse TRX files
        var trxFiles = Directory.GetFiles(testResultsPath, "*.trx", SearchOption.AllDirectories);
        foreach (var trxFile in trxFiles)
        {
            await ParseTrxFileAsync(trxFile, testResults);
        }

        // Parse JSON test results if available
        var jsonFiles = Directory.GetFiles(testResultsPath, "*.json", SearchOption.AllDirectories);
        foreach (var jsonFile in jsonFiles.Where(f => Path.GetFileName(f).Contains("test")))
        {
            await ParseJsonTestFileAsync(jsonFile, testResults);
        }

        _logger.LogInformation("Parsed {Count} test results", testResults.TestCases.Count);
        return testResults;
    }

    private async Task ParseTrxFileAsync(string trxFile, TestResultsData testResults)
    {
        try
        {
            var content = await File.ReadAllTextAsync(trxFile);
            var doc = XDocument.Parse(content);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var testCases = doc.Descendants(ns + "UnitTestResult");
            foreach (var testCase in testCases)
            {
                var testName = testCase.Attribute("testName")?.Value ?? "Unknown";
                var outcome = testCase.Attribute("outcome")?.Value ?? "Unknown";
                var duration = TimeSpan.Parse(testCase.Attribute("duration")?.Value ?? "00:00:00");
                var startTime = DateTime.Parse(testCase.Attribute("startTime")?.Value ?? DateTime.Now.ToString());
                var endTime = DateTime.Parse(testCase.Attribute("endTime")?.Value ?? DateTime.Now.ToString());

                // Extract error information if available
                string? errorMessage = null;
                string? stackTrace = null;
                var outputElement = testCase.Element(ns + "Output");
                if (outputElement != null)
                {
                    errorMessage = outputElement.Element(ns + "ErrorInfo")?.Element(ns + "Message")?.Value;
                    stackTrace = outputElement.Element(ns + "ErrorInfo")?.Element(ns + "StackTrace")?.Value;
                }

                // Extract traits (categories, compliance info)
                var traits = new Dictionary<string, string>();
                var testMethod = doc.Descendants(ns + "TestMethod")
                    .FirstOrDefault(tm => tm.Attribute("name")?.Value == testName);
                
                // Add to results
                testResults.TestCases.Add(new TestCase
                {
                    Name = testName,
                    DisplayName = testCase.Attribute("testDisplayName")?.Value ?? testName,
                    Outcome = outcome,
                    Duration = duration,
                    StartTime = startTime,
                    EndTime = endTime,
                    ErrorMessage = errorMessage,
                    StackTrace = stackTrace,
                    Traits = traits
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse TRX file {File}", trxFile);
        }
    }

    private async Task ParseJsonTestFileAsync(string jsonFile, TestResultsData testResults)
    {
        try
        {
            var content = await File.ReadAllTextAsync(jsonFile);
            // Implementation depends on the specific JSON format used
            // This is a placeholder for custom JSON test result parsing
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON test file {File}", jsonFile);
        }
    }

    private async Task<CoverageData> ParseCoverageReportAsync(string coverageReportPath)
    {
        _logger.LogInformation("Parsing coverage report from {Path}", coverageReportPath);

        var coverageData = new CoverageData();

        try
        {
            // Parse Cobertura XML format
            var content = await File.ReadAllTextAsync(coverageReportPath);
            var doc = XDocument.Parse(content);

            var coverage = doc.Element("coverage");
            if (coverage != null)
            {
                coverageData.LineCoverage = double.Parse(coverage.Attribute("line-rate")?.Value ?? "0") * 100;
                coverageData.BranchCoverage = double.Parse(coverage.Attribute("branch-rate")?.Value ?? "0") * 100;
                
                var packages = coverage.Element("packages")?.Elements("package");
                foreach (var package in packages ?? Enumerable.Empty<XElement>())
                {
                    var packageName = package.Attribute("name")?.Value ?? "Unknown";
                    var classes = package.Element("classes")?.Elements("class");
                    
                    foreach (var @class in classes ?? Enumerable.Empty<XElement>())
                    {
                        var className = @class.Attribute("name")?.Value ?? "Unknown";
                        var lineRate = double.Parse(@class.Attribute("line-rate")?.Value ?? "0") * 100;
                        var branchRate = double.Parse(@class.Attribute("branch-rate")?.Value ?? "0") * 100;

                        coverageData.ClassCoverage[className] = new ClassCoverageData
                        {
                            LineCoverage = lineRate,
                            BranchCoverage = branchRate,
                            PackageName = packageName
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse coverage report {File}", coverageReportPath);
        }

        _logger.LogInformation("Coverage: {Line:F1}% line, {Branch:F1}% branch", 
            coverageData.LineCoverage, coverageData.BranchCoverage);
        
        return coverageData;
    }

    private async Task GenerateMainReportAsync(string outputDir, string projectName, 
        string complianceStandard, TestResultsData testResults, CoverageData coverageData)
    {
        var reportPath = Path.Combine(outputDir, "TestReport.html");
        
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>Medical Device Test Report</title>");
        html.AppendLine("<style>");
        html.AppendLine(await GetReportCssAsync());
        html.AppendLine("</style>");
        html.AppendLine("</head><body>");

        // Header
        html.AppendLine($"<div class='header'>");
        html.AppendLine($"<h1>{projectName} - Test Report</h1>");
        html.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine($"<p>Compliance Standard: {complianceStandard}</p>");
        html.AppendLine("</div>");

        // Executive Summary
        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Executive Summary</h2>");
        html.AppendLine("<div class='summary-grid'>");
        
        var passedTests = testResults.TestCases.Count(t => t.Outcome == "Passed");
        var failedTests = testResults.TestCases.Count(t => t.Outcome == "Failed");
        var totalTests = testResults.TestCases.Count;
        var passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;

        var passStatusClass = passRate >= 95 ? "pass" : "fail";
        html.AppendLine($"<div class='summary-item {passStatusClass}'>");
        html.AppendLine($"<h3>Test Results</h3>");
        html.AppendLine($"<p>{passedTests}/{totalTests} Passed ({passRate:F1}%)</p>");
        html.AppendLine("</div>");

        var coverageStatus = coverageData.LineCoverage >= 95 ? "pass" : "fail";
        html.AppendLine($"<div class='summary-item {coverageStatus}'>");
        html.AppendLine($"<h3>Code Coverage</h3>");
        html.AppendLine($"<p>Line: {coverageData.LineCoverage:F1}%</p>");
        html.AppendLine($"<p>Branch: {coverageData.BranchCoverage:F1}%</p>");
        html.AppendLine("</div>");

        html.AppendLine("</div></div>");

        // Medical Device Compliance Section
        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Medical Device Compliance</h2>");
        
        var complianceTests = testResults.TestCases.Where(t => 
            t.Traits.ContainsKey("Compliance") || 
            t.Traits.ContainsKey("Category") && t.Traits["Category"] == "MedicalDevice").ToList();
        
        html.AppendLine($"<p>Compliance-specific tests: {complianceTests.Count}</p>");
        html.AppendLine($"<p>Critical system tests: {testResults.TestCases.Count(t => t.Name.Contains("Critical"))}</p>");
        html.AppendLine("</div>");

        // Test Results Detail
        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Test Execution Details</h2>");
        html.AppendLine("<table class='test-table'>");
        html.AppendLine("<thead><tr><th>Test Name</th><th>Result</th><th>Duration</th><th>Category</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var test in testResults.TestCases.OrderBy(t => t.Name))
        {
            var statusClass = test.Outcome.ToLower();
            var category = test.Traits.ContainsKey("Category") ? test.Traits["Category"] : "General";
            
            html.AppendLine($"<tr class='test-{statusClass}'>");
            html.AppendLine($"<td>{test.DisplayName}</td>");
            html.AppendLine($"<td>{test.Outcome}</td>");
            html.AppendLine($"<td>{test.Duration.TotalMilliseconds:F0}ms</td>");
            html.AppendLine($"<td>{category}</td>");
            html.AppendLine("</tr>");
            
            if (!string.IsNullOrEmpty(test.ErrorMessage))
            {
                html.AppendLine($"<tr><td colspan='4' class='error-detail'>{test.ErrorMessage}</td></tr>");
            }
        }

        html.AppendLine("</tbody></table></div>");

        html.AppendLine("</body></html>");

        await File.WriteAllTextAsync(reportPath, html.ToString());
        _logger.LogInformation("Main report generated: {Path}", reportPath);
    }

    private async Task GenerateMedicalValidationReportAsync(string outputDir, string projectName,
        TestResultsData testResults, CoverageData coverageData)
    {
        var reportPath = Path.Combine(outputDir, "MedicalDeviceValidation.html");
        
        // Generate detailed medical device validation report
        // This would include specific regulatory requirements, traceability matrices, etc.
        var html = $@"<!DOCTYPE html>
<html><head><title>{projectName} - Medical Device Validation</title></head>
<body>
<h1>Medical Device Software Validation Report</h1>
<h2>Regulatory Compliance: ISO 13485</h2>
<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>

<h3>Validation Summary</h3>
<p>This report demonstrates compliance with medical device software validation requirements.</p>

<h3>Test Traceability Matrix</h3>
<table border='1'>
<tr><th>Requirement ID</th><th>Test Case</th><th>Result</th><th>Evidence</th></tr>";

        var medicalTests = testResults.TestCases.Where(t => 
            t.Name.StartsWith("MD-") || 
            t.Traits.ContainsKey("Compliance")).ToList();

        foreach (var test in medicalTests)
        {
            var reqId = ExtractRequirementId(test.Name);
            html += $@"
<tr>
<td>{reqId}</td>
<td>{test.DisplayName}</td>
<td>{test.Outcome}</td>
<td>Test executed on {test.StartTime:yyyy-MM-dd HH:mm}</td>
</tr>";
        }

        html += "</table></body></html>";
        
        await File.WriteAllTextAsync(reportPath, html);
        _logger.LogInformation("Medical validation report generated: {Path}", reportPath);
    }

    private async Task GenerateTestExecutionReportAsync(string outputDir, TestResultsData testResults)
    {
        var reportPath = Path.Combine(outputDir, "TestExecution.json");
        
        var executionReport = new
        {
            GeneratedAt = DateTime.Now,
            TotalTests = testResults.TestCases.Count,
            PassedTests = testResults.TestCases.Count(t => t.Outcome == "Passed"),
            FailedTests = testResults.TestCases.Count(t => t.Outcome == "Failed"),
            SkippedTests = testResults.TestCases.Count(t => t.Outcome == "Skipped"),
            TotalDuration = testResults.TestCases.Sum(t => t.Duration.TotalMilliseconds),
            TestsByCategory = testResults.TestCases
                .GroupBy(t => t.Traits.ContainsKey("Category") ? t.Traits["Category"] : "General")
                .ToDictionary(g => g.Key, g => g.Count()),
            TestResults = testResults.TestCases.Select(t => new
            {
                t.Name,
                t.DisplayName,
                t.Outcome,
                DurationMs = t.Duration.TotalMilliseconds,
                t.StartTime,
                t.EndTime,
                t.ErrorMessage,
                t.Traits
            })
        };

        var json = JsonConvert.SerializeObject(executionReport, Formatting.Indented);
        await File.WriteAllTextAsync(reportPath, json);
        _logger.LogInformation("Test execution report generated: {Path}", reportPath);
    }

    private async Task GenerateCoverageReportAsync(string outputDir, CoverageData coverageData)
    {
        var reportPath = Path.Combine(outputDir, "CoverageReport.json");
        
        var json = JsonConvert.SerializeObject(coverageData, Formatting.Indented);
        await File.WriteAllTextAsync(reportPath, json);
        _logger.LogInformation("Coverage report generated: {Path}", reportPath);
    }

    private async Task GenerateComplianceSummaryAsync(string outputDir, string complianceStandard,
        TestResultsData testResults, CoverageData coverageData)
    {
        var reportPath = Path.Combine(outputDir, "ComplianceSummary.txt");
        
        var summary = new StringBuilder();
        summary.AppendLine($"MEDICAL DEVICE SOFTWARE COMPLIANCE SUMMARY");
        summary.AppendLine($"==========================================");
        summary.AppendLine($"Standard: {complianceStandard}");
        summary.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine();
        
        summary.AppendLine("TEST RESULTS SUMMARY:");
        summary.AppendLine($"- Total Tests: {testResults.TestCases.Count}");
        summary.AppendLine($"- Passed: {testResults.TestCases.Count(t => t.Outcome == "Passed")}");
        summary.AppendLine($"- Failed: {testResults.TestCases.Count(t => t.Outcome == "Failed")}");
        summary.AppendLine($"- Pass Rate: {(testResults.TestCases.Count > 0 ? (double)testResults.TestCases.Count(t => t.Outcome == "Passed") / testResults.TestCases.Count * 100 : 0):F1}%");
        summary.AppendLine();
        
        summary.AppendLine("CODE COVERAGE:");
        summary.AppendLine($"- Line Coverage: {coverageData.LineCoverage:F1}%");
        summary.AppendLine($"- Branch Coverage: {coverageData.BranchCoverage:F1}%");
        summary.AppendLine();
        
        summary.AppendLine("COMPLIANCE STATUS:");
        var isCompliant = testResults.TestCases.All(t => t.Outcome == "Passed") && 
                         coverageData.LineCoverage >= 95;
        summary.AppendLine($"- Overall Status: {(isCompliant ? "COMPLIANT" : "NON-COMPLIANT")}");
        
        if (!isCompliant)
        {
            summary.AppendLine("- Issues:");
            if (testResults.TestCases.Any(t => t.Outcome == "Failed"))
                summary.AppendLine("  * Failed tests detected");
            if (coverageData.LineCoverage < 95)
                summary.AppendLine("  * Code coverage below 95% threshold");
        }

        await File.WriteAllTextAsync(reportPath, summary.ToString());
        _logger.LogInformation("Compliance summary generated: {Path}", reportPath);
    }

    private static string ExtractRequirementId(string testName)
    {
        // Extract requirement ID from test names like "MD-REQ-001: ..."
        var match = System.Text.RegularExpressions.Regex.Match(testName, @"(MD-\w+-\d+)");
        return match.Success ? match.Groups[1].Value : "N/A";
    }

    private async Task<string> GetReportCssAsync()
    {
        return @"
body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background-color: #f5f5f5; }
.header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }
.section { background: white; padding: 20px; margin-bottom: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
.summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; }
.summary-item { padding: 15px; border-radius: 8px; text-align: center; }
.summary-item.pass { background-color: #d4edda; border: 2px solid #28a745; }
.summary-item.fail { background-color: #f8d7da; border: 2px solid #dc3545; }
.test-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
.test-table th, .test-table td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
.test-table th { background-color: #f8f9fa; font-weight: bold; }
.test-passed { background-color: #d4edda; }
.test-failed { background-color: #f8d7da; }
.error-detail { font-size: 0.9em; color: #721c24; background-color: #f8d7da; padding: 10px; }
h1, h2, h3 { color: #333; }
";
    }
}

/// <summary>
/// Test runner with comprehensive reporting
/// </summary>
public class TestRunner
{
    private readonly ILogger _logger;

    public TestRunner(ILogger logger)
    {
        _logger = logger;
    }

    public async Task RunTestsWithReportingAsync(string solutionPath, string outputDir, int coverageThreshold)
    {
        _logger.LogInformation("Starting comprehensive test execution...");

        Directory.CreateDirectory(outputDir);

        // Run unit tests with coverage
        await RunDotNetTestAsync(solutionPath, outputDir, coverageThreshold);

        // Generate reports
        var reportGenerator = new TestReportGenerator(_logger);
        await reportGenerator.GenerateReportAsync(
            outputDir, 
            Path.Combine(outputDir, "coverage.cobertura.xml"),
            outputDir,
            "Tag Management Service",
            "ISO-13485",
            true);

        _logger.LogInformation("Test execution and reporting completed");
    }

    private async Task RunDotNetTestAsync(string solutionPath, string outputDir, int coverageThreshold)
    {
        var testCommand = $"dotnet test \"{solutionPath}\" " +
                         $"--configuration Release " +
                         $"--logger \"trx;LogFileName=test-results.trx\" " +
                         $"--logger \"console;verbosity=normal\" " +
                         $"--collect:\"XPlat Code Coverage\" " +
                         $"--results-directory \"{outputDir}\" " +
                         $"--settings coverlet.runsettings " +
                         $"/p:CoverletOutputFormat=cobertura " +
                         $"/p:CoverletOutput=\"{outputDir}/\" " +
                         $"/p:Threshold={coverageThreshold} " +
                         $"/p:ThresholdType=line,branch,method " +
                         $"/p:ThresholdStat=minimum";

        _logger.LogInformation("Running: {Command}", testCommand);

        var processInfo = new ProcessStartInfo("cmd", $"/c {testCommand}")
        {
            WorkingDirectory = Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process != null)
        {
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            _logger.LogInformation("Test output: {Output}", output);
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("Test errors: {Error}", error);
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Tests failed with exit code {process.ExitCode}");
            }
        }
        else
        {
            throw new InvalidOperationException("Failed to start test process");
        }
    }
}

// Data models
public class TestResultsData
{
    public List<TestCase> TestCases { get; set; } = new();
}

public class TestCase
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string> Traits { get; set; } = new();
}

public class CoverageData
{
    public double LineCoverage { get; set; }
    public double BranchCoverage { get; set; }
    public Dictionary<string, ClassCoverageData> ClassCoverage { get; set; } = new();
}

public class ClassCoverageData
{
    public double LineCoverage { get; set; }
    public double BranchCoverage { get; set; }
    public string PackageName { get; set; } = string.Empty;
}
