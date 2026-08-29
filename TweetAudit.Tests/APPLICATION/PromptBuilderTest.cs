using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweet_Audit.APPLICATION;
using Tweet_Audit.DOMAIN;

namespace TweetAudit.Tests.APPLICATION
{
    public class PromptBuilderTest
    {
        [Fact]
        public void BuildPrompt_ThrowsArgumentNullException_WhenTweetBatchIsNull()
        {
            var promptBuilder = CreatePromptBuilder();

            Assert.Throws<ArgumentNullException>(
                () => promptBuilder.BuildPrompt(null!));
        }
        [Fact]
        public void BuildPrompt_ThrowsArgumentException_WhenTweetBatchIsEmpty()
        {
            var promptBuilder = CreatePromptBuilder();

            var tweets = Enumerable.Empty<Tweet>();

            Assert.Throws<ArgumentException>(
                () => promptBuilder.BuildPrompt(tweets));
        }
        [Fact]
        public void BuildPrompt_ContainsTweetData_WhenTweetsProvided()
        {
            var promptBuilder = CreatePromptBuilder();

            var tweets = new[]
            {
        new Tweet
        {
            Id = "123",
            FullText = "Hello World"
        }
    };

            var prompt = promptBuilder.BuildPrompt(tweets);

            Assert.Contains("123", prompt);
            Assert.Contains("Hello World", prompt);
        }

        [Fact]
        public void BuildPrompt_ContainsAlignmentCriteria_WhenPromptIsGenerated()
        {
            var promptBuilder = CreatePromptBuilder();

            var tweets = new[]
            {
        new Tweet
        {
            Id = "123",
            FullText = "Test Tweet"
        }
    };

            var prompt = promptBuilder.BuildPrompt(tweets);

            Assert.Contains("happy", prompt);
            Assert.Contains("sad", prompt);
            Assert.Contains("professional", prompt);
        }
        [Fact]
        public void BuildPrompt_ContainsAllTweets_WhenMultipleTweetsProvided()
        {
            var promptBuilder = CreatePromptBuilder();

            var tweets = new[]
            {
        new Tweet
        {
            Id = "1",
            FullText = "First tweet"
        },
        new Tweet
        {
            Id = "2",
            FullText = "Second tweet"
        }
    };
            var prompt = promptBuilder.BuildPrompt(tweets);

            Assert.Contains("First tweet", prompt);
            Assert.Contains("Second tweet", prompt);
            Assert.Contains("\"Id\":\"1\"", prompt);
            Assert.Contains("\"Id\":\"2\"", prompt);
        }
        [Fact]
        public void BuildPrompt_ReturnsNonEmptyPrompt_WhenInputIsValid()
        {
            var promptBuilder = CreatePromptBuilder();

            var tweets = new[]
            {
        new Tweet
        {
            Id = "123",
            FullText = "Testing"
        }
         };

            var prompt = promptBuilder.BuildPrompt(tweets);

            Assert.False(string.IsNullOrWhiteSpace(prompt));
        }
        private static PromptBuilder CreatePromptBuilder()
        {

            var criteria = new AlignmentCriteria
            {
                ForbiddenWords = new List<string> { "happy", "sad" },
                ProfessionalCheck = true,
                Tone = "professional",
                ExcludePolitics = true
            };

            var options = Options.Create(criteria);

            return new PromptBuilder(options);
        }
    }
}
