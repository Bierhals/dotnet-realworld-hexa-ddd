using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Dtos;
using Conduit.Application.Services;
using Conduit.Domain.Article;
using Conduit.Domain.Common;
using Conduit.Domain.User;
using CSharpFunctionalExtensions;
using MediatR;

namespace Conduit.Users.Application.Articles.Commands.CreateArticle;

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, Result<string, Error>>
{
    readonly IAuthenticationService _authenticationService;
    readonly IAuthenticatedUserService _authenticatedUserService;
    readonly IArticleCounter _articleCounter;
    readonly IUnitOfWork _unitOfWork;

    public CreateArticleHandler(
        IAuthenticationService authenticationService,
        IAuthenticatedUserService authenticatedUserService,
        IArticleCounter articleCounter,
        IUnitOfWork unitOfWork)
    {
        _authenticationService = authenticationService;
        _authenticatedUserService = authenticatedUserService;
        _articleCounter = articleCounter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string, Error>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        AuthenticatedUserDto? authUser = _authenticatedUserService.GetAuthenticatedUser();
        if (authUser == null)
        {
            return Result.Failure<string, Error>(AuthenticationErrors.UserIsNotAuthorized());
        }

        Result<ArticleSlug, Error> slug = ArticleSlug.CreateFromTitle(request.Title);
        Result<ArticleTitle, Error> title = ArticleTitle.Create(request.Title);
        Result<ArticleDescription, Error> description = ArticleDescription.Create(request.Description);
        Result<ArticleBody, Error> body = ArticleBody.Create(request.Body);
        UserId authorId = UserId.Create(authUser.UserId);

        return Task.FromResult(Result.Combine<Error>(slug, title, description, body))
            .Bind(() => Article.CreateNewArticle(authorId, slug, title, description, body, [..request.TagList], _articleCounter))
            .Map(async (newArticle) =>
            {
                await _articleRepository.AddAsync(newArticle, cancellationToken);
                await _unitOfWork.CommitAsync();

                return new ArticleDto
                {
                    Id = newUser.Id.Value,
                    Email = newUser.Email.Value,
                    Username = newUser.Username.Value,
                    Bio = newUser.Bio,
                    Image = newUser.Image,
                    Token = _authenticationService.GenerateJwtToken(newUser.Id.Value)
                };
            });
    }
}
