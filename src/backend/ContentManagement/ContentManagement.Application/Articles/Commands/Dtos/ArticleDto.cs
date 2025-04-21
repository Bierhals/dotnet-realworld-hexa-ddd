using System;
using System.Collections;

namespace Conduit.Users.Application.Articles.Commands.Dtos;

public record ArticleDto
{
    public required string Id { get; init; }
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Body { get; init; }
    public required IEnumerable TagList { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required bool Favorited { get; init; }
    public required int FavoritesCount { get; init; }
    //public required Profile Author { get; init; }
}
