using System.Collections.Generic;
using Conduit.Host.WebApi.Domain;

namespace Conduit.Host.WebApi.Features.Comments;

public record CommentsEnvelope(List<Comment> Comments);
