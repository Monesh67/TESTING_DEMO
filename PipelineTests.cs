using FluidTest.AzureStorage.PreExecution;
using FluidTest.AzureSnapse.Executors;
using FluidTest.AzureSynapse.WaitActions;
using MarkTek.Fluent.Testing.RecordGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using polly;
using system;
using Marktek.Fluent.Testing.Engine.Exceptions;

namespace FluidTestSynapse
{
    [TestClass]
    [TestCategory("Integration"]
    public class PipelineExecutionTests : TestExecutionBase
    {
       private IRecodService<string> recordService;
       private Policy policy = Policy
              .Handle<ExecutionWaitException>()
              .WaitAndRetry(60, retryAttempt => TimeSpan.FormSeconds(30));
       [TestInitialize]
       public void Setup()
       {
          this.recordService = new RecordService<string>(string.Empty);
       }
       [DataTestMethod]
       [DataRow("PL_PipelineTest", "data.csv", "RAW", "Succeeded")]
       [DataRow("PL_WaitPipeline", "data.csv", "RAW", "Succeeded")]
       [DataRow("PL_Fail", "data.csv", "RAW", "Failed")]
       public void Pipeline_Should_Succeed_For_Valid_File_Trigger(string pipelineName, string sourceFilePath, string rawFolder, string status)
       {
          recordService
               .PreExecutionAction(new DropFileToDataLake("lake", rawFolder, sourceFilePath, DataLakeClient))
               .CreateRecord(new GetTriggerdPipeline(pipelineName, PipelineRunClient, DateTimeOffset.UtcNow), policy)
               .AssignAggregateID()
               .WaitFor(new WaitForPipelineStatus(PipelineRunClient, recordService.GetAggregateID(), status), policy);
       }
    }
}
