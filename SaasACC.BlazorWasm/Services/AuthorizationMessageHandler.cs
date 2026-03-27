using System.Net;

namespace SaacACC.BlazorWasm.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationMessageHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var authService = _serviceProvider.GetRequiredService<IAuthService>();
                await authService.Logout(sessionExpired: true);
            }

            return response;
        }
    }
}
