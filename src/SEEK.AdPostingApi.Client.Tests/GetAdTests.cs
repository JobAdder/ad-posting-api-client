﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using PactNet.Mocks.MockHttpService.Models;
using SEEK.AdPostingApi.Client.Models;
using SEEK.AdPostingApi.Client.Resources;
using SEEK.AdPostingApi.Client.Tests.Framework;
using Xunit;

namespace SEEK.AdPostingApi.Client.Tests
{
    [Collection(AdPostingApiCollection.Name)]
    public class GetAdTests : IDisposable
    {
        private const string AdvertisementLink = "/advertisement";
        private const string RequestId = "PactRequestId";

        private IBuilderInitializer MinimumFieldsInitializer => new MinimumFieldsInitializer();

        private IBuilderInitializer AllFieldsInitializer => new AllFieldsInitializer();

        public GetAdTests(AdPostingApiPactService adPostingApiPactService)
        {
            this.Fixture = new AdPostingApiFixture(adPostingApiPactService);
        }

        public void Dispose()
        {
            this.Fixture.Dispose();
        }

        [Theory]
        [InlineData(LocationType.UseGranularLocation, "There is a pending standout advertisement with granular location data")]
        [InlineData(LocationType.UseLocation, "There is a pending standout advertisement with maximum data")]
        public async Task GetExistingAdvertisement(LocationType locationType, string givenStatement)
        {
            const string advertisementId = "8e2fde50-bc5f-4a12-9cfb-812e50500184";

            OAuth2Token oAuth2Token = new OAuth2TokenBuilder().Build();
            var link = $"{AdvertisementLink}/{advertisementId}";
            var viewRenderedAdvertisementLink = $"{AdvertisementLink}/{advertisementId}/view";

            var builderInitializer = new AllFieldsInitializer(locationType);

            this.Fixture.AdPostingApiService
                .Given(givenStatement)
                .UponReceiving("a GET advertisement request")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = link,
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", "Bearer " + oAuth2Token.AccessToken },
                        { "Accept", $"{ResponseContentTypes.AdvertisementVersion1}, {ResponseContentTypes.AdvertisementErrorVersion1}" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", ResponseContentTypes.AdvertisementVersion1 },
                        { "Processing-Status", "Pending" },
                        { "X-Request-Id", RequestId }
                    },
                    Body = new AdvertisementResponseContentBuilder(builderInitializer)
                        .WithId(advertisementId)
                        .WithState(AdvertisementState.Open.ToString())
                        .WithLink("self", link)
                        .WithLink("view", viewRenderedAdvertisementLink)
                        .WithAgentId(null)
                        .WithAdditionalProperties(AdditionalPropertyType.ResidentsOnly.ToString(), AdditionalPropertyType.Graduate.ToString())
                        .Build()
                });

            AdvertisementResource result;

            using (AdPostingApiClient client = this.Fixture.GetClient(oAuth2Token))
            {
                result = await client.GetAdvertisementAsync(new Uri(this.Fixture.AdPostingApiServiceBaseUri, link));
            }

