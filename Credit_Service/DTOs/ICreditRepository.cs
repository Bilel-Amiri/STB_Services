using Credit_Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Credit_Service.Services
{
    public interface ICreditRepository
    {
        Task<IEnumerable<Credit>> GetActiveCreditsByClientAsync(int clientId);
        Task<IEnumerable<Credit>> GetDueCreditsAsync(DateOnly upToDate);
        Task<Credit?> GetCreditByIdAsync(int creditId);
        Task AddCreditAsync(Credit credit);
        Task UpdateCreditAsync(Credit credit);
        Task AddRepaymentAsync(Repayment repayment);
        Task<IEnumerable<Credit>> GetOverdueCreditsAsync(DateOnly currentDate);
        Task<IEnumerable<Credit>> GetCreditsDueBetweenAsync(int clientId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<Credit>> GetActiveCreditsWithRepaymentsAsync();
    }

    public class CreditRepository : ICreditRepository
    {
        private readonly CreditDBcontext _context;

        public CreditRepository(CreditDBcontext context)
        {
            _context = context;
        }




        public async Task<IEnumerable<Credit>> GetActiveCreditsWithRepaymentsAsync()
        {
            return await _context.Credits
                .Where(c => c.Status == "Active")
                .Include(c => c.Repayments)  // Important: Load related repayments
                .ToListAsync();
        }



        public async Task AddCreditAsync(Credit credit)
        {
            await _context.Credits.AddAsync(credit);
            await _context.SaveChangesAsync();
        }

        public async Task AddRepaymentAsync(Repayment repayment)
        {
            await _context.Repayments.AddAsync(repayment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Credit>> GetActiveCreditsByClientAsync(int accountId)
        {
            return await _context.Credits
                .Where(c => c.AccountId == accountId && c.Status == "Active")
                .Include(c => c.Repayments)
                .ToListAsync();
        }

        public async Task<Credit?> GetCreditByIdAsync(int creditId)
        {
            return await _context.Credits
                .Include(c => c.Repayments)
                .FirstOrDefaultAsync(c => c.CreditId == creditId);
        }

        public async Task<IEnumerable<Credit>> GetDueCreditsAsync(DateOnly upToDate)
        {
            return await _context.Credits
                .Where(c => c.Status == "Active" && c.Repayments.Any(r => r.RepaymentDate <= upToDate))
                .Include(c => c.Repayments)
                .ToListAsync();
        }

        public async Task UpdateCreditAsync(Credit credit)
        {
            _context.Credits.Update(credit);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Credit>> GetOverdueCreditsAsync(DateOnly currentDate)
        {
            return await _context.Credits
                .Where(c => c.Status == "Active")
                .Include(c => c.Repayments)
                .Where(c => c.Repayments.Any(r =>
                    r.RepaymentDate < currentDate &&
                    r.Status != "Completed"))
                .ToListAsync();
        }


        public async Task<IEnumerable<Credit>> GetCreditsDueBetweenAsync(int accountId, DateOnly startDate, DateOnly endDate)
        {
            var credits = await _context.Credits
        .Where(c => c.AccountId == accountId && c.Status == "Active")
        .Include(c => c.Repayments)
        .ToListAsync();

            // Filter credits by calculated NextDueDate in memory
            return credits.Where(c =>
            {
                var nextDue = c.Repayments
                    .Where(r => r.Status != "Paid")
                    .OrderBy(r => r.RepaymentDate)
                    .Select(r => r.RepaymentDate)
                    .FirstOrDefault();

                return nextDue >= startDate && nextDue <= endDate;
            });
        }


    }
}