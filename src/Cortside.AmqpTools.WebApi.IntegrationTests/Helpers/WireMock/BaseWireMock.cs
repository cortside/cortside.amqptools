using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Cortside.AmqpTools.WebApi.IntegrationTests.Helpers.WireMock {
    public class BaseWireMock {

        public WireMockServer MockServer { get; }
        public BaseWireMock() {
            MockServer ??= WireMockServer.Start();
        }
        public void ConfigureBuilder() {

            MockServer
                .Given(
                    Request.Create().WithPath($"/".Split('?')[0]).UsingGet()
                    )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                    );

            MockServer
                .Given(
                    Request.Create().WithPath($"/").UsingGet()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                );
        }
    }
}
