using Dima.Api.Data;
using Dima.Core.Common.Extensions;
using Dima.Core.Handlers;
using Dima.Core.Models;
using Dima.Core.Requests.Transactions;
using Dima.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Dima.Api.Handlers;

public class TransactionHandler(AppDbContext context) : ITransactionHandler
{
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        try
        {
            var transaction = new Transaction
            {
                UserId = request.UserId,
                CreatedAt = DateTime.Now,
                Title = request.Title,
                Type = request.Type,
                Amount = request.Amount,
                CategoryId = request.CategoryId,
                PaidOrReceivedAt = request.PaidOrReceivedAt
            };
            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();
            return new Response<Transaction?>(transaction, 201, "Transacao criada com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Nao foi possivel criar a transacao");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        try
        {
            var transaction =
                await context.Transactions.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transacao nao encontrada");

            transaction.CategoryId = request.CategoryId;
            transaction.Title = request.Title;
            transaction.Type = request.Type;
            transaction.Amount = request.Amount;
            transaction.PaidOrReceivedAt = request.PaidOrReceivedAt;

            context.Transactions.Update(transaction);
            await context.SaveChangesAsync();
            return new Response<Transaction?>(transaction);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Nao foi possivel atualizar a transacao");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction =
                await context.Transactions.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transacao nao encontrada");

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            return new Response<Transaction?>(transaction, 200, "Transacao deletada com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Nao foi possivel deletar a transacao");
        }
    }

    public async Task<Response<Transaction>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await context.Transactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return transaction is null
                ? new Response<Transaction?>(null, 404, "Transacao nao encontrada")!
                : new Response<Transaction>(transaction);
        }
        catch
        {
            return new Response<Transaction>(null, 500, "Nao foi possivel recuperar a transacao");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionsByPeriodRequest request)
    {
        try
        {
            request.StartDate ??= DateTime.Now.GetFirstDay();
            request.EndDate ??= DateTime.Now.GetLastDay();
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500,
                "Nao foi possivel determinar a data de inicio ou termino");
        }

        try
        {
            var query = context
                .Transactions
                .AsNoTracking()
                .Where(x => x.CreatedAt >= request.StartDate && x.CreatedAt <= request.EndDate &&
                            x.UserId == request.UserId).OrderBy(x => x.CreatedAt);

            var transactions = await query.Skip(request.PageSize * (request.PageNumber - 1)).Take(request.PageSize)
                .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Transaction>?>(transactions, count, request.PageSize, request.PageNumber);
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500, "Nao foi possivel consultar as transacoes");
        }
    }
}