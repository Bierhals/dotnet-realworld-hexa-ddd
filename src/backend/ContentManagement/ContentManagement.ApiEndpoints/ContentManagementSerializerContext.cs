using System;
using System.Text.Json.Serialization;
using Conduit.ContentManagement.ApiEndpoints.Models;

namespace Conduit.ContentManagement.ApiEndpoints;

[JsonSerializable(typeof(Profile))]
[JsonSerializable(typeof(Article))]
[JsonSerializable(typeof(NewArticle))]
[JsonSerializable(typeof(NewArticleRequest))]
[JsonSerializable(typeof(GetArticlesFeedRequest))]
[JsonSerializable(typeof(MultipleArticlesResponse))]
[JsonSerializable(typeof(SingleArticleResponse))]
[JsonSerializable(typeof(GetArticlesRequest))]
[JsonSerializable(typeof(Models.UpdateArticle))]
[JsonSerializable(typeof(UpdateArticleRequest))]
[JsonSerializable(typeof(TagsResponse))]
internal partial class ContentManagementSerializerContext : JsonSerializerContext
{

}
