using Google.GenAI;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using Tweet_Audit.DOMAIN;
using Tweet_Audit.DOMAIN.Exceptions;

namespace Tweet_Audit.APPLICATION;

public class BatchAuditService
{   
    private const int MaxRetries = 3;

    private readonly GeminiClient _geminiClient;
    private readonly PromptBuilder _promptBuilder;

    public BatchAuditService(
        GeminiClient geminiClient,
        PromptBuilder promptBuilder)
    {
        _geminiClient = geminiClient;
        _promptBuilder = promptBuilder;
    }
    
    public async Task<BatchResult> AuditBatchAsync(
         BatchContext batch,
        CancellationToken cancellationToken = default)
    {

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var result = await ProcessTweetAsync(batch, cancellationToken);
                return new BatchResult
                {
                    Batch = result,
                    IsFailed = false
                };
            }
            catch (ClientError ex) when (ex.StatusCode == 429)
            {

                if (attempt == MaxRetries)
                {
                    return new BatchResult { Batch = null, IsFailed = true };
                }

                await DelayBeforeRetryAsync(attempt, cancellationToken);
            }
            catch (ClientError ex)
            {
                throw new FatalAuditException(
                    $"Gemini API rejected the request with a non-retryable client error ({ex.StatusCode}).",
                    ex.StatusCode,
                    ex);
            }
            catch (ServerError ex) when (IsRetryableServerStatus(ex.StatusCode))
            {
                if (attempt == MaxRetries)
                {
                    return new BatchResult { Batch = null, IsFailed = true };
                }

                await DelayBeforeRetryAsync(attempt, cancellationToken);
            }
            catch (ServerError)
            {
                return new BatchResult { Batch = null, IsFailed = true };
            }
            catch (MalformedGeminiResponseException)
            {
                return new BatchResult { Batch = null, IsFailed = true };
            }
            catch (BatchValidationException)
            {
                return new BatchResult { Batch = null, IsFailed = true };
            }
        }
        throw new InvalidOperationException();
    }


    private static bool IsRetryableServerStatus(int statusCode) =>
      statusCode is 500 or 503 or 504;

    private static async Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken)
    {
        var delaySeconds = Math.Pow(2, attempt + 1);
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }
    private async Task<List<TweetVerdict>> ProcessTweetAsync(BatchContext batch,
        CancellationToken cancellationToken = default)
    {
       
        var prompt = _promptBuilder.BuildPrompt(batch.Tweets);

        var jsonResponse =
            await _geminiClient.GeminiAuditAsync(
                prompt,
                cancellationToken);

        var typedResponse =
           JsonSerializer.Deserialize<List<TweetVerdict>>(
               jsonResponse)
           ?? throw new MalformedGeminiResponseException();  
        ValidateBatch(batch.Tweets, typedResponse);

        return typedResponse;
    }
    
  
    private static void ValidateBatch(
        Tweet[] batch,
        List<TweetVerdict> verdicts)
    {
        if (verdicts.Count != batch.Length)
        {
            throw new BatchValidationException();
        }

        var expectedIds =
            batch.Select(t => t.Id)
                 .ToHashSet();

        var returnedIds =
            verdicts.Select(v => v.Id)
                    .ToHashSet();

        if (!expectedIds.SetEquals(returnedIds))
        {
            throw new BatchValidationException();
        }
    }
}