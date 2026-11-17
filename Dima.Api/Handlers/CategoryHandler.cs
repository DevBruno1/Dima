using Dima.Api.Data;
using Dima.Core.Handlers;
using Dima.Core.Models;
using Dima.Core.Requests.Categories;
using Dima.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Dima.Api.Handlers;

public class CategoryHandler(AppDbContext context) : ICategoryHandler
{
    public async Task<Response<Category?>> CreateAsync(CreateCategoryRequest request)
    {
        try
        {
            var category = new Category
            {
                UserId = request.UserId,
                Title = request.Title,
                Description = request.Description
            };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            return new Response<Category?>(category, 201, "Categoria criada com sucesso");
        }
        catch
        {
            return new Response<Category?>(null, 500, "Nao foi possivel criar a categoria");
        }
    }

    public async Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request)
    {
        try
        {
            var category =
                await context.Categories.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
            if (category is null)
                return new Response<Category?>(null, 404, "Categoria nao encontrada");

            category.Title = request.Title;
            category.Description = request.Description;

            context.Categories.Update(category);
            await context.SaveChangesAsync();
            return new Response<Category?>(category);
        }
        catch (Exception ex)
        {
            return new Response<Category?>(null, 500, "Nao foi possivel atualizar a categoria");
        }
    }

    public async Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request)
    {
        try
        {
            var category =
                await context.Categories.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
            if (category is null)
                return new Response<Category?>(null, 404, "Categoria nao encontrada");

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return new Response<Category?>(category, 200,"Categoria deletada com sucesso");
        }
        catch
        {
            return new Response<Category?>(null, 500, "Nao foi possivel excluir a categoria");
        }
    }

    public Task<Response<List<Category>>> GetAllAsync(GetAllCategoriesRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<Category?>> GetByIdAsync(GetCategoryByIdRequest request)
    {
        try
        {
            var category = await context.Categories.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return category is null
                ? new Response<Category?>(null, 404, "Categoria nao encontrada")
                : new Response<Category?>(category);
        }
        catch
        {
            return new Response<Category?>(null, 500, "Nao foi possivel encontrar a categoria");
        }
    }
}