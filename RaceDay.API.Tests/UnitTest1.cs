using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay_.API.Data;
using Xunit;

namespace RaceDay.API.Tests
{
    public class UnitTest1
    {
        private static WebApplicationFactory<Program> CreateTestFactory()
        {
            var databaseName = "RaceDayTests_" +
                System.Guid.NewGuid().ToString();

            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["Jwt:Key"] =
                                    "RaceDayTestingKey2026!SecureKey123456",
                                ["Jwt:Issuer"] = "RaceDay.API",
                                ["Jwt:Audience"] = "RaceDay.Client"
                            });
                    });

                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<RaceDayDbContext>();
                        services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

                        services.AddDbContext<RaceDayDbContext>(options =>
                        {
                            options.UseInMemoryDatabase(databaseName);
                        });

                        services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = "Test";
                            options.DefaultChallengeScheme = "Test";
                            options.DefaultForbidScheme = "Test";
                        })
                        .AddScheme<
                            AuthenticationSchemeOptions,
                            TestAuthHandler>(
                            "Test",
                            options => { });
                    });
                });
        }

        [Fact]
        public async Task UsersEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            using var factory = CreateTestFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/Users");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task EventsEndpoint_WithoutToken_ReturnsSuccess()
        {
            using var factory = CreateTestFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/Events");

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Participant_CannotCreateEvent_ReturnsForbidden()
        {
            using var factory = CreateTestFactory();
            using var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-Role",
                "Participant");

            var response = await client.PostAsJsonAsync(
                "/api/Events",
                new
                {
                    organiserId = 6,
                    title = "Test Event",
                    description = "Automated test event",
                    location = "Polokwane",
                    eventDate = "2027-06-01T08:00:00"
                });

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Organiser_WithInvalidOrganiserId_ReturnsBadRequest()
        {
            using var factory = CreateTestFactory();
            using var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-Role",
                "Organiser");

            var response = await client.PostAsJsonAsync(
                "/api/Events",
                new
                {
                    organiserId = 999999,
                    title = "Automated Test Event",
                    description = "Test only",
                    eventDate = "2027-06-01T08:00:00",
                    location = "Polokwane",
                    imageUrl = (string?)null
                });

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Register_WithValidDetails_ReturnsCreated()
        {
            using var factory = CreateTestFactory();
            using var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    email = "new.test@raceday.co.za",
                    password = "TestPassword2026!",
                    fullName = "New Test User",
                    role = "Participant"
                });

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_AfterRegistration_ReturnsSuccess()
        {
            using var factory = CreateTestFactory();
            using var client = factory.CreateClient();

            var registrationResponse = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    email = "login.test@raceday.co.za",
                    password = "TestPassword2026!",
                    fullName = "Login Test User",
                    role = "Participant"
                });

            Assert.True(
                registrationResponse.StatusCode == HttpStatusCode.Created,
                $"Registration failed. Status: {(int)registrationResponse.StatusCode}. " +
                $"Response: {await registrationResponse.Content.ReadAsStringAsync()}");

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "login.test@raceday.co.za",
                    password = "TestPassword2026!"
                });

            Assert.True(
                loginResponse.StatusCode == HttpStatusCode.OK,
                $"Login failed. Status: {(int)loginResponse.StatusCode}. " +
                $"Response: {await loginResponse.Content.ReadAsStringAsync()}");

            var loginData =
                await loginResponse.Content.ReadFromJsonAsync<LoginResult>();

            Assert.NotNull(loginData);
            Assert.False(string.IsNullOrWhiteSpace(loginData.Token));
        }

        private class LoginResult
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}