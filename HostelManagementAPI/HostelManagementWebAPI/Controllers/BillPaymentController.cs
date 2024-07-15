using DTOs.BillPayment;
using DTOs.Enum;
using HostelManagementWebAPI.MessageStatusResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service.Exceptions;
using Service.Interface;
using Service.Vnpay;

namespace HostelManagementWebAPI.Controllers
{

    public class BillPaymentController : BaseApiController
    {
        private readonly IBillPaymentService _billPaymentService;
        private readonly IContractService _contractService;
        private readonly IVnpayService _vnpayService;
        private readonly VnPayProperties _vnPayProperties;

        public BillPaymentController(IBillPaymentService billPaymentService, IContractService contractService, IOptions<VnPayProperties> vnPayProperties, IVnpayService vnpayService)
        {
            _billPaymentService = billPaymentService;
            _contractService = contractService;
            _vnPayProperties = vnPayProperties.Value;
            _vnpayService = vnpayService;
        }

        [HttpGet("bill-payment/last-month-bills/{ownerId}")]
        public async Task<ActionResult> GetLastMonthBillPaymentsByOwnerId(int ownerId)
        {
            try
            {
                var result = await _billPaymentService.GetLastMonthBillPaymentsByOwnerId(ownerId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [HttpPost("bill-payment/monthly")]
        public async Task<ActionResult> CreateBillPaymentMonthly([FromBody] CreateBillPaymentRequestDto createBillPaymentRequestDto)
        {
            try
            {
                await _billPaymentService.CreateBillPaymentMonthly(createBillPaymentRequestDto);
                return Ok(new ApiResponseStatus(Ok().StatusCode, "Create new monthly bill successfully"));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [HttpGet("bill-payment/last-month-bill/{contractId}")]
        public async Task<ActionResult> GetLastBillPayment(int contractId)
        {
            try
            {
                var result = await _billPaymentService.GetLastMonthBillPayment(contractId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [HttpGet("bill-payment/contract/{contractId}")]
        public async Task<ActionResult> GetBillPaymentsByContract(int contractId)
        {
            try
            {
                var result = await _billPaymentService.GetBillPaymentsByContractId(contractId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [HttpGet("bill-payment/{billPaymentId}")]
        public async Task<ActionResult> GetBillPaymentDetail(int billPaymentId)
        {
            try
            {
                var result = await _billPaymentService.GetBillPaymentDetail(billPaymentId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(Policy = "Member")]
        [HttpPost("bill-payment/deposit")]
        public async Task<ActionResult> DepositRoom(DepositRoomInputDto depositRoomInputDto)
        {
            int accountId = GetLoginAccountId();
            try
            {
                var contract = await _contractService.GetContractDetailByContractId(depositRoomInputDto.ContractId);

                if (contract == null || contract?.Status != (int)ContractStatusEnum.pending || contract?.DepositFee == null || contract?.StudentAccountID != accountId)
                {
                    throw new ServiceException("The contract is not suitable for deposited");
                }

                var billPayment = await _billPaymentService.CreateDepositPayment(depositRoomInputDto, accountId);

                string paymentUrl = _vnpayService.CreateVnpayPaymentLink(billPayment.TnxRef, (double)billPayment.TotalAmount, depositRoomInputDto.ReturnUrl, "Room deposit", _vnPayProperties);

                return Ok(new
                {
                    paymentUrl,
                    billPayment
                });
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(Policy = "Member")]
        [HttpPost("bill-payment/pay/monthly")]
        public async Task<ActionResult> MonthlyBillPayment(MonthlyBillPaymentInputDto monthlyBillPaymentInputDto)
        {
            int accountId = GetLoginAccountId();
            try
            {
                var billPayment = await _billPaymentService.PrepareBillingForMonthlyPayment(monthlyBillPaymentInputDto.BillPaymentId, accountId);

                string paymentUrl = _vnpayService.CreateVnpayPaymentLink(billPayment.TnxRef, (double)billPayment.TotalAmount, monthlyBillPaymentInputDto.ReturnUrl, "Monthly bill payment", _vnPayProperties);

                return Ok(new
                {
                    paymentUrl,
                    billPayment
                });
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(Policy = "Member")]
        [HttpGet("bill-payment/payment-history")]
        public async Task<ActionResult> GetPaymentHistory()
        {
            int accountId = GetLoginAccountId();
            try
            {
                var result = await _billPaymentService.GetPaymentHistoryByMemberAccount(accountId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(Policy = "Owner")]
        [HttpGet("owner/bill-payment/payment-history")]
        public async Task<ActionResult> GetPaymentHistoryOwner()
        {
            int accountId = GetLoginAccountId();
            try
            {
                var result = await _billPaymentService.GetPaymentHistoryByOwnerAccount(accountId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(Policy = "Member")]
        [HttpGet("bill-payment/monthly")]
        public async Task<ActionResult> GetBillMonthlyPayment()
        {
            int accountId = GetLoginAccountId();
            try
            {
                var result = await _billPaymentService.GetMonthlyBillPaymentForMember(accountId);
                return Ok(result);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(policy: "Owner")]
        [HttpGet("owner/get-old-number-electric-and-water/{roomID}")]
        public async Task<ActionResult> GetOldNumberServiceElectricAndWater(int roomID)
        {
            var numberService = await _billPaymentService.GetOldNumberServiceElectricAndWater(roomID);
            return Ok(numberService);
        }

        //[Authorize(Policy = "Member")]
        //[HttpPost("bill-payment/confirm-payment")]
        //public async Task<ActionResult> ConfirmRegisterPayment(VnPayReturnUrlDto vnPayReturnUrlDto)
        //{
        //    int accountId = GetLoginAccountId();

        //    try
        //    {
        //        if (!_vnpayService.ConfirmReturnUrl(vnPayReturnUrlDto.Url, vnPayReturnUrlDto.TnxRef, _vnPayProperties))
        //        {
        //            return BadRequest(new ApiResponseStatus(400, "The transaction is not valid"));
        //        }

        //        var billPayment = await _billPaymentService.ConfirmBillingTransaction(vnPayReturnUrlDto);

        //        if (billPayment.BillType == (int)BillType.Deposit)
        //        {
        //            await _contractService.ChangeContractStatus((int)billPayment.ContractId, (int)ContractStatusEnum.signed, DateTime.Now);
        //        }

        //        return Ok();
        //    }
        //    catch (ServiceException ex)
        //    {
        //        return BadRequest(new ApiResponseStatus(400, ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ApiResponseStatus(500, ex.Message));
        //    }
        //}
    }
}
