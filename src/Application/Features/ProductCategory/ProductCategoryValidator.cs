// using Application.Common.Exceptions;
// using Application.Common.Interfaces.Contexts;
// using Application.Contract.ProductCategory.Commands;
// using FluentValidation;
//
// namespace Application.Features.ProductCategory;
//
// public class ProductCategoryValidator : AbstractValidator<CreateProductCategoryCommand>
// {
//     private readonly IApplicationDbContext _context;
//
//     public ProductCategoryValidator(IApplicationDbContext context)
//     {
//         _context = context;
//         RuleFor(s => s.Name).Custom(NameValidator).NotNull().NotEmpty();
//     }
//
//     private void NameValidator(string categoryName, ValidationContext<CreateProductCategoryCommand> validatorContext)
//     {
//         if (_context.ProductCategories.Any(s => s.Name.ToLower() == categoryName.ToLower()))
//             throw new ExistException("the category with name " + categoryName + " is already exist");
//     }
// }

