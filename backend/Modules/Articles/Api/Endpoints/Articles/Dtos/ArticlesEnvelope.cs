using System.Collections.Generic;

namespace Conduit.Articles.Api.Endpoints.Articles.Dtos;

public sealed record ArticlesEnvelope(IReadOnlyCollection<ArticleResponse> Articles, int ArticlesCount);
