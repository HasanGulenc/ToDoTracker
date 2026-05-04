using System.Net;
using System.Net.Http.Json;
using TaskManager.Tests.Helpers;
using Xunit;

namespace TaskManager.Tests.Tasks;

public class TaskIsolationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<HttpClient> NewAuthClient()
    {
        var client = factory.CreateClient();
        var token = await AuthHelper.RegisterAndGetToken(client);
        return client.WithToken(token);
    }

    private static async Task<int> CreateTask(HttpClient client, string title, string? dueDate = null)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new { title, dueDate, status = 0, priority = 0 });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!.Id;
    }

    [Fact]
    public async Task UserB_CannotGet_UserAs_Task()
    {
        var clientA = await NewAuthClient();
        var clientB = await NewAuthClient();
        var taskId = await CreateTask(clientA, "A's private task");

        var response = await clientB.GetAsync($"/api/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserB_CannotUpdate_UserAs_Task()
    {
        var clientA = await NewAuthClient();
        var clientB = await NewAuthClient();
        var taskId = await CreateTask(clientA, "A's task");

        var response = await clientB.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "Hijacked",
            status = 0,
            priority = 0
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserB_CannotDelete_UserAs_Task()
    {
        var clientA = await NewAuthClient();
        var clientB = await NewAuthClient();
        var taskId = await CreateTask(clientA, "A's task");

        var response = await clientB.DeleteAsync($"/api/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnTasks()
    {
        var clientA = await NewAuthClient();
        var clientB = await NewAuthClient();

        await CreateTask(clientA, "A task 1");
        await CreateTask(clientA, "A task 2");
        await CreateTask(clientB, "B task 1");

        var response = await clientA.GetAsync("/api/tasks");
        var body = await response.Content.ReadFromJsonAsync<TaskListResponse>();

        Assert.Equal(2, body!.Total);
        Assert.All(body.Items, t => Assert.StartsWith("A task", t.Title));
    }

    [Fact]
    public async Task DueToday_ReturnsOnlyOwnTasks()
    {
        var clientA = await NewAuthClient();
        var clientB = await NewAuthClient();
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

        await CreateTask(clientA, "A due today", today);
        await CreateTask(clientB, "B due today", today);

        var response = await clientA.GetAsync("/api/tasks/due-today");
        var items = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();

        Assert.Single(items!);
        Assert.Equal("A due today", items![0].Title);
    }
}
