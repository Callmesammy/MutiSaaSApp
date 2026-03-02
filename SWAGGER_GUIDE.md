# 🔍 **SWAGGER UI ACCESS GUIDE**

> How to view and test your API using Swagger UI

---

## **✅ Swagger is Now Enabled!**

Your API now includes comprehensive Swagger UI documentation with:
- ✅ All endpoints documented
- ✅ Request/response schemas
- ✅ JWT authentication support
- ✅ Try-it-out capability
- ✅ Real-time API testing

---

## **🚀 How to Access Swagger**

### **Local Development (Docker)**

```bash
# Start the application
docker-compose up -d

# Access Swagger UI at:
http://localhost:5000/swagger
```

### **Local Development (.NET SDK)**

```bash
# Run the application
dotnet run -p MutiSaaSApp

# Access Swagger UI at:
https://localhost:5001/swagger
```

### **Production (Kubernetes)**

```bash
# Get the service IP
kubectl get svc -n mutisaas-production

# Access Swagger UI at:
http://<YOUR_LOAD_BALANCER_IP>/swagger
```

---

## **📋 API Endpoints Available in Swagger**

### **Authentication**
- `POST /api/auth/register` - Register new organization
- `POST /api/auth/login` - Login with email/password

### **Invitations**
- `POST /api/invites/create` - Create invitation token
- `POST /api/invites/accept` - Accept invitation

### **Tasks**
- `GET /api/tasks` - List all tasks (with filters & pagination)
- `GET /api/tasks/{id}` - Get specific task
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### **Health**
- `GET /api/health` - Check application health (DB + Cache)

---

## **🔐 Testing With JWT Authentication**

### **Step 1: Register Organization**

1. Go to Swagger UI: `http://localhost:5000/swagger`
2. Expand `POST /api/auth/register`
3. Click "Try it out"
4. Enter sample data:
   ```json
   {
     "organizationName": "Acme Corp",
     "email": "admin@acme.com",
     "password": "SecurePassword123!"
   }
   ```
5. Click "Execute"
6. Copy the `token` from response

### **Step 2: Authenticate for Other Endpoints**

1. Click the **"Authorize"** button at top of Swagger UI
2. Paste your token in the format: `Bearer <your-token-here>`
3. Click "Authorize"
4. Now all endpoints are authenticated!

### **Step 3: Test Other Endpoints**

- Create task: `POST /api/tasks`
- List tasks: `GET /api/tasks`
- Get health: `GET /api/health`
- etc.

---

## **📊 Example Requests**

### **Register Organization**
```bash
POST /api/auth/register
Content-Type: application/json

{
  "organizationName": "Acme Corp",
  "email": "admin@acme.com",
  "password": "SecurePassword123!"
}

Response:
{
  "success": true,
  "data": {
    "organizationId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### **Create Task**
```bash
POST /api/tasks
Authorization: Bearer {YOUR_TOKEN}
Content-Type: application/json

{
  "title": "Complete API documentation",
  "description": "Write comprehensive API docs",
  "status": 0,
  "priority": 2,
  "dueDate": "2024-02-01T00:00:00Z"
}

Response:
{
  "success": true,
  "data": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "title": "Complete API documentation",
    "status": "Todo",
    "priority": "High",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### **List Tasks With Filters**
```bash
GET /api/tasks?status=Todo&priority=High&skip=0&take=10&orderBy=dueDate
Authorization: Bearer {YOUR_TOKEN}

Response:
{
  "success": true,
  "data": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "title": "Complete API documentation",
      "status": "Todo",
      "priority": "High",
      "dueDate": "2024-02-01T00:00:00Z"
    }
  ],
  "metadata": {
    "total": 1,
    "skip": 0,
    "take": 10
  }
}
```

---

## **🧪 Testing Without Swagger**

### **Using curl**

```bash
# Register organization
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "organizationName": "Acme Corp",
    "email": "admin@acme.com",
    "password": "SecurePassword123!"
  }'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@acme.com",
    "password": "SecurePassword123!"
  }'

# Get health (no auth needed)
curl http://localhost:5000/api/health

# Create task (with token)
curl -X POST http://localhost:5000/api/tasks \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Task",
    "description": "Task description",
    "status": 0,
    "priority": 1
  }'
```

### **Using Postman**

1. Import API from Swagger: `http://localhost:5000/swagger/v1/swagger.json`
2. Or manually create requests with endpoints above
3. Use "Bearer Token" auth type in Postman
4. Paste your JWT token

---

## **📍 Swagger Configuration**

Swagger is configured in `Program.cs`:

```csharp
// Enable Swagger generation
builder.Services.AddSwaggerGen();

// In middleware pipeline:
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MutiSaaS API v1");
    options.RoutePrefix = "swagger";
});
```

**Available at:**
- Development: `http://localhost:5000/swagger` or `https://localhost:5001/swagger`
- Production: `http://<YOUR_DOMAIN>/swagger`

---

## **✨ Features**

✅ **Complete API Documentation**
- All endpoints listed
- Request/response schemas
- Parameter descriptions

✅ **JWT Authentication**
- "Authorize" button for token input
- Automatic Bearer token injection

✅ **Try-it-Out**
- Test endpoints directly from UI
- See real responses
- Copy curl commands

✅ **Schema Validation**
- Request validation
- Response validation
- Error documentation

---

## **🐛 Troubleshooting**

### **"Swagger not found" / 404 error**
- Make sure app is running: `docker-compose up -d`
- Check port is correct (5000 for HTTP, 5001 for HTTPS)
- Try: `http://localhost:5000/swagger`

### **"Endpoints not showing"**
- Rebuild project: `dotnet build`
- Restart app: `docker-compose restart`
- Clear browser cache

### **"Authorize button not working"**
- Ensure token is in format: `Bearer eyJhbG...`
- Token must be valid (not expired)
- Register first to get a token

### **"500 errors when testing"**
- Check application logs: `docker logs mutisaasapp`
- Verify database is running: `docker ps`
- Check Redis is running: `redis-cli ping`

---

## **📚 Next Steps**

1. **Access Swagger:** Go to `http://localhost:5000/swagger`
2. **Register Organization:** Try the register endpoint
3. **Get Token:** Copy token from response
4. **Authorize:** Click Authorize button, paste token
5. **Test Endpoints:** Try each endpoint
6. **Read Docs:** Check endpoint descriptions & schemas

---

**Happy API Testing! 🚀**
