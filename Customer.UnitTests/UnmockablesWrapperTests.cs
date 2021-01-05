using HttpManager;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Customer.UnitTests
{
    public class UnmockablesWrapperTests
    {

        [Fact]
        public async Task GetAccessToken_AccessTokenIsNull_ReturnsNull()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var tokenResponse = new TokenResponse();

            //Act
            var result = await wrapper.GetAccessToken(tokenResponse);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAccessToken_TokenResponseIsNull_ReturnsEmptyString()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var emptyString = "";

            //Act
            var result = await wrapper.GetAccessToken(null);

            //Assert
            Assert.Equal(emptyString, result);
        }

        [Fact]
        public async Task GetTokenEndPoint_DiscoEndPointIsNull_ReturnsNull()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var disco = new DiscoveryDocumentResponse();

            //Act
            var result = await wrapper.GetTokenEndPoint(disco);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTokenEndPoint_DiscoIsNull_ReturnsEmptyString()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var emptyString = "";

            //Act
            var result = await wrapper.GetTokenEndPoint(null);

            //Assert
            Assert.Equal(emptyString, result);
        }

        [Fact]
        public async Task GetDiscoveryDoc_ReturnsDisco()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var client = new HttpClient();
            var url = "https://test_url";

            //Act
            var result = wrapper.GetDiscoveryDocumentAsync(client,url);

            //Assert
            Assert.NotNull(result);
            var disco = await result as DiscoveryDocumentResponse;
            Assert.NotNull(disco);
        }

        [Fact]
        public async Task GetDiscoveryDoc_NullClient_ReturnsDisco()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var url = "https://test_url";

            //Act
            var result = wrapper.GetDiscoveryDocumentAsync(null, url);

            //Assert
            Assert.NotNull(result);
            var disco = await result as DiscoveryDocumentResponse;
            Assert.NotNull(disco);
        }

        [Fact]
        public async Task GetDiscoveryDoc_NullUrl_ReturnsDisco()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var client = new HttpClient();

            //Act
            var result = wrapper.GetDiscoveryDocumentAsync(client, null);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDiscoveryDoc_EmptyUrl_ReturnsDisco()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var client = new HttpClient();

            //Act
            var result = wrapper.GetDiscoveryDocumentAsync(client, "");

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDiscoveryDoc_BothNull_ReturnsDisco()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();

            //Act
            var result = wrapper.GetDiscoveryDocumentAsync(null, null);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetClientCredentialsToken_ReturnsTokenResponse()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var client = new HttpClient();
            var tokenRequest = new ClientCredentialsTokenRequest();

            //Act
            var result = wrapper.RequestClientCredentialsTokenAsync(client, tokenRequest);

            //Assert
            Assert.NotNull(result);
            var response = await result as TokenResponse;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetClientCredentialsToken_NullClient_ReturnsTokenResponse()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var tokenRequest = new ClientCredentialsTokenRequest();

            //Act
            var result = wrapper.RequestClientCredentialsTokenAsync(null, tokenRequest);

            //Assert
            Assert.NotNull(result);
            var response = await result as TokenResponse;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetClientCredentialsToken_NullRequest_ReturnsTokenResponse()
        {
            //Arrange
            var wrapper = new UnmockablesWrapper();
            var client = new HttpClient();

            //Act
            var result = wrapper.RequestClientCredentialsTokenAsync(client, null);

            //Assert
            Assert.Null(result);
        }
    }
}
