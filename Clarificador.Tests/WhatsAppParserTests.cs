using Xunit;
using Clarificador.Api.Services; // Lo crearemos en un momento

namespace Clarificador.Tests
{
    public class WhatsAppParserTests
    {
        [Fact]
        public void ExtraerEnlace_DeJsonDeWhatsApp_RetornaUrlDeTikTok()
        {
            // 1. Arrange
            var jsonPayload = @"{
                ""object"": ""whatsapp_business_account"",
                ""entry"": [{
                    ""changes"": [{
                        ""value"": {
                            ""messages"": [{
                                ""from"": ""5215512345678"",
                                ""text"": {
                                    ""body"": ""Checa este video https://vt.tiktok.com/ZS9krTvWjmo2n-hvoqI/""
                                }
                            }]
                        }
                    }]
                }]
            }";

            var parser = new WhatsAppMessageParser();

            // 2. Act
            var enlaceExtraido = parser.ExtraerEnlaceDeMensaje(jsonPayload);

            // 3. Assert
            Assert.Equal("https://vt.tiktok.com/ZS9krTvWjmo2n-hvoqI/", enlaceExtraido);
        }
    }
}