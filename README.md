# OpenTalk

A modern, real-time chat application with group messaging and direct messaging capabilities. Built with ASP.NET Core backend and Vue 3 frontend.

## Features

- **Group Chat**: Create and manage chat groups with multiple members
- **Direct Messaging**: One-on-one private conversations
- **Real-time Communication**: Powered by SignalR for instant message delivery
- **File Sharing**: Upload and share files within groups and direct messages
- **User Management**: User registration, authentication, and profile management
- **Admin Dashboard**: Manage users, groups, and files
- **Dark Mode**: Toggle between light and dark themes
- **Message History**: Persistent message storage with pagination support

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Real-time**: SignalR
- **Database**: MySQL
- **ORM**: Dapper
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: Vue 3
- **Build Tool**: Vite
- **UI Library**: Element Plus
- **HTTP Client**: Axios
- **Routing**: Vue Router
- **Language**: TypeScript

## Project Structure

```
OpenTalk/
├── server/
│   └── OpenTalk.Api/
│       ├── Application/          # Business logic & services
│       ├── Controllers/          # API endpoints
│       ├── Domain/              # Entity models
│       ├── Hubs/                # SignalR hubs
│       ├── Infrastructure/      # Data access & repositories
│       ├── Pages/               # Admin dashboard (Razor Pages)
│       ├── sql/                 # Database schema
│       └── Program.cs           # Application startup
└── web/
    └── opentalk-web/
        ├── src/
        │   ├── api/             # HTTP client configuration
        │   ├── components/      # Vue components
        │   ├── views/           # Page views
        │   └── main.ts          # Entry point
        └── package.json
```

## Getting Started

### Prerequisites

- **.NET 8.0 SDK** or later
- **Node.js 18+** and npm
- **MySQL 8.0+**

### Backend Setup

1. **Configure Database Connection**

   Edit `server/OpenTalk.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost;Port=3306;Database=opentalk;User Id=root;Password=your_password;charset=utf8mb4;AllowUserVariables=True"
     }
   }
   ```

2. **Initialize Database**

   ```bash
   mysql -u root -p opentalk < server/OpenTalk.Api/sql/schema.sql
   ```

3. **Run the Backend**

   ```bash
   cd server/OpenTalk.Api
   dotnet run
   ```

   The API will be available at `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`
   - Admin Dashboard: `https://localhost:5001/Admin/Dashboard`

### Frontend Setup

1. **Install Dependencies**

   ```bash
   cd web/opentalk-web
   npm install
   ```

2. **Configure API Base URL**

   Edit `src/api/http.ts` if needed to point to your backend server.

3. **Run Development Server**

   ```bash
   npm run dev
   ```

   The application will be available at `http://localhost:5173`

4. **Build for Production**

   ```bash
   npm run build
   ```

## Default Credentials

- **Admin Username**: `admin`
- **Admin Password**: `admin` (SHA256 hash: `8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918`)

⚠️ **Important**: Change the default admin password in production!

## Configuration

### Storage Provider

The application supports two storage providers for file uploads:

- **LocalStorageProvider** (default): Stores files in `wwwroot/uploads`
- **OssStorageProvider**: Stores files in Alibaba OSS

To switch providers, edit `server/OpenTalk.Api/Program.cs`:

```csharp
// Use local storage (default)
builder.Services.AddScoped<IStorageProvider, LocalStorageProvider>();

// Or use Alibaba OSS
// builder.Services.AddScoped<IStorageProvider, OssStorageProvider>();
```

### CORS Configuration

CORS is configured to allow requests from any origin. For production, update the CORS policy in `Program.cs`:

```csharp
policy.WithOrigins("https://yourdomain.com")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login

### Groups
- `GET /api/groups` - List user's groups
- `POST /api/groups` - Create new group
- `GET /api/groups/{id}` - Get group details
- `DELETE /api/groups/{id}` - Delete group

### Messages
- `GET /api/messages` - Get group messages
- `POST /api/messages` - Send message to group
- `GET /api/direct-messages` - Get direct messages
- `POST /api/direct-messages` - Send direct message

### Files
- `POST /api/files/upload` - Upload file
- `GET /api/files` - List files

### Statistics
- `GET /api/stats` - Get application statistics

## Real-time Features

The application uses SignalR for real-time communication. Connect to the hub at:

```
wss://localhost:5001/hubs/chat
```

### Hub Methods

- `SendMessage` - Send message to group
- `SendDirectMessage` - Send direct message
- `JoinGroup` - Join a group
- `LeaveGroup` - Leave a group

## Database Schema

The application uses the following main tables:

- `users` - User accounts and profiles
- `chat_groups` - Group information
- `group_members` - Group membership
- `messages` - Group messages
- `direct_messages` - Direct messages
- `files` - File records

See `server/OpenTalk.Api/sql/schema.sql` for complete schema.

## Development

### Backend Development

- API documentation available at `/swagger`
- Admin dashboard at `/Admin/Dashboard`
- Modify services in `Application/Services/`
- Add new endpoints in `Controllers/`

### Frontend Development

- Components in `src/components/`
- Views in `src/views/`
- API calls in `src/api/`
- Routing configuration in `src/router.ts`

## Deployment

### Docker (Recommended)

Create a `Dockerfile` for the backend and frontend, then deploy using Docker Compose.

### Manual Deployment

1. Build the backend: `dotnet publish -c Release`
2. Build the frontend: `npm run build`
3. Deploy to your hosting platform
4. Configure environment variables and database connection
5. Set up SSL certificates for HTTPS

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or suggestions, please open an issue on GitHub.

---

**Built with ❤️ using ASP.NET Core and Vue 3**