            AdvertisementResource expectedResult = new AdvertisementResourceBuilder(builderInitializer)
                .WithId(new Guid(advertisementId))
                .WithLinks(advertisementId)
                .WithProcessingStatus(ProcessingStatus.Pending)
                .WithAgentId(null)
                .Build();

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetExistingAdvertisementWithWarnings()
        {
            const string advertisementId = "8e2fde50-bc5f-4a12-9cfb-812e50500184";

            OAuth2Token oAuth2Token = new OAuth2TokenBuilder().Build();
            var link = $"{AdvertisementLink}/{advertisementId}";
            var viewRenderedAdvertisementLink = $"{AdvertisementLink}/{advertisementId}/view";

            this.Fixture.AdPostingApiService
                .Given("There is a pending standout advertisement with maximum data")
                .UponReceiving("a GET advertisement request for an advertisement with warnings")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = link,
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", "Bearer " + oAuth2Token.AccessToken },
                        { "Accept", $"{ResponseContentTypes.AdvertisementVersion1}, {ResponseContentTypes.AdvertisementErrorVersion1}" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", ResponseContentTypes.AdvertisementVersion1 },
                        { "Processing-Status", "Pending" },
                        { "X-Request-Id", RequestId }
                    },
                    Body = new AdvertisementResponseContentBuilder(AllFieldsInitializer)
                        .WithId(advertisementId)
                        .WithState(AdvertisementState.Open.ToString())
                        .WithLink("self", link)
                        .WithLink("view", viewRenderedAdvertisementLink)
                        .WithWarnings(
                            new { field = "standout.logoId", code = "missing" },
                            new { field = "standout.bullets", code = "missing" })
                        .WithAgentId(null)
                        .WithAdditionalProperties(AdditionalPropertyType.ResidentsOnly.ToString(), AdditionalPropertyType.Graduate.ToString())
                        .Build()
                });

            AdvertisementResource result;

            using (AdPostingApiClient client = this.Fixture.GetClient(oAuth2Token))
            {
                result = await client.GetAdvertisementAsync(new Uri(this.Fixture.AdPostingApiServiceBaseUri, link));
            }

            AdvertisementResource expectedResult = new AdvertisementResourceBuilder(this.AllFieldsInitializer)
                .WithId(new Guid(advertisementId))
                .WithLinks(advertisementId)
                .WithProcessingStatus(ProcessingStatus.Pending)
                .WithWarnings(
                    new AdvertisementError { Field = "standout.logoId", Code = "missing" },
                    new AdvertisementError { Field = "standout.bullets", Code = "missing" })
                .WithAgentId(null)
                .Build();

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetExistingAdvertisementWithErrors()
        {
            const string advertisementId = "448b8474-6165-4eed-a5b5-d2bb52e471ef";

            OAuth2Token oAuth2Token = new OAuth2TokenBuilder().Build();
            var link = $"{AdvertisementLink}/{advertisementId}";
            var viewRenderedAdvertisementLink = $"{AdvertisementLink}/{advertisementId}/view";

            this.Fixture.AdPostingApiService
                .Given("There is a failed classic advertisement")
                .UponReceiving("a GET advertisement request for an advertisement with errors")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = link,
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", "Bearer " + oAuth2Token.AccessToken },
                        { "Accept", $"{ResponseContentTypes.AdvertisementVersion1}, {ResponseContentTypes.AdvertisementErrorVersion1}" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", ResponseContentTypes.AdvertisementVersion1 },
                        { "Processing-Status", "Failed" },
                        { "X-Request-Id", RequestId }
                    },
                    Body = new AdvertisementResponseContentBuilder(this.MinimumFieldsInitializer)
                        .WithId(advertisementId)
                        .WithState(AdvertisementState.Open.ToString())
                        .WithLink("self", link)
                        .WithLink("view", viewRenderedAdvertisementLink)
                        .WithErrors(new { code = "Unauthorised", message = "Unauthorised" })
                        .WithAgentId(null)
                        .Build()
                });

            AdvertisementResource result;

            using (AdPostingApiClient client = this.Fixture.GetClient(oAuth2Token))
            {
                result = await client.GetAdvertisementAsync(new Uri(this.Fixture.AdPostingApiServiceBaseUri, link));
            }

            AdvertisementResource expectedResult = new AdvertisementResourceBuilder(this.MinimumFieldsInitializer)
                .WithId(new Guid(advertisementId))
                .WithLinks(advertisementId)
                .WithProcessingStatus(ProcessingStatus.Failed)
                .WithErrors(new AdvertisementError { Code = "Unauthorised", Message = "Unauthorised" })
                .WithAgentId(null)
                .Build();

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetNonExistentAdvertisement()
        {
            const string advertisementId = "9b650105-7434-473f-8293-4e23b7e0e064";
            OAuth2Token oAuth2Token = new OAuth2TokenBuilder().Build();
            var link = $"{AdvertisementLink}/{advertisementId}";

            this.Fixture.AdPostingApiService
                .UponReceiving("a GET advertisement request for a non-existent advertisement")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = link,
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", "Bearer " + oAuth2Token.AccessToken },
                        { "Accept", $"{ResponseContentTypes.AdvertisementVersion1}, {ResponseContentTypes.AdvertisementErrorVersion1}" }
                    }
                })
                .WillRespondWith(
                    new ProviderServiceResponse
                    {
                        Status = 404,
                        Headers = new Dictionary<string, string> { { "X-Request-Id", RequestId } }
                    });

            AdvertisementNotFoundException actualException;

            using (AdPostingApiClient client = this.Fixture.GetClient(oAuth2Token))
            {
                actualException =
                    await Assert.ThrowsAsync<AdvertisementNotFoundException>(
                        async () => await client.GetAdvertisementAsync(new Uri(this.Fixture.AdPostingApiServiceBaseUri, link)));
            }

            actualException.ShouldBeEquivalentToException(new AdvertisementNotFoundException(RequestId));
        }

        [Fact]
        public async Task GetAdvertisementUsingDisabledRequestorAccount()
        {
            const string advertisementId = "8e2fde50-bc5f-4a12-9cfb-812e50500184";

            OAuth2Token oAuth2Token = new OAuth2TokenBuilder().WithAccessToken(AccessTokens.ValidAccessToken_Disabled).Build();
            var link = $"{AdvertisementLink}/{advertisementId}";

            this.Fixture.AdPostingApiService
                .Given("There is a pending standout advertisement with maximum data")
                .UponReceiving("a GET advertisement request for an advertisement using a disabled requestor account")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = link,
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", "Bearer " + oAuth2Token.AccessToken },
                        { "Accept", $"{ResponseContentTypes.AdvertisementVersion1}, {ResponseContentTypes.AdvertisementErrorVersion1}" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 403,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", ResponseContentTypes.AdvertisementErrorVersion1 },
                        { "X-Request-Id", RequestId }
                    },
                    Body = new
                    {
                        message = "Forbidden",
                        errors = new[] { new { code = "AccountError" } }
                    }
                });

            UnauthorizedException actualException;

            using (AdPostingApiClient client = this.Fixture.GetClient(oAuth2Token))
            {
                actualException = await Assert.ThrowsAsync<UnauthorizedException>(
                    async () => await client.GetAdvertisementAsync(new Uri(this.Fixture.AdPostingApiServiceBaseUri, link)));
            }

            actualException.ShouldBeEquivalentToException(
                new UnauthorizedException(
                    RequestId,
                    new AdvertisementErrorResponse
                    {
                        Message = "Forbidden",
                        Errors = new[] { new AdvertisementError { Code = "AccountError" } }
                    }));
        }

        [Fact]
        public async Task GetAdvertisementWhereAdvertiserNotRelatedToRequestor()
        {
            const string advertisementId = "8e2fde50-bc5f-4a12-9cfb-812e50500184";

            OAuth2Token oAuth2Token = new OAuth2TokenBuilder().WithAccessToken(AccessTokens.OtherThirdPartyUploader).Build();
            var link = $"{AdvertisementLink}/{advertisementId}";

            this.Fixture.AdPostingApiService
                .Given("There is a pending standout advertisement with maximum data")
                .UponReceiving("a GET advertisement request for an advertisement of an advertiser not related to the requestor's account")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = link,
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", "Bearer " + oAuth2Token.AccessToken },
                        { "Accept", $"{ResponseContentTypes.AdvertisementVersion1}, {ResponseContentTypes.AdvertisementErrorVersion1}" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 403,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", ResponseContentTypes.AdvertisementErrorVersion1 },
                        { "X-Request-Id", RequestId }
                    },
                    Body = new
                    {
                        message = "Forbidden",
                        errors = new[]
                        {
                            new { code = "RelationshipError" }
                        }
                    }
                });

            UnauthorizedException actualException;

            using (AdPostingApiClient client = this.Fixture.GetClient(oAuth2Token))
            {
                actualException = await Assert.ThrowsAsync<UnauthorizedException>(
                    async () => await client.GetAdvertisementAsync(new Uri(this.Fixture.AdPostingApiServiceBaseUri, link)));
            }

            actualException.ShouldBeEquivalentToException(
                new UnauthorizedException(
                    RequestId,
                    new AdvertisementErrorResponse
                    {
                        Message = "Forbidden",
                        Errors = new[] { new AdvertisementError { Code = "RelationshipError" } }
                    }));
        }

        private AdPostingApiFixture Fixture { get; }
    }
}