using System.Net;
using System.Text.Json;

using Platform.IntegrationTests.Shared;

using Shouldly;

using Xunit;

namespace QueryReadModel.IntegrationTests;

public sealed class OpenApiDocumentTests : IClassFixture<QueryReadModelApiFactory>
{
    private readonly QueryReadModelApiFactory _factory;

    public OpenApiDocumentTests(QueryReadModelApiFactory factory) =>
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    [Fact]
    public async Task OpenApi_v1_document_returns_ok_statusAsync()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApi_v1_document_includes_platform_optional_headers_on_operationsAsync()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        (bool foundCorrelation, bool foundTenant) = OpenApiOptionalHeaderScan.TryFindPlatformHeaders(doc.RootElement.GetProperty("paths"));
        foundCorrelation.ShouldBeTrue();
        foundTenant.ShouldBeTrue();
    }

    [Fact]
    public async Task OpenApi_v1_document_includes_bearer_jwt_security_schemeAsync()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        OpenApiBearerSecurityScan.HasBearerJwtSecurityScheme(doc.RootElement).ShouldBeTrue();
        OpenApiBearerSecurityScan.BearerDescriptionListsAuthorizationScopes(doc.RootElement).ShouldBeTrue();
    }

    [Fact]
    public async Task OpenApi_v1_document_marks_authorized_operations_with_bearer_requirementAsync()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement paths = doc.RootElement.GetProperty("paths");
        OpenApiBearerSecurityScan.AnyOperationRequiresBearerSecurity(paths).ShouldBeTrue();
        OpenApiBearerSecurityScan.AnyBearerRequirementListsScope(paths, "Dialysis.ReadModel.Write").ShouldBeTrue();
        OpenApiBearerSecurityScan.AnyBearerRequirementListsScope(paths, "Dialysis.ReadModel.Read").ShouldBeTrue();
    }
}
