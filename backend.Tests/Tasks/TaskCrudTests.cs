using System.Net;
using System.Net.Http.Json;
using TaskManager.Tests.Helpers;
using Xunit;

namespace TaskManager.Tests.Tasks;

public class TaskCrudTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<HttpClient> NewAuthClient()
    {
        var client = factory.CreateClient();
        var token = await AuthHelper.RegisterAndGetToken(client);
        return client.WithToken(token);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocationAndTask()
    {
        var client = await NewAuthClient();

        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Test Task",
            description = "A description",
            status = 0, // Todo
            priority = 1  // Medium
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(task);
        Assert.Equal("Test Task", task.Title);
        Assert.Equal("A description", task.Description);
        Assert.Equal("Todo", task.Status);
        Assert.Equal("Medium", task.Priority);
    }

    [Fact]
    public async Task Create_EmptyTitle_Returns400()
    {
        var client = await NewAuthClient();

        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = "",
            status = 0,
            priority = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingTask_ReturnsTask()
    {
        var client = await NewAuthClient();
        var created = await CreateTask(client, "Fetch Me");

        var response = await client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal("Fetch Me", task!.Title);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var client = await NewAuthClient();

        var response = await client.GetAsync("/api/tasks/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingTask_ReturnsUpdatedTask()
    {
        var client = await NewAuthClient();
        var created = await CreateTask(client, "Before Update");

        var response = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = "After Update",
            status = 1, // InProgress
            priority = 2  // High
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal("After Update", task!.Title);
        Assert.Equal("InProgress", task.Status);
        Assert.Equal("High", task.Priority);
    }

    [Fact]
    public async Task Update_NonExistent_Returns404()
    {
        var client = await NewAuthClient();

        var response = await client.PutAsJsonAsync("/api/tasks/999999", new
        {
            title = "Phantom",
            status = 0,
            priority = 0
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTask_Returns204_ThenGetReturns404()
    {
        var client = await NewAuthClient();
        var created = await CreateTask(client, "Delete Me");

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        var client = await NewAuthClient();

        var response = await client.DeleteAsync("/api/tasks/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    internal static async Task<TaskResponse> CreateTask(HttpClient client, string title, string? dueDate = null)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new { title, dueDate, status = 0, priority = 1 });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!;
    }
}
