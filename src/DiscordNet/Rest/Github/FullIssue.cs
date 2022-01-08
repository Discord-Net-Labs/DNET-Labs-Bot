namespace DiscordNet.Github
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public interface IssuePR
    {
        long Number { get; }
        Uri HtmlUrl { get; }
        string Title { get; }
        string State { get; }
        Assignee User { get; }
        List<Label> Labels { get; }

        bool IsPullRequest { get; }
    }

    public partial class FullIssue : IssuePR
    {
        public bool IsPullRequest => false;

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("repository_url")]
        public Uri RepositoryUrl { get; set; }

        [JsonProperty("labels_url")]
        public string LabelsUrl { get; set; }

        [JsonProperty("comments_url")]
        public Uri CommentsUrl { get; set; }

        [JsonProperty("events_url")]
        public Uri EventsUrl { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("user")]
        public Assignee User { get; set; }

        [JsonProperty("labels")]
        public List<Label> Labels { get; set; }

        [JsonProperty("assignee")]
        public Assignee Assignee { get; set; }

        [JsonProperty("assignees")]
        public List<Assignee> Assignees { get; set; }

        [JsonProperty("milestone")]
        public Milestone Milestone { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("active_lock_reason")]
        public string ActiveLockReason { get; set; }

        [JsonProperty("comments")]
        public long Comments { get; set; }

        [JsonProperty("pull_request")]
        public PullRequestRaw PullRequest { get; set; }

        [JsonProperty("closed_at")]
        public object ClosedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("closed_by")]
        public Assignee ClosedBy { get; set; }

        [JsonProperty("author_association")]
        public string AuthorAssociation { get; set; }
    }
}
