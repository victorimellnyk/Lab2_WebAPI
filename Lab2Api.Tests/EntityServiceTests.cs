using Lab2Api.DTOs;
using Lab2Api.Services;
using Xunit;
using Lab2Api.Models;

namespace Lab2Api.Tests;

public class EntityServiceTests
{
[Fact]
public async Task CreateAsync_ValidData_ReturnsEntityWithId()
{
// Arrange
var service = new EntityService();

    var dto = new CreateEntityDto
    {
        Name = "Test Entity",
        Description = "Test Description"
    };

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.True(result.Id > 0);
    Assert.Equal("Test Entity", result.Name);
    Assert.Equal("Test Description", result.Description);
}
[Fact]
public async Task GetAllAsync_Search_FiltersRecords()
{
// Arrange
var service = new EntityService();

await service.CreateAsync(new CreateEntityDto
{
    Name = "Apple",
    Description = "Fruit"
});

await service.CreateAsync(new CreateEntityDto
{
    Name = "Banana",
    Description = "Yellow fruit"
});

await service.CreateAsync(new CreateEntityDto
{
    Name = "Orange",
    Description = "Citrus"
});

// Act
var result = await service.GetAllAsync(
    page: 1,
    pageSize: 10,
    search: "APPLE");

// Assert
Assert.Single(result.Items);
Assert.Equal("Apple", result.Items[0].Name);
Assert.Equal(1, result.TotalCount);
Assert.Equal(1, result.TotalPages);
Assert.Equal(1, result.CurrentPage);
}
[Fact]
public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
{
// Arrange
var service = new EntityService();

for (int i = 1; i <= 15; i++)
{
    await service.CreateAsync(new CreateEntityDto
    {
        Name = $"Entity {i}",
        Description = $"Description {i}"
    });
}

// Act
var result = await service.GetAllAsync(
    page: 2,
    pageSize: 10);

// Assert
Assert.Equal(5, result.Items.Count);
Assert.Equal(15, result.TotalCount);
Assert.Equal(2, result.TotalPages);
Assert.Equal(2, result.CurrentPage);
Assert.True(result.HasPreviousPage);
Assert.False(result.HasNextPage);
}
[Fact]
public async Task GetAllAsync_SearchNotFound_ReturnsEmptyResult()
{
// Arrange
var service = new EntityService();

await service.CreateAsync(new CreateEntityDto
{
    Name = "Apple",
    Description = "Fruit"
});

// Act
var result = await service.GetAllAsync(
    page: 1,
    pageSize: 10,
    search: "Samsung");

// Assert
Assert.Empty(result.Items);
Assert.Equal(0, result.TotalCount);
Assert.Equal(0, result.TotalPages);
Assert.Equal(1, result.CurrentPage);
Assert.False(result.HasNextPage);
Assert.False(result.HasPreviousPage);
}
[Fact]
public async Task GetAllAsync_FirstPage_ReturnsTenItems()
{
var service = new EntityService();

for (int i = 1; i <= 15; i++)
{
    await service.CreateAsync(new CreateEntityDto
    {
        Name = $"Entity {i}"
    });
}

var result = await service.GetAllAsync();

Assert.Equal(10, result.Items.Count);
Assert.Equal(15, result.TotalCount);
Assert.Equal(2, result.TotalPages);
Assert.Equal(1, result.CurrentPage);
Assert.False(result.HasPreviousPage);
Assert.True(result.HasNextPage);
}

[Fact]
public async Task GetAllAsync_PageOutOfRange_ReturnsEmptyItems()
{
var service = new EntityService();

await service.CreateAsync(new CreateEntityDto
{
    Name = "Test Entity"
});

var result = await service.GetAllAsync(
    page: 5,
    pageSize: 10);

Assert.Empty(result.Items);
Assert.Equal(1, result.TotalCount);
Assert.Equal(1, result.TotalPages);
Assert.Equal(5, result.CurrentPage);
}

[Fact]
public async Task GetAllAsync_PageSizeOver100_IsLimitedTo100()
{
var service = new EntityService();

for (int i = 1; i <= 105; i++)
{
    await service.CreateAsync(new CreateEntityDto
    {
        Name = $"Entity {i}"
    });
}

var result = await service.GetAllAsync(
    page: 1,
    pageSize: 200);

Assert.Equal(100, result.Items.Count);
Assert.Equal(100, result.PageSize);
Assert.Equal(105, result.TotalCount);
Assert.Equal(2, result.TotalPages);
}
[Fact]
public async Task GetAllAsync_SearchAndPagination_WorksTogether()
{
// Arrange
var service = new EntityService();

await service.CreateAsync(new CreateEntityDto
{
    Name = "Apple One"
});

await service.CreateAsync(new CreateEntityDto
{
    Name = "Banana"
});

await service.CreateAsync(new CreateEntityDto
{
    Name = "Apple Two"
});

await service.CreateAsync(new CreateEntityDto
{
    Name = "Apple Three"
});

// Act
var result = await service.GetAllAsync(
    page: 2,
    pageSize: 2,
    search: "apple");

// Assert
Assert.Single(result.Items);
Assert.Equal("Apple Three", result.Items[0].Name);
Assert.Equal(3, result.TotalCount);
Assert.Equal(2, result.TotalPages);
Assert.Equal(2, result.CurrentPage);
Assert.True(result.HasPreviousPage);
Assert.False(result.HasNextPage);
}
[Fact]
public async Task GetAllAsync_SearchByDescription_ReturnsMatchingRecords()
{
// Arrange
var service = new EntityService();

await service.CreateAsync(new CreateEntityDto
{
    Name = "First Entity",
    Description = "Programming in CSharp"
});

await service.CreateAsync(new CreateEntityDto
{
    Name = "Second Entity",
    Description = "Database systems"
});

// Act
var result = await service.GetAllAsync(
    page: 1,
    pageSize: 10,
    search: "csharp");

// Assert
Assert.Single(result.Items);
Assert.Equal("First Entity", result.Items[0].Name);
Assert.Equal(1, result.TotalCount);
Assert.Equal(1, result.TotalPages);
}
[Fact]
public async Task UpdateAsync_ExistingEntity_UpdatesSuccessfully()
{
// Arrange
var service = new EntityService();

var created = await service.CreateAsync(new CreateEntityDto
{
    Name = "Old Name",
    Description = "Old Description"
});

var updateDto = new UpdateEntityDto
{
    Name = "New Name",
    Description = "New Description"
};

// Act
var result = await service.UpdateAsync(created.Id, updateDto);

// Assert
Assert.NotNull(result);
Assert.Equal(created.Id, result.Id);
Assert.Equal("New Name", result.Name);
Assert.Equal("New Description", result.Description);
}
[Fact]
public async Task UpdateAsync_NonExistingEntity_ThrowsNotFound()
{
// Arrange
var service = new EntityService();

var updateDto = new UpdateEntityDto
{
    Name = "Updated Name",
    Description = "Updated Description"
};

// Act & Assert
await Assert.ThrowsAsync<KeyNotFoundException>(
    async () => await service.UpdateAsync(999, updateDto));
}
[Fact]
public async Task UpdateAsync_InvalidName_ThrowsException()
{
// Arrange
var service = new EntityService();

var created = await service.CreateAsync(new CreateEntityDto
{
    Name = "Original Name",
    Description = "Original Description"
});

var updateDto = new UpdateEntityDto
{
    Name = "A",
    Description = "Updated Description"
};

// Act & Assert
await Assert.ThrowsAsync<ArgumentException>(
    async () => await service.UpdateAsync(created.Id, updateDto));
}
[Fact]
public async Task UpdateAsync_TooLongDescription_ThrowsException()
{
// Arrange
var service = new EntityService();

var created = await service.CreateAsync(new CreateEntityDto
{
    Name = "Original Name",
    Description = "Original Description"
});

var updateDto = new UpdateEntityDto
{
    Name = "Updated Name",
    Description = new string('A', 501)
};

// Act & Assert
await Assert.ThrowsAsync<ArgumentException>(
    async () => await service.UpdateAsync(
        created.Id, updateDto));
}
[Fact]
public async Task GetAllAsync_SearchWithSpacesAndMixedCase_ReturnsMatchingRecords()
{
// Arrange
var service = new EntityService();

var field = typeof(EntityService).GetField(
    "_entities",
    System.Reflection.BindingFlags.NonPublic |
    System.Reflection.BindingFlags.Instance);

Assert.NotNull(field);

var entities = Assert.IsType<List<Entity>>(
    field.GetValue(service));

entities.Add(new Entity
{
    Id = 1,
    Name = "Apple",
    Description = "Fruit"
});

entities.Add(new Entity
{
    Id = 2,
    Name = "Banana",
    Description = "Fruit"
});

// Act
var result = await service.GetAllAsync(
    page: 1,
    pageSize: 10,
    search: "  APPLE  ");

// Assert
Assert.Single(result.Items);
Assert.Equal("Apple", result.Items[0].Name);
Assert.Equal(1, result.TotalCount);
}
[Theory]
[InlineData(0, 10, 1, 10)]
[InlineData(-1, 10, 1, 10)]
[InlineData(1, 0, 1, 10)]
[InlineData(1, -5, 1, 10)]
[InlineData(1, 150, 1, 100)]
public async Task GetAllAsync_InvalidPagination_NormalizesParameters(
int page,
int pageSize,
int expectedPage,
int expectedPageSize)
{
// Arrange
var service = new EntityService();

// Act
var result = await service.GetAllAsync(
    page: page,
    pageSize: pageSize);

// Assert
Assert.Equal(expectedPage, result.CurrentPage);
Assert.Equal(expectedPageSize, result.PageSize);
}
}