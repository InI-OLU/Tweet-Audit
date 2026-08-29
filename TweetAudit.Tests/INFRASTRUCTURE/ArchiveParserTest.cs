
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using Moq;
using Tweet_Audit.DOMAIN;
using Tweet_Audit.INFRASTRUCTURE;
using File = System.IO.File;

namespace TweetAudit.Tests.INFRASTRUCTURE
{
    public class ArchiveParserTest
    {
        [Fact]
        public void ArchiveReadAndParse_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            var archivePathSettings = new ArchiveTweetPathSettings
            {
                ArchivePath = "doesnotexist.json"
            };
            var options = Options.Create(archivePathSettings);
            var archiveParser = new ArchiveParser(options);
            Assert.Throws<FileNotFoundException>(() => archiveParser.ArchiveReadAndParse());
           
        }


        [Fact]
        public void ArchiveReadAndParse_ReturnsTweets_WhenArchiveIsValid()
        {
            string tempFile = Path.GetTempFileName();

            string content = """
            window.YTD.tweets.part0 = [
             {
                "tweet": {
               "id_str": "123",
              "full_text": "Hello World"
             }
           }
           ]
         """;

            File.WriteAllText(tempFile, content);

            var settings = new ArchiveTweetPathSettings
            {
                ArchivePath = tempFile
            };

            var options = Options.Create(settings);
            var parser = new ArchiveParser(options);

            var result = parser.ArchiveReadAndParse();

            Assert.NotNull(result);

            
            File.Delete(tempFile);
        }
    }
  
  
}
