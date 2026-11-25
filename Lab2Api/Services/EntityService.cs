using Lab2Api.DTOs;
using Lab2Api.Mappings;
using Lab2Api.Models;
using Lab2Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Lab2Api.Services;

public class EntityService : IEntityService
{
    private readonly List<Entity> _entities = new();
    private int _nextId = 1;

    private readonly AppDbContext? _context;

public EntityService()
{
    // Конструктор для наявних модульних тестів
}

public EntityService(AppDbContext context)
{
    _context = context;
}

    public async Task<EntityResponseDto> CreateAsync(CreateEntityDto dto)
    {
        if (dto == null ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            dto.Name.Length < 2 ||
            dto.Name.Length > 100 ||
            (dto.Description?.Length ?? 0) > 500)
        {
            throw new ArgumentException("Invalid entity data");
        }

        var entity = dto.ToEntity();

        if (_context != null)
{
    _context.Entities.Add(entity);
    await _context.SaveChangesAsync();
}
else
{
    entity.Id = _nextId++;
    _entities.Add(entity);
}

return entity.ToResponseDto();
    }
    public Task<PagedResultDto<EntityResponseDto>> GetAllAsync(
int page = 1,
int pageSize = 10,
string? search = null)
{
if (page < 1)
page = 1;

if (pageSize < 1)
    pageSize = 10;

if (pageSize > 100)
    pageSize = 100;

IQueryable<Entity> query = _context != null
    ? _context.Entities.AsNoTracking()
    : _entities.AsQueryable();

if (!string.IsNullOrWhiteSpace(search))
{
search = search.Trim().ToLower();

query = query.Where(e =>
    e.Name.ToLower().Contains(search) ||
    (e.Description != null &&
     e.Description.ToLower().Contains(search)));
}

int totalCount = query.Count();

int totalPages = (int)Math.Ceiling(
    (double)totalCount / pageSize);

var items = query
    .OrderBy(e => e.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(e => e.ToResponseDto())
    .ToList();

var result = new PagedResultDto<EntityResponseDto>
{
    Items = items,
    TotalCount = totalCount,
    TotalPages = totalPages,
    CurrentPage = page,
    PageSize = pageSize
};

return Task.FromResult(result);
}
public async Task<EntityResponseDto> UpdateAsync(
    int id, UpdateEntityDto dto)
{
    var entity = _context != null
        ? await _context.Entities.FindAsync(id)
        : _entities.FirstOrDefault(e => e.Id == id);

if (entity == null)
{
    throw new KeyNotFoundException(
        $"Entity with ID {id} not found");
}

if (string.IsNullOrWhiteSpace(dto.Name) ||
    dto.Name.Length < 2 ||
    dto.Name.Length > 100)
{
    throw new ArgumentException(
        "Name must contain 2-100 characters");
}

if (dto.Description?.Length > 500)
{
    throw new ArgumentException(
        "Description cannot exceed 500 characters");
}

entity.Name = dto.Name;
entity.Description = dto.Description ?? string.Empty;

var result = new EntityResponseDto
{
    Id = entity.Id,
    Name = entity.Name,
    Description = entity.Description ?? string.Empty,
    CreatedAt = entity.CreatedAt
};

if (_context != null)
{
    await _context.SaveChangesAsync();
}

return result;
}
}