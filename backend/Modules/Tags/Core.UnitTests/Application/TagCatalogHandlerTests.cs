using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application.Queries.TagCatalog;
using Conduit.Tags.Core.UnitTests.TestDoubles;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Application;

public class TagCatalogHandlerTests
{
    [Fact]
    public async Task The_catalog_is_handed_out_in_alphabetical_order()
    {
        var handler = new TagCatalogHandler(new FakeTagsReadRepository("training", "dragons", "baby"));

        var result = await handler.Handle(new TagCatalogQuery(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(["baby", "dragons", "training"]);
    }

    [Fact]
    public async Task An_empty_catalog_is_handed_out_as_an_empty_list()
    {
        var handler = new TagCatalogHandler(new FakeTagsReadRepository());

        var result = await handler.Handle(new TagCatalogQuery(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeEmpty();
    }
}
