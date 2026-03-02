using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;

namespace Template.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<SampleItem> SampleItems { get; }
}
