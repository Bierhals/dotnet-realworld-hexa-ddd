using System.Collections.Generic;
using Conduit.Articles.Application.Commands.CreateArticle;
using Conduit.Articles.Application.Commands.CreateComment;
using Conduit.Articles.Application.Commands.DeleteArticle;
using Conduit.Articles.Application.Commands.DeleteComment;
using Conduit.Articles.Application.Commands.EditArticle;
using Conduit.Articles.Application.Commands.FavoriteArticle;
using Conduit.Articles.Application.Commands.UnfavoriteArticle;
using Conduit.Articles.Application.Queries.ArticleDetails;
using Conduit.Articles.Application.Queries.ArticleFeed;
using Conduit.Articles.Application.Queries.ArticleList;
using Conduit.Articles.Application.Queries.CommentList;
using Conduit.Shared.Application.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Articles.Application;

public static class ArticlesApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddArticlesApplication(this IServiceCollection services)
    {
        services.AddCqrsMediator();

        services.AddScoped<ICommandHandler<CreateArticleCommand, string>, CreateArticleHandler>();
        services.AddScoped<ICommandHandler<EditArticleCommand, string>, EditArticleHandler>();
        services.AddScoped<ICommandHandler<DeleteArticleCommand>, DeleteArticleHandler>();
        services.AddScoped<ICommandHandler<FavoriteArticleCommand>, FavoriteArticleHandler>();
        services.AddScoped<ICommandHandler<UnfavoriteArticleCommand>, UnfavoriteArticleHandler>();
        services.AddScoped<ICommandHandler<CreateCommentCommand, CommentReadModel>, CreateCommentHandler>();
        services.AddScoped<ICommandHandler<DeleteCommentCommand>, DeleteCommentHandler>();

        services.AddScoped<IQueryHandler<ArticleDetailsQuery, ArticleReadModel>, ArticleDetailsHandler>();
        services.AddScoped<IQueryHandler<ArticleListQuery, ArticleListReadModel>, ArticleListHandler>();
        services.AddScoped<IQueryHandler<ArticleFeedQuery, ArticleListReadModel>, ArticleFeedHandler>();
        services.AddScoped<IQueryHandler<CommentListQuery, IReadOnlyCollection<CommentReadModel>>, CommentListHandler>();

        return services;
    }
}
