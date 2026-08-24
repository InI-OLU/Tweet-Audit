using Microsoft.Extensions.Options;
using System.Text.Json;
using Tweet_Audit.APPLICATION.DTO;
using Tweet_Audit.DOMAIN;

namespace Tweet_Audit.APPLICATION
{

    public class PromptBuilder
    {
        private readonly AlignmentCriteria _criteria;
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false
        };

        public PromptBuilder(IOptions<AlignmentCriteria> criteriaOptions)
        {
            _criteria = criteriaOptions.Value;
        }
        public string BuildPrompt(IEnumerable<Tweet> tweetBatch)
        {
            ArgumentNullException.ThrowIfNull(tweetBatch);
          

            var tweetItems = tweetBatch
                .Select(t => new TweetPromptItem { Id = t.Id, Text = t.FullText })
                .ToList();

            if (tweetItems.Count == 0)
            {
                throw new ArgumentException("Tweet batch cannot be empty.", nameof(tweetBatch));
            }

            string tweetsJson = JsonSerializer.Serialize(tweetItems, SerializerOptions);
            string criteriaJson = JsonSerializer.Serialize(_criteria, SerializerOptions);

            return $$"""
                You are auditing a list of tweets against a user's personal alignment criteria,
                to help them decide which old tweets they may want to delete.

                Alignment criteria (JSON):
                {{criteriaJson}}

                Tweets to evaluate (JSON array of {id, text}):
                {{tweetsJson}}

                For EVERY tweet in the list above, return a verdict — do not skip any tweet,
                even if it clearly does not violate the criteria (in that case, flagged should be false).

                Respond with ONLY a JSON array, no prose, no markdown formatting, matching exactly this shape:
                [
                  { "Id": <int>, "Flagged": <bool>, "Reason": "<short explanation>" }
                ]

                The "id" in each verdict MUST match the "id" of the tweet it corresponds to.
                If a tweet does not violate any criteria, set "flagged" to false and give a brief reason
                confirming why (e.g. "No forbidden words, professional tone").
                """;
        }
    }
}