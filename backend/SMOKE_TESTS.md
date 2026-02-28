# ShopWave Smoke Tests

Quick curl-based integration checks. Run these after `dotnet run` starts (default: `http://localhost:5106`).

## 1. Register

```bash
curl -s -X POST http://localhost:5106/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test123!"}' | jq .
```

Expected: `{ "accessToken": "...", "refreshToken": "...", "username": "testuser", ... }`

## 2. Login

```bash
curl -s -X POST http://localhost:5106/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}' | jq .
```

Save the `accessToken` value:

```bash
TOKEN=$(curl -s -X POST http://localhost:5106/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}' | jq -r '.accessToken')
```

## 3. Profile (requires auth)

```bash
curl -s http://localhost:5106/api/auth/profile \
  -H "Authorization: Bearer $TOKEN" | jq .
```

Expected: 200 with user profile. Without token: 401.

## 4. Cart (requires auth)

```bash
curl -s http://localhost:5106/api/cart \
  -H "Authorization: Bearer $TOKEN" | jq .
```

Expected: 200 with cart items (empty array initially). Without token: 401.

## 5. Orders (requires auth)

```bash
curl -s http://localhost:5106/api/orders \
  -H "Authorization: Bearer $TOKEN" | jq .
```

Expected: 200 with paginated orders. Without token: 401.

## 6. Reviews — Create (requires auth)

```bash
curl -s -X POST http://localhost:5106/api/reviews \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"productId":1,"rating":5,"comment":"Great product, highly recommend!"}' | jq .
```

Expected: 200 or 400 (if already reviewed). Without token: 401.

## 7. Admin Dashboard (requires Admin role)

```bash
curl -s http://localhost:5106/api/Admin/dashboard \
  -H "Authorization: Bearer $TOKEN" -w "\nHTTP Status: %{http_code}\n"
```

For a non-admin user: **403 Forbidden**.
For an admin user: **200** with dashboard data.

### Making a User Admin

Option A — SQL update:

```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'test@example.com';
```

Option B — The database is seeded with an admin user on first run. Check `SeedData.cs` for the default admin credentials.

After promoting to Admin, log in again to get a fresh token with the Admin role claim.

## 8. Refresh Token

```bash
REFRESH=$(curl -s -X POST http://localhost:5106/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}' | jq -r '.refreshToken')

curl -s -X POST http://localhost:5106/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH\"}" | jq .
```

Expected: New `accessToken` + `refreshToken` (rotation).

## 9. Verify 401 Clean Rejection (No Token)

Run these without the Authorization header to ensure they fail cleanly with 401, returning no stack traces or NullReferenceExceptions:

```bash
# Cart
curl -s http://localhost:5106/api/cart -w "\nHTTP Status: %{http_code}\n"
# Orders
curl -s http://localhost:5106/api/orders -w "\nHTTP Status: %{http_code}\n"
# Reviews
curl -s -X POST http://localhost:5106/api/reviews \
  -H "Content-Type: application/json" \
  -d '{"productId":1,"rating":5,"comment":"test"}' -w "\nHTTP Status: %{http_code}\n"
```

Expected: **401 Unauthorized** for all without crashing backend.
