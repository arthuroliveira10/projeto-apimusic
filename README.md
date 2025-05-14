# projeto-apimusic

| Entidade | Campos principais                           |
| -------- | ------------------------------------------- |
| **User** | Username, Token                             |
| **Song** | Id, Title, Artist, Url                      |
| **Room** | Id, Name, Playlist (List\<Song>), CreatedBy |

GET /
Rota inicial que retorna a mensagem "API estilo Untitled.stream 🚀"
Rotas de Usuário:
POST /register - Para registrar novos usuários
POST /login - Para fazer login
Rotas de Salas:
GET /rooms - Lista todas as salas
POST /rooms - Cria uma nova sala
Rotas de Músicas:
GET /rooms/{roomId}/songs - Lista todas as músicas de uma sala específica
POST /rooms/{roomId}/songs - Adiciona uma nova música a uma sala específica


