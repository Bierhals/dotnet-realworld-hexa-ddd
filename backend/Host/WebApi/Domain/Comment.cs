using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Conduit.Host.WebApi.Domain;

public class Comment
{
    [JsonPropertyName("id")]
    public int CommentId { get; init; }

    public string? Body { get; init; }

    [JsonIgnore]
    public string AuthorUsername { get; set; } = null!;

    [NotMapped]
    public AuthorProfile? Author { get; set; }

    [JsonIgnore]
    public Article? Article { get; init; }

    [JsonIgnore]
    public int ArticleId { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
