using LisAeroGest.Controllers;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace LisAeroGest.Tests
{
    public class CheckInControllerTests
    {
        [Fact]
        public async Task ProcessCheckIn_TicketNotFound_ReturnsRedirect()
        {
            // ARRANGE - Preparar o cenário

            int ticketId = 999;

            var ticketRepository = new Mock<ITicketRepository>();

            ticketRepository
                .Setup(r => r.GetTicketWithDetailsAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            var boardingPassRepository =
                new Mock<IBoardingPassRepository>();

            var controller = new CheckInController(
                ticketRepository.Object,
                boardingPassRepository.Object,
                Mock.Of<IPassengerRepository>(),
                Mock.Of<IUserHelper>(),
                null!,
                Mock.Of<IQrCodeService>(),
                Mock.Of<IFlightRepository>(),
                Mock.Of<IConverterHelper>(),
                Mock.Of<INotificationRepository>(),
                Mock.Of<IMailHelper>()
            );

            var httpContext = new DefaultHttpContext();

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Role, "Employee")
                    },
                    "TestAuthentication"
                )
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );

            // ACT - Executar o método

            var result = await controller.ProcessCheckIn(ticketId);

            // ASSERT - Verificar o resultado

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(
                nameof(CheckInController.EmployeeCheckIn),
                redirect.ActionName
            );

            Assert.Equal(
                "Bilhete não encontrado.",
                controller.TempData["Error"]
            );

            ticketRepository.Verify(
                r => r.GetTicketWithDetailsAsync(ticketId),
                Times.Once()
            );

            boardingPassRepository.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task ProcessCheckIn_TicketNotPaid_RedirectsWithError()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = 1,
                Status = "Reserved"
            };

            var ticketRepository = new Mock<ITicketRepository>();

            ticketRepository
                .Setup(r => r.GetTicketWithDetailsAsync(1))
                .ReturnsAsync(ticket);

            var boardingPassRepository = new Mock<IBoardingPassRepository>();

            boardingPassRepository
                .Setup(r => r.GetByTicketIdAsync(1))
                .ReturnsAsync((BoardingPass?)null);

            var controller = new CheckInController(
                ticketRepository.Object,
                boardingPassRepository.Object,
                Mock.Of<IPassengerRepository>(),
                Mock.Of<IUserHelper>(),
                null!,
                Mock.Of<IQrCodeService>(),
                Mock.Of<IFlightRepository>(),
                Mock.Of<IConverterHelper>(),
                Mock.Of<INotificationRepository>(),
                Mock.Of<IMailHelper>()
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                    new Claim(ClaimTypes.Role, "Employee")
                        },
                        "Test"
                    )
                )
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );

            // Act
            var result = await controller.ProcessCheckIn(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("EmployeeCheckIn", redirect.ActionName);

            Assert.Equal(
                "Este bilhete não está pago ou já tem check-in feito.",
                controller.TempData["Error"]
            );

            boardingPassRepository.Verify(
                r => r.AddAsync(It.IsAny<BoardingPass>()),
                Times.Never()
            );
        }


        [Fact]
        public async Task ProcessCheckIn_AlreadyCheckedIn_ReturnsExistingBoardingPass()
        {
            // ARRANGE
            int ticketId = 10;

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = "CheckedIn"
            };

            var existingBoardingPass = new BoardingPass
            {
                Id = 50,
                TicketId = ticketId
            };

            var ticketRepository = new Mock<ITicketRepository>();

            ticketRepository
                .Setup(r => r.GetTicketWithDetailsAsync(ticketId))
                .ReturnsAsync(ticket);

            var boardingPassRepository =
                new Mock<IBoardingPassRepository>();

            boardingPassRepository
                .Setup(r => r.GetByTicketIdAsync(ticketId))
                .ReturnsAsync(existingBoardingPass);

            var controller = new CheckInController(
                ticketRepository.Object,
                boardingPassRepository.Object,
                Mock.Of<IPassengerRepository>(),
                Mock.Of<IUserHelper>(),
                null!,
                Mock.Of<IQrCodeService>(),
                Mock.Of<IFlightRepository>(),
                Mock.Of<IConverterHelper>(),
                Mock.Of<INotificationRepository>(),
                Mock.Of<IMailHelper>()
            );

            var httpContext = new DefaultHttpContext();

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                new Claim(ClaimTypes.Role, "Employee")
                    },
                    "TestAuthentication"
                )
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );

            // ACT
            var result = await controller.ProcessCheckIn(ticketId);

            // ASSERT
            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(
                nameof(CheckInController.Confirmation),
                redirect.ActionName
            );

            Assert.Equal(
                50,
                Assert.IsType<int>(
                    redirect.RouteValues!["boardingPassId"]
                )
            );

            boardingPassRepository.Verify(
                r => r.GetByTicketIdAsync(ticketId),
                Times.Once()
            );

            boardingPassRepository.Verify(
                r => r.AddAsync(It.IsAny<BoardingPass>()),
                Times.Never()
            );

            boardingPassRepository.Verify(
                r => r.SaveAsync(),
                Times.Never()
            );
        }
    }
}