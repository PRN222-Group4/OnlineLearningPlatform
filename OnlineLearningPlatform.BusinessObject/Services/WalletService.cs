using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Wallet;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClaimService _claimService;

        public WalletService(IUnitOfWork uow, IClaimService claimService)
        {
            _uow = uow;
            _claimService = claimService;
        }

        public async Task<ApiResponse> GetMyWalletAsync()
        {
            var response = new ApiResponse();
            try
            {
                var userId = _claimService.GetUserClaim().UserId;

                var wallet = await _uow.Wallets.GetAsync(
                    w => w.UserId == userId,
                    include: w => w.Include(x => x.WalletTransactions.OrderByDescending(t => t.CreatedAt))
                );

                if (wallet == null)
                {
                    return response.SetOk(new WalletResponse { Balance = 0, Transactions = new List<WalletTransactionResponse>() });
                }

                var result = new WalletResponse
                {
                    WalletId = wallet.WalletId,
                    Balance = wallet.Balance,
                    PendingBalance = wallet.PendingBalance,
                    TotalEarnings = wallet.TotalEarnings,
                    TotalWithdrawn = wallet.TotalWithdrawn,
                    Status = wallet.Status,
                    Transactions = wallet.WalletTransactions.Select(t => new WalletTransactionResponse
                    {
                        TransactionId = t.WalletTransactionId,
                        Amount = t.Amount,
                        TransactionType = t.TransactionType,
                        Description = t.Description ?? "No description",
                        BalanceAfterTransaction = t.BalanceAfterTransaction,
                        CreatedAt = t.CreatedAt
                    }).ToList()
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}