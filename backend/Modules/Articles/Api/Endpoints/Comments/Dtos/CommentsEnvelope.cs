using System.Collections.Generic;

namespace Conduit.Articles.Api.Endpoints.Comments.Dtos;

public sealed record CommentsEnvelope(IReadOnlyCollection<CommentResponse> Comments);
