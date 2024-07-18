using AutoMapper;
using DTOs;
using DTOs.Account;
using DTOs.AccountAuthentication;
using DTOs.Enum;
using DTOs.MemberShipRegisterTransaction;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using Repository.Interface;
using Service.Exceptions;
using Service.Interface;
using Service.Mail;
using System.Security.Cryptography;
using System.Text;


namespace Service.Implement
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;

        public AccountService(IAccountRepository accountRepository, ITokenService tokenService, IMapper mapper, IMailService mailService)
        {
            _accountRepository = accountRepository; _tokenService = tokenService; _mapper = mapper;
            _mailService = mailService;
            //_cache = cache;
        }

        private AccountDto AdminLogin(EmailLoginDto emailLoginDto)
        {
            var adminEmail = ConfigurationHelper.config.GetSection("AdminAccount:Email").Value;
            var adminPassword = ConfigurationHelper.config.GetSection("AdminAccount:Password").Value;

            if (emailLoginDto.Email.Equals(adminEmail) && emailLoginDto.Password == (adminPassword))
            {
                var accountDto = new AccountDto
                {
                    AccountId = 0,
                    Email = emailLoginDto.Email,
                    Name = "System Admin",
                    RoleId = (int)AccountRoleEnum.Admin
                };
                accountDto.Token = _tokenService.CreateToken(accountDto);

                return accountDto;
            }

            return null;
        }



        public async Task<IEnumerable<AccountViewDto>> GetAllAccounts()
        {
            return _accountRepository.GetAllAsync().Result.Select(x => new AccountViewDto
            {
                AccountId = x.AccountId,
                Email = x.Email,
                Name = x.Name,
                Status = x.Status
            }).ToList();
        }

        public async Task<AccountLoginDto> Login(EmailLoginDto login)
        {
            var adminAccount = AdminLogin(login);
            if (adminAccount != null)
            {



                return _mapper.Map<AccountLoginDto>(adminAccount);
            }


            AccountDto accountDto = await _accountRepository.GetAccountByEmail(login.Email);

            if (accountDto != null)
            {
                if ((bool)accountDto.IsLoginWithGmail)
                {
                    throw new ServiceException("The account is created by login with Google, please use login with Google");
                }

                if (accountDto.Status == (int)AccountStatusEnum.Register_But_Not_Confirm)
                {
                    throw new ServiceException("Your account has not been confirm yet, please navigate to the register page again");
                }

                if (accountDto.Status == (int)AccountStatusEnum.Inactive)
                {
                    throw new ServiceException("Your account has been deem inactive, please contact the admin for further instructions");
                }




                using var hmac = new HMACSHA512(accountDto.PasswordSalt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != accountDto.PasswordHash[i])
                    {
                        throw new ServiceException("Wrong password");
                    }
                }

                accountDto.Token = _tokenService.CreateToken(accountDto);

                var refreshToken = _tokenService.GenerateRefreshToken();
                accountDto.RefreshToken = refreshToken;
                accountDto.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                if (login.FirebaseRegisterToken.IsNullOrEmpty())
                {

                }
                else if (!login.FirebaseRegisterToken.Equals(accountDto.FirebaseToken))
                {
                    var firebaseTokenExistedAccount = await _accountRepository.FirebaseTokenExisted(login.FirebaseRegisterToken);
                    if (firebaseTokenExistedAccount != null)
                    {
                        firebaseTokenExistedAccount.FirebaseToken = null;
                        await _accountRepository.UpdateAccount(firebaseTokenExistedAccount);
                    }
                    accountDto.FirebaseToken = login.FirebaseRegisterToken;
                }
                await _accountRepository.UpdateAccount(accountDto);

                return _mapper.Map<AccountLoginDto>(accountDto);

            }
            throw new ServiceException("No account associate with this email");
        }

        public async Task ConfirmOtp(AccountConfirmDto accountConfirmDto)
        {
            AccountDto accountDto = await _accountRepository.GetAccountByEmail(accountConfirmDto.Email);
            if (accountDto != null && accountDto.Status == (int)AccountStatusEnum.Register_But_Not_Confirm && (bool)!accountDto.IsLoginWithGmail)
            {
                if (accountDto.OtpToken != accountConfirmDto.OtpToken)
                {
                    throw new ServiceException("Wrong Otp Code");
                }

                accountDto.Status = (int)AccountStatusEnum.Active;

                await _accountRepository.UpdateAccount(accountDto);
            }
            else
            {
                throw new ServiceException("The email does not need this function");
            }
        }

        public async Task ResendRegisterOtp(string email)
        {
            AccountDto accountDto = await _accountRepository.GetAccountByEmail(email);
            if (accountDto != null && accountDto.Status == (int)AccountStatusEnum.Register_But_Not_Confirm && (bool)!accountDto.IsLoginWithGmail)
            {
                Random random = new Random();
                var otp = random.Next(111111, 999999).ToString();

                accountDto.OtpToken = otp;

                await _accountRepository.UpdateAccount(accountDto);

                _mailService.SendMail(SendAccountPassword.SendInitPassword(email, otp));
            }
            else
            {
                throw new ServiceException("The email does not need this function");
            }
        }


        public async Task RegisterEmail(EmailRegisterDto emailRegisterDto)
        {
            AccountDto accountDto = await _accountRepository.GetAccountByEmail(emailRegisterDto.Email);
            if (accountDto != null && accountDto.Status != (int)AccountStatusEnum.Register_But_Not_Confirm)
            {
                throw new ServiceException("This email has already been used. Please choose other email");
            }

            if (accountDto != null)
            {
                if (accountDto.Status == (int)AccountStatusEnum.Register_But_Not_Confirm)
                {
                    await _accountRepository.RemoveAccount(accountDto);
                }
            }

            Random random = new Random();
            var otp = random.Next(111111, 999999).ToString();


            using var hmac = new HMACSHA512();
            AccountDto newAccount = new AccountDto
            {
                Email = emailRegisterDto.Email,
                Name = emailRegisterDto.Name,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(emailRegisterDto.Password)),
                PasswordSalt = hmac.Key,
                CreatedDate = DateTime.Now,
                Status = (int)AccountStatusEnum.Register_But_Not_Confirm,
                RoleId = emailRegisterDto.RoleId,
                IsLoginWithGmail = false,
                OtpToken = otp,
                Address = emailRegisterDto.Address,
                Phone = emailRegisterDto.Phone,
                CitizenCard = emailRegisterDto.CitizenCard
            };

            if (newAccount.RoleId == (int)AccountRoleEnum.Owner)
            {
                newAccount.PackageStatus = (int)AccountPackageStatusEnum.Inactive;
            }

            await _accountRepository.CreateAccount(newAccount);

            _mailService.SendMail(SendAccountPassword.SendInitPassword(emailRegisterDto.Email, otp));
        }

        public async Task ForgetPassword(string email)
        {
            AccountDto accountDto = await _accountRepository.GetAccountByEmail(email);
            if (accountDto == null)
            {
                throw new ServiceException("No account associate with this email");
            }

            if ((bool)accountDto.IsLoginWithGmail == true)
            {
                throw new ServiceException("Your account was registered using gmail service, there is no password");
            }

            if (accountDto.Status == (int)AccountStatusEnum.Register_But_Not_Confirm)
            {
                throw new ServiceException("Your account has not been confirm yet, please navigate to the register page again");
            }

            Random random = new Random();
            var otp = random.Next(111111, 999999).ToString();

            accountDto.OtpToken = otp;

            await _accountRepository.UpdateAccount(accountDto);

            //send mail here for the passwords
            _mailService.SendMail(SendAccountPassword.SendInitPassword(email, otp));
        }

        public async Task<AccountLoginDto> ConfirmPassword(ConfirmPasswordDtos confirmPasswordDtos)
        {
            AccountDto accountDto = await _accountRepository.GetAccountByEmail(confirmPasswordDtos.Email);
            if (accountDto != null)
            {
                if ((bool)accountDto.IsLoginWithGmail)
                {
                    throw new ServiceException("Your account was registered using gmail service, there is no password");
                }
                if (!confirmPasswordDtos.OtpToken.Equals(accountDto.OtpToken))
                {
                    throw new ServiceException("Wrong Otp Code");
                }

                using var hmac = new HMACSHA512();

                accountDto.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(confirmPasswordDtos.NewPassword));
                accountDto.PasswordSalt = hmac.Key;
                accountDto.Status = (int?)AccountStatusEnum.Active; // fix later
                try
                {
                    await _accountRepository.UpdateAccount(accountDto);
                }
                catch (Exception ex)
                {
                    throw new ServiceException("Cannot update the password");
                }
                accountDto.Token = _tokenService.CreateToken(accountDto);

                return _mapper.Map<AccountLoginDto>(accountDto);
            }
            else throw new ServiceException("No account associate with this email");
        }

        public async Task<AccountLoginDto> LoginWithGoogle(LoginWithGoogleDto loginWithGoogle)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(loginWithGoogle.IdTokenString);

            string userEmail = payload.Email;

            AccountDto accountDto = await _accountRepository.GetAccountByEmail(userEmail);
            if (accountDto != null)
            {
                if ((bool)!accountDto.IsLoginWithGmail)
                {
                    throw new ServiceException("This email has already been used and associated with an password, please choose other login method");
                }
                accountDto.Token = _tokenService.CreateToken(accountDto);
                var refreshToken = _tokenService.GenerateRefreshToken();
                accountDto.RefreshToken = refreshToken;
                accountDto.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                await _accountRepository.UpdateAccount(accountDto);

                return _mapper.Map<AccountLoginDto>(accountDto);
            }
            return null;
        }

        public async Task<AccountLoginDto> RegisterWithGoogle(RegisterWithGoogleDto registerWithGoogle)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(registerWithGoogle.IdTokenString);

            string userEmail = payload.Email;

            AccountDto accountDto = await _accountRepository.GetAccountByEmail(userEmail);
            if (accountDto != null)
            {
                throw new ServiceException("This email has already been used. Please choose other email");
            }

            AccountDto newAccount = new AccountDto
            {
                Email = payload.Email,
                Name = payload.Name,
                RoleId = registerWithGoogle.RoleId,
                CreatedDate = DateTime.Now,
                IsLoginWithGmail = true
            };


            var refreshToken = _tokenService.GenerateRefreshToken();
            newAccount.RefreshToken = refreshToken;
            newAccount.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            if (newAccount.RoleId == (int)AccountRoleEnum.Owner)
            {
                newAccount.PackageStatus = (int)AccountPackageStatusEnum.Inactive;
            }

            await _accountRepository.CreateAccount(newAccount);

            accountDto.Token = _tokenService.CreateToken(accountDto);
            return _mapper.Map<AccountLoginDto>(accountDto);
        }

        public async Task Logout(int accountId)
        {
            var account = await _accountRepository.GetAccountById(accountId);

            account.RefreshToken = null;
            account.RefreshTokenExpiryTime = DateTime.MinValue;

            await _accountRepository.UpdateAccount(account);
        }


        public async Task ActiveAccount(int idAccount)
        {
            var account = _accountRepository.GetAccountById(idAccount);
            account.Result.Status = 0;
            await _accountRepository.UpdateAccount(account.Result);
        }

        public async Task UnactiveAccount(int idAccount)
        {
            var account = _accountRepository.GetAccountById(idAccount);
            account.Result.Status = 1;
            await _accountRepository.UpdateAccount(account.Result);
        }

        public async Task<AccountViewDetail> GetAccountById(int id)
        {
            var account = await _accountRepository.GetAccountById(id);
            AccountViewDetail accountViewDetail = _mapper.Map<AccountViewDetail>(account);
            return accountViewDetail;
        }

        public async Task<AccountLoginDto> RefreshToken(TokenApiDto tokenApiDto)
        {
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

            var accountId = int.Parse(principal.Claims.First(i => i.Type == "AccountId").Value);

            var accountDto = await _accountRepository.GetAccountById(accountId);

            if (accountDto is null || accountDto.RefreshToken != refreshToken || accountDto.RefreshTokenExpiryTime <= DateTime.Now)
                return null;

            var newAccessToken = _tokenService.CreateToken(principal.Claims);
            //var newRefreshToken = _tokenService.GenerateRefreshToken();

            accountDto.Token = newAccessToken;
            //accountDto.RefreshToken = newRefreshToken;

            await _accountRepository.UpdateAccount(accountDto);

            return _mapper.Map<AccountLoginDto>(accountDto);
        }

        public async Task<CustomerViewAccount> GetAccountProfileById(int id)
        {
            var account = await _accountRepository.GetAccountProfileById(id);
            CustomerViewAccount customerViewAccount = _mapper.Map<CustomerViewAccount>(account);
            return customerViewAccount;
        }

        public async Task<IEnumerable<ViewMemberShipDto>> GetAllMemberShip()
        {
            return await _accountRepository.GetAllMemberShip();
        }

        public async Task<AccountMemberShipInformationDtos> GetDetailMemberShipRegisterInformation(int accountid)
        {
            return await _accountRepository.GetDetailMemberShipRegisterInformation(accountid);
        }

        public async Task UpdateAccountPackageStatus(int accountId, int status)
        {
            var account = await _accountRepository.GetAccountById(accountId);

            account.PackageStatus = status;

            await _accountRepository.UpdateAccount(account);
        }

        public async Task<ProfileDto> GetProfileAccount(int accountID)
        {
            return await _accountRepository.GetProfileAccount(accountID);
        }

        public async Task UpdateOwnerProfile(AccountUpdate accountUpdate)
        {
            await _accountRepository.UpdateOwnerProfile(accountUpdate);
        }

        public async Task UpdateOwnerPassword(ChangePassword newPassword)
        {
            var account = await _accountRepository.GetAccountById(newPassword.AccountID);
            using var hmac = new HMACSHA512();

            account.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(newPassword.Password));
            account.PasswordSalt = hmac.Key;
            try
            {
                await _accountRepository.UpdateAccount(account);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Cannot update the password");
            }
        }

        public async Task GetOldPassword(ChangePassword oldPassword)
        {
            var account = await _accountRepository.GetAccountById(oldPassword.AccountID);
            using var hmac = new HMACSHA512(account.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(oldPassword.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != account.PasswordHash[i])
                {
                    throw new Exception("Old password do not equal your password!");
                }
            }
        }

        public async Task DeleteAcount(int accountID)
        {
            try
            {
                await _accountRepository.RemoveAccountInDB(accountID);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
