using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using FSharp.Control;
using Storczyk.Prodpol.Controllers.Data;
using Storczyk.Prodpol.Core.Data;
using Storczyk.Prodpol.Core.Models;
using Storczyk.Prodpol.Core.Utils.AsyncResult;

namespace Storczyk.Prodpol.Tests
{
    [TestFixture]
    public class EmployeesRolesControllerTests
    {
        [Test]
        public void GetAll_ReturnsOkWithRoles()
        {
            var role = new EmployeeRole { Id = 1, DisplayName = "Admin", RoleName = "admin" };

            var repoMock = new Mock<IEmployeeRoleRepository>();
            repoMock.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => FSharp.Core.Operators.AsyncSingleton(async () => Result.Ok(AsyncSeqModule.AsyncSeqSingleton(role))));

            var loggerMock = new Mock<ILogger<EmployeesRolesController>>();
            var controller = new EmployeesRolesController(repoMock.Object, loggerMock.Object);

            var result = controller.GetAll(CancellationToken.None).Result;

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = (OkObjectResult)result;
            var seq = (FSharp.Control.AsyncSeq<EmployeeRole>)ok.Value;
            var items = FSharp.Control.AsyncSeqModule.ToArray(seq).Result;
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("admin", items[0].RoleName);
        }

        [Test]
        public void GetById_ReturnsNotFound()
        {
            var repoMock = new Mock<IEmployeeRoleRepository>();
            repoMock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                .Returns((string id) => FSharp.Core.Operators.AsyncSingleton(async () => Result.Error(DatabaseError.NotFound)));

            var loggerMock = new Mock<ILogger<EmployeesRolesController>>();
            var controller = new EmployeesRolesController(repoMock.Object, loggerMock.Object);

            var result = controller.GetById("missing").Result;

            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
