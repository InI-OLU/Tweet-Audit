using Google.GenAI;
using Microsoft.Extensions.Options;
using Moq;
using Tweet_Audit.APPLICATION;
using Tweet_Audit.APPLICATION.INTERFACE;
using Tweet_Audit.DOMAIN;
using Tweet_Audit.DOMAIN.Exceptions;


namespace TweetAudit.Tests.APPLICATION
{
   public class BatchAuditServiceTest
    {
        private readonly Mock<IGeminiClient> _mockGeminiClient;
        public BatchAuditServiceTest()
        {
            _mockGeminiClient = new Mock<IGeminiClient>();
        }
        [Fact]
        public async Task AuditBatchAsync_RetriesUpToThreeTimes_WhenClientIsRateLimited429()
        {

            var batchContext = new BatchContext
            {
                BatchId = 1,
                Tweets = new Tweet[] { new Tweet { Id = "123568955786", FullText = "yoo what's popping", CreatedAt = "27082026102609" },new Tweet { Id = "467876543456", FullText = "I am good , you?", CreatedAt = "270820261040"} }
            };

            _mockGeminiClient.Setup(service => service.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                                .ThrowsAsync(new ClientError("Rate limit exceeded",429));
            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "angry tone",
                ExcludePolitics = false
            };
            var options = Options.Create(criteria);
            var promptBuilder = new PromptBuilder(options);
            var auditBatchService = new BatchAuditService(_mockGeminiClient.Object,promptBuilder);
            var result = await auditBatchService.AuditBatchAsync(batchContext, CancellationToken.None);
            _mockGeminiClient.Verify(x => x.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            Assert.True(result.IsFailed);
            Assert.Null(result.Batch);
            


        }
        [Fact]
        //this tests that there is up to 3 retries as the function intended ,However only error 500 is tested here 
        //it is assumed that the remaining  transient 5** errors(503,504) would be retired would also pass
        public async Task AuditBatchAsync_RetriesUpToThreeTimes_ServerError()
        {
            var batchContext = new BatchContext
            {
                BatchId = 1,
                Tweets = new Tweet[] { new Tweet { Id = "123568955786", FullText = "yoo what's popping", CreatedAt = "27082026102609" }, new Tweet { Id = "467876543456", FullText = "I am good , you?", CreatedAt = "270820261040" } }
            };

            _mockGeminiClient.Setup(service => service.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                                .ThrowsAsync(new ServerError("Server timed out", 500));
            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "angry tone",
                ExcludePolitics = false
            };
            var options = Options.Create(criteria);
            var promptBuilder = new PromptBuilder(options);
            var auditBatchService = new BatchAuditService(_mockGeminiClient.Object, promptBuilder);
            var result = await auditBatchService.AuditBatchAsync(batchContext, CancellationToken.None);
            Assert.True(result.IsFailed);
            Assert.Null(result.Batch);
            _mockGeminiClient.Verify(x => x.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(4));

        }
        [Fact]
        public async Task AuditBatchAsync_DoesNotRetry_ClientError401()
        {

            var batchContext = new BatchContext
            {
                BatchId = 1,
                Tweets = new Tweet[] { new Tweet { Id = "123568955786", FullText = "yoo what's popping", CreatedAt = "27082026102609" }, new Tweet { Id = "467876543456", FullText = "I am good , you?", CreatedAt = "270820261040" } }
            };

            _mockGeminiClient.Setup(service => service.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                                .ThrowsAsync(new ClientError("User not Authorized", 401));
            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "angry tone",
                ExcludePolitics = false
            };
            var options = Options.Create(criteria);
            var promptBuilder = new PromptBuilder(options);
            var auditBatchService = new BatchAuditService(_mockGeminiClient.Object, promptBuilder);
            var service = new BatchAuditService( _mockGeminiClient.Object, promptBuilder);
            await Assert.ThrowsAsync<FatalAuditException>(() => auditBatchService.AuditBatchAsync(batchContext, CancellationToken.None));
            _mockGeminiClient.Verify(x => x.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);




        }

        [Fact]
        public async Task AuditBatchAsync_DoesNotRetry_ClientError501()
        {

            var batchContext = new BatchContext
            {
                BatchId = 1,
                Tweets = new Tweet[] { new Tweet { Id = "123568955786", FullText = "yoo what's popping", CreatedAt = "27082026102609" }, new Tweet { Id = "467876543456", FullText = "I am good , you?", CreatedAt = "270820261040" } }
            };

            _mockGeminiClient.Setup(service => service.GeminiAuditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                                .ThrowsAsync(new ServerError("Request Timed Out", 501));
            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "angry tone",
                ExcludePolitics = false
            };
            var options = Options.Create(criteria);
            var promptBuilder = new PromptBuilder(options);
            var auditBatchService = new BatchAuditService(_mockGeminiClient.Object, promptBuilder);
            var result = await auditBatchService.AuditBatchAsync(batchContext, CancellationToken.None);
            Assert.True(result.IsFailed);
            Assert.Null(result.Batch);

        }
        [Fact]
        public async Task AuditBatchAsync_Fails_WhenGeminiReturnsNullJson()
        {
            var batchContext = new BatchContext
            {
                BatchId = 1,
                Tweets = new Tweet[] { new Tweet { Id = "123568955786", FullText = "yoo what's popping", CreatedAt = "27082026102609" }, new Tweet { Id = "467876543456", FullText = "I am good , you?", CreatedAt = "270820261040" } }
            };
            _mockGeminiClient
                .Setup(x => x.GeminiAuditAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("null");

            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "angry tone",
                ExcludePolitics = false
            };
            var options = Options.Create(criteria);
            var promptBuilder = new PromptBuilder(options);
            var auditBatchService = new BatchAuditService(_mockGeminiClient.Object, promptBuilder);
            var result = await auditBatchService.AuditBatchAsync(
                batchContext,
                CancellationToken.None);
            Assert.True(result.IsFailed);
            Assert.Null(result.Batch);
            _mockGeminiClient.Verify(
              x => x.GeminiAuditAsync(
                  It.IsAny<string>(),
                  It.IsAny<CancellationToken>()),
              Times.Once);
        }

        [Fact]
        //this is to test that the class throws a BatchValidationException and fails when gemini gives a wrong output
        public async Task AuditBatchAsync_Fails_WhenVerdictsDoNotMatchBatchIds()
        {

            var batchContext = new BatchContext
            {
                BatchId = 1,
                Tweets = new Tweet[]
                {
            new Tweet
            {
                Id = "123568955786",
                FullText = "yoo what's popping",
                CreatedAt = "27082026102609"
            },
            new Tweet
            {
                Id = "467876543456",
                FullText = "I am good , you?",
                CreatedAt = "270820261040"
            }
                }
            };

            var invalidResponse = """
             [
                  {
                      "Id": "111111111",
                      "Flagged": false,
                      "Reason": "ok"
                   },
                   {
                     "Id": "222222222",
                     "Flagged": false,
                     "Reason": "ok"
                    }
              ]
    """;

            _mockGeminiClient
                .Setup(x => x.GeminiAuditAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidResponse);

            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "angry tone",
                ExcludePolitics = false
            };

            var options = Options.Create(criteria);
            var promptBuilder = new PromptBuilder(options);

            var auditBatchService = new BatchAuditService(
                _mockGeminiClient.Object,
                promptBuilder);


            var result = await auditBatchService.AuditBatchAsync(
                batchContext,
                CancellationToken.None);


            Assert.True(result.IsFailed);
            Assert.Null(result.Batch);

            _mockGeminiClient.Verify(
                x => x.GeminiAuditAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
