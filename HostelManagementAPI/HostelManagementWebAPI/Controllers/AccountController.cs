
using DTOs;
using DTOs.Account;
using DTOs.AccountAuthentication;
using DTOs.Enum;
using HostelManagementWebAPI.MessageStatusResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interface;

namespace HostelManagementWebAPI.Controllers
{
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize]
        [HttpGet("check/token/expire")]
        public async Task<ActionResult> CheckIsTokenExpire()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AccountLoginDto>> Login(EmailLoginDto login)
        {

            try
            {
                var user = await _accountService.Login(login);

                if (user == null)
                {
                    return Unauthorized(new ApiResponseStatus(401, "Wrong password"));
                }
                return Ok(user);
            }
            catch (ServiceException ex)
            {
                return Unauthorized(new ApiResponseStatus(401, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost("register/email")]
        public async Task<ActionResult> Register(EmailRegisterDto emailRegisterDto)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(new ApiResponseStatus(400, "Invalid format"));
            //}

            try
            {
                await _accountService.RegisterEmail(emailRegisterDto);
                return Ok();
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost("otp/confirm")]
        public async Task<ActionResult> ConfirmAccount(AccountConfirmDto accountConfirmDto)
        {
            try
            {
                await _accountService.ConfirmOtp(accountConfirmDto);
                return Ok();
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpGet("otp/resend")]
        public async Task<ActionResult> ResendOtp(string email)
        {
            try
            {
                await _accountService.ResendRegisterOtp(email);
                return Ok();
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpGet("forget/password")]
        public async Task<ActionResult> ForgetPassword(string email)
        {
            try
            {
                await _accountService.ForgetPassword(email);
                return Ok();
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost("confirm/password")]
        public async Task<ActionResult<AccountLoginDto>> ConfirmPassword(ConfirmPasswordDtos confirmPasswordDtos)
        {
            try
            {
                var user = await _accountService.ConfirmPassword(confirmPasswordDtos);
                return Ok(user);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost("login/google")]
        public async Task<ActionResult<AccountLoginDto>> LoginWithGoogle(LoginWithGoogleDto loginWithGoogle)
        {
            try
            {
                var user = await _accountService.LoginWithGoogle(loginWithGoogle);
                if (user == null)
                {
                    return Unauthorized(new ApiResponseStatus(401, "No account associate with this email"));
                }
                return Ok(user);
            }
            catch (ServiceException ex)
            {
                return Unauthorized(new ApiResponseStatus(401, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost("register/google")]
        public async Task<ActionResult<AccountLoginDto>> RegisterWithGoogle(RegisterWithGoogleDto register)
        {
            try
            {
                var user = await _accountService.RegisterWithGoogle(register);
                return Ok(user);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<ActionResult<AccountLoginDto>> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest(new ApiResponseStatus(400, "Invalid client request"));

            try
            {
                var accountDto = await _accountService.RefreshToken(tokenApiDto);
                if (accountDto == null)
                {
                    return BadRequest(new ApiResponseStatus(400, "Invalid token"));
                }
                return Ok(accountDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }


        }

        [HttpPost, Authorize]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                int accountId = GetLoginAccountId();
                await _accountService.Logout(accountId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }

        }

        [Authorize(policy: "Member")]
        [HttpGet("member/profile/{accountID}")]
        public async Task<ActionResult<CustomerViewAccount>> GetAccountDetailById(int accountID)
        {
            try
            {
                var account = await _accountService.GetAccountProfileById(accountID);
                if (account == null)
                {
                    return NotFound(new ApiResponseStatus(404, "Account not found"));
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        [Authorize(policy: "Member")]
        [HttpPost("member/profile/update")]
        public async Task<IActionResult> UpdateMemberProfile(AccountUpdate accountUpdate)
        {
            try
            {
                await _accountService.UpdateOwnerProfile(accountUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(policy: "Member")]
        [HttpPost("member/password/update")]
        public async Task<IActionResult> UpdateMemberPassword(ChangePassword newPassword)
        {
            try
            {
                await _accountService.UpdateOwnerPassword(newPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(404, ex.Message));
            }
        }

        [Authorize(policy: "Member")]
        [HttpPost("member/password/get-old-password")]
        public async Task<IActionResult> GetMemberOldPassword(ChangePassword oldPassword)
        {
            try
            {
                await _accountService.GetOldPassword(oldPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(404, ex.Message));
            }
        }



        [Authorize(policy: "Owner")]
        [HttpGet("owner/profile/{accountID}")]
        public async Task<IActionResult> GetProfileOwner(int accountID)
        {
            var inf = await _accountService.GetProfileAccount(accountID);
            return Ok(inf);
        }

        [Authorize(policy: "Owner")]
        [HttpGet("owner/profile/detail/{accountID}")]
        public async Task<IActionResult> GetProfileDetailOwner(int accountID)
        {
            var inf = await _accountService.GetAccountById(accountID);
            return Ok(inf);
        }

        [Authorize(policy: "Owner")]
        [HttpPost("owner/profile/update")]
        public async Task<IActionResult> UpdateOwnerProfile(AccountUpdate accountUpdate)
        {
            try
            {
                await _accountService.UpdateOwnerProfile(accountUpdate);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(policy: "Owner")]
        [HttpPost("owner/password/update")]
        public async Task<IActionResult> UpdateOwnerPassword(ChangePassword newPassword)
        {
            try
            {
                await _accountService.UpdateOwnerPassword(newPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(404, ex.Message));
            }
        }

        [Authorize(policy: "Owner")]
        [HttpPost("owner/password/get-old-password")]
        public async Task<IActionResult> GetOldPassword(ChangePassword oldPassword)
        {
            try
            {
                await _accountService.GetOldPassword(oldPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseStatus(404, ex.Message));
            }
        }

        [HttpGet("check/status/{accountID}")]
        public async Task<IActionResult> CheckStatusAccount(int accountID)
        {
            try
            {
                var account = await _accountService.GetAccountById(accountID);
                if(account == null)
                {
                    return BadRequest(new ApiResponseStatus(404, "Invalid account"));
                }
                else if(account.Status == (int)AccountStatusEnum.Inactive){
                    return BadRequest(new ApiResponseStatus(404, "Invalid account"));
                }
                else
                {
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new ApiResponseStatus(404, "Invalid account"));
            }
        }
    }
}
