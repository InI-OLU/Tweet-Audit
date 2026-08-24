using Google.GenAI;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tweet_Audit.APPLICATION.DTO;
using Tweet_Audit.DOMAIN;
using Tweet_Audit.DOMAIN.Exceptions;
using Tweet_Audit.INFRASTRUCTURE;

namespace Tweet_Audit.APPLICATION
{
    public class TweetAuditService
    {
        private readonly ArchiveParser _archiveParser;
        private readonly TweetUrlBuilder _urlBuilder;
        private readonly BatchAuditService _batchAuditService;

        public TweetAuditService(ArchiveParser archiveParser,TweetUrlBuilder urlBuilder,
                                 BatchAuditService batchAuditService)
        {
            _archiveParser = archiveParser;
            _urlBuilder = urlBuilder;
            _batchAuditService = batchAuditService;
        }

        public async Task<List<string>> TaskServiceOrchestrator(IProgress<int> progress)
        {
            var tweets = _archiveParser.ArchiveReadAndParse();

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 1
            };

            if (tweets is null || tweets.Count == 0)
            {
                Console.WriteLine("No tweet found in the file, Check if file is damaged or empty");
                return new List<string>();
            }

            var allVerdicts = new ConcurrentBag<TweetVerdict>();
            var failedBatches = new ConcurrentBag<FailedBatch>();
            var batchContexts = tweets
                                 .Chunk(100)
                                 .Select((chunk, index) => new BatchContext
                                 {
                                     BatchId = index + 1,
                                     Tweets = chunk
                                 })
                                 .ToList();

            int totalBatches = batchContexts.Count;
            int completedBatches = 0;

            await Parallel.ForEachAsync(
              batchContexts,
              parallelOptions,
              async (batchContext, cancellationToken) =>
              {
                  try
                  {
                      var verdicts =
                          await _batchAuditService.AuditBatchAsync(
                              batchContext,
                              cancellationToken);

                      if (verdicts.IsFailed)
                      {
                          failedBatches.Add(
                              new FailedBatch
                              {
                                  BatchId = batchContext.BatchId,
                                  Reason = "Failed to audit batch",
                                  Tweets = batchContext.Tweets
                              });
                      }
                      else
                      {
                          foreach (var verdict in verdicts.Batch)
                          {
                              allVerdicts.Add(verdict);
                          }
                      }
                  }
                  catch (FatalAuditException)
                  {
                      throw;
                  }
                  catch (Exception ex)
                  {
                      failedBatches.Add(
                          new FailedBatch
                          {
                              BatchId = batchContext.BatchId,
                              Reason = ex.Message,
                              Tweets = batchContext.Tweets
                          });
                  }
                  finally
                  {
                      int completedSoFar = Interlocked.Increment(ref completedBatches);
                      int percentComplete = completedSoFar * 100 / totalBatches;
                      progress?.Report(percentComplete);
                  }
              });
            var flaggedTweets = new List<TweetVerdict>();
            foreach(var tweet in allVerdicts)
            {
               if (tweet.Flagged == true)
                {
                    flaggedTweets.Add(tweet);
                }
            }
            var url = _urlBuilder.UrlBuilder(flaggedTweets);
            return url;
          
        }
    }
}