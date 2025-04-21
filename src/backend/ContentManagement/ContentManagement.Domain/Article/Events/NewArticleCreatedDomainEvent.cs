using Conduit.Users.Domain.Common;

namespace Conduit.Users.Domain.Article.Events;

public class NewArticleCreatedDomainEvent : DomainEvent
{
    public string Slug { get; }

    public NewArticleCreatedDomainEvent(string slug) : base()
    {
        Slug = slug;
    }
}
