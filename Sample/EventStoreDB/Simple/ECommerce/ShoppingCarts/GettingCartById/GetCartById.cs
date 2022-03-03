using ECommerce.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ShoppingCarts.GettingCartById;

public record GetCartById(
    Guid ShoppingCartId
) : IRequest<ShoppingCartDetails>
{
    public static GetCartById From(Guid cartId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new GetCartById(cartId);
    }
}

public class GetCartByIdHandler : IRequestHandler<GetCartById, ShoppingCartDetails>
{
    private readonly ECommerceDbContext _eCommerceDbContext;

    public GetCartByIdHandler(ECommerceDbContext eCommerceDbContext)
    {
        _eCommerceDbContext = eCommerceDbContext;
    }

    public async Task<ShoppingCartDetails> Handle(GetCartById query, CancellationToken cancellationToken)
    {
        return await _eCommerceDbContext.Set<ShoppingCartDetails>()
            .SingleOrDefaultAsync(
                x => x.Id == query.ShoppingCartId, cancellationToken
            ) ?? throw new InvalidOperationException();
    }
}
