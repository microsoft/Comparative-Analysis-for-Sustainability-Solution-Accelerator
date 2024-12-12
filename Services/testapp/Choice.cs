using System.ComponentModel;

namespace Tester.ConsoleApp
{
    enum Choice
    {
        [Description("1. List Registered Documents")]
        ListRegisteredDocuments,
        [Description("2. Register Documents")]
        RegisterDocuments,
        [Description("3. Delete Registered Documents in Azure")]
        DeleteDocuments,
        [Description("4. Perform Gap Analysis")]
        GapAnalysis,
        [Description("5. Get All Gap Analysis Results")]
        GetAllGapAnalysisResults,
        [Description("6. Perform Benchmark Analysis")]
        BenchMarks,
        [Description("7. Get All Benchmark Analysis Results")]
        GetAllBenchMarksResults,
        [Description("8. Test API Connection")]
        TestApiConnection,
        [Description("9. Exit")]
        Exit
    }
}
