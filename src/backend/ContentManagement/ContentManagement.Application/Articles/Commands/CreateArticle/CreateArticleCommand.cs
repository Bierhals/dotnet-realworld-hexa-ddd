/* using System.Collections.Generic;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;
using MediatR;

namespace Conduit.Users.Application.Articles.Commands.CreateArticle;

public class CreateArticleCommand : IRequest<Result<string, Error>>
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Body { get; init; }
    public IEnumerable<string>? TagList { get; init; }
}
 */
