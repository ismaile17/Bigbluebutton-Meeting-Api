using Application.TransferPaymentAndInvoice.Commands;
using Application.TransferPaymentAndInvoice.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class PurchasesListAndPaymentController : BaseController
    {
        [HttpGet("/api/payment/purchaseslist")]
        public async Task<IActionResult> GetPurchasesList()
        {
            //int appUseRId = 2;
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetPurchasesListByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/payment/moneytransferlistforadmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMoneyTransferList()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetMoneyTransferListForAdminQuery { AppUserId = appUseRId }));
        }


        [HttpPost("/api/payment/invoicecreateforadmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateInvoiceBeenForAdminCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/payment/invocecreatelistforadmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInvoiceBeenCreateList()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new InvoiceBeenCreateListForAdminQuery { AppUserId = appUseRId }));
        }


        [HttpPost("/api/payment/moneytransfer")]
        public async Task<IActionResult> Add([FromBody] CreateMoneyTransferCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpPost("/api/payment/addmoneytransferorgift")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreatePackageDefinitionForAdminCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }


    }
}
