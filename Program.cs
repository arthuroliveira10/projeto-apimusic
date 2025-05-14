using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Services.AddSingleton<DataStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Music API", Version = "v1" });
});

var app = builder.Build();

// Configuração de middlewares
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseDefaultFiles(); // Procura por index.html automaticamente
app.UseStaticFiles();  // Serve os arquivos da pasta wwwroot

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "API DE UPLOAD DE MUSICA");

// Registro de usuário
app.MapPost("/register", (User user, DataStore db) =>
{
    if (db.Users.Any(u => u.Username == user.Username))
        return Results.BadRequest("Nome de usuário já existe.");

    user.Token = Guid.NewGuid().ToString();
    db.Users.Add(user);
    return Results.Ok(new { user.Username, user.Token });
});

// Login (simples)
app.MapPost("/login", (User input, DataStore db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Username == input.Username);
    if (user == null) return Results.NotFound("Usuário não encontrado.");
    return Results.Ok(new { user.Username, user.Token });
});

// Criar sala
app.MapPost("/rooms", (RoomCreateRequest request, HttpRequest req, DataStore db) =>
{
    if (!ValidateToken(req, db, out var user)) return Results.Unauthorized();

    var room = new Room
    {
        Id = Guid.NewGuid().ToString(),
        Name = request.Name,
        CreatedBy = user.Username
    };
    db.Rooms.Add(room);
    return Results.Ok(room);
});

// Listar salas
app.MapGet("/rooms", (HttpRequest req, DataStore db) =>
{
    if (!ValidateToken(req, db, out _)) return Results.Unauthorized();
    return Results.Ok(db.Rooms);
});

// Adicionar música à sala
app.MapPost("/rooms/{roomId}/songs", (string roomId, Song song, HttpRequest req, DataStore db) =>
{
    if (!ValidateToken(req, db, out _)) return Results.Unauthorized();
    var room = db.Rooms.FirstOrDefault(r => r.Id == roomId);
    if (room == null) return Results.NotFound("Sala não encontrada.");

    song.Id = room.Playlist.Count + 1;
    room.Playlist.Add(song);
    return Results.Ok(song);
});

// Listar músicas de uma sala
app.MapGet("/rooms/{roomId}/songs", (string roomId, HttpRequest req, DataStore db) =>
{
    if (!ValidateToken(req, db, out _)) return Results.Unauthorized();
    var room = db.Rooms.FirstOrDefault(r => r.Id == roomId);
    if (room == null) return Results.NotFound("Sala não encontrada.");

    return Results.Ok(room.Playlist);
});

app.Run();

bool ValidateToken(HttpRequest req, DataStore db, out User? user)
{
    user = null;
    if (!req.Headers.TryGetValue("Authorization", out var token)) return false;
    user = db.Users.FirstOrDefault(u => u.Token == token);
    return user != null;
}

// Models

record User
{
    public required string Username { get; set; }
    public string? Token { get; set; }
}

record Song
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public required string Url { get; set; }
}

record Room
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string CreatedBy { get; set; }
    public List<Song> Playlist { get; set; } = new();
}

record RoomCreateRequest
{
    public required string Name { get; set; }
}

class DataStore
{
    public List<User> Users { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
}
