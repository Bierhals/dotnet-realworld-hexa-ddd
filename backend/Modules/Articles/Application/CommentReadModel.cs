using System;

namespace Conduit.Articles.Application;

public sealed record CommentReadModel(
    int Id,
    string Body,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    AuthorProfile Author);
