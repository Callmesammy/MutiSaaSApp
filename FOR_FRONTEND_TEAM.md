# 📢 SHARE THIS WITH YOUR FRONTEND TEAM

## **TL;DR** (30 seconds)

Backend is working. You're calling the wrong endpoint paths. Make these 3 fixes:

```env
# Fix 1: .env.local
NEXT_PUBLIC_API_URL=https://teamflow-gpasggaxb5cxhmc5.polandcentral-01.azurewebsites.net
```

```typescript
// Fix 2: lib/api/auth.ts
export async function registerOrganization(payload) {
  const response = await apiClient.post('/api/auth/register-organization', payload);
  //                                     ↑ CHANGED FROM: /api/organizations
}

// Fix 3: lib/api/members.ts  
export async function inviteMember(orgId, email) {
  const response = await apiClient.post(`/api/invites/organizations/${orgId}`, { email });
  //                                    ↑ CHANGED FROM: /api/organizations/{id}/invites
}
```

Deploy. Done. 🚀

---

## **The Issue**

Your API calls don't match the backend endpoints. The backend is correct - you're just calling them wrong.

**What you're calling:**
```
POST /api/organizations                    ← WRONG
POST /api/organizations/{id}/invites        ← WRONG
GET  /api/organizations/{id}/members       ← WORKS NOW (just added!)
```

**What backend has:**
```
POST /api/auth/register-organization       ← CORRECT
POST /api/invites/organizations/{id}       ← CORRECT
GET  /api/organizations/{id}/members       ← NEW! Just added!
```

---

## **Complete Fixes**

### **File 1: `.env.local`**

**Find:**
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

**Replace with:**
```env
NEXT_PUBLIC_API_URL=https://teamflow-gpasggaxb5cxhmc5.polandcentral-01.azurewebsites.net
```

---

### **File 2: `lib/api/auth.ts`**

**Find this function:**
```typescript
export async function registerOrganization(payload: RegisterOrganizationRequest) {
  const response = await apiClient.post('/api/organizations', payload);
  return response.data;
}
```

**Replace with:**
```typescript
export async function registerOrganization(payload: RegisterOrganizationRequest) {
  const response = await apiClient.post('/api/auth/register-organization', payload);
  return response.data;
}
```

---

### **File 3: `lib/api/members.ts`**

**Find:**
```typescript
export async function inviteMember(orgId: string, email: string) {
  const response = await apiClient.post(`/api/organizations/${orgId}/invites`, {
    email
  });
  return response.data.data;
}
```

**Replace with:**
```typescript
export async function inviteMember(orgId: string, email: string) {
  const response = await apiClient.post(`/api/invites/organizations/${orgId}`, {
    email
  });
  return response.data.data;
}
```

**Also make sure you have:**
```typescript
export async function getOrganizationMembers(orgId: string) {
  const response = await apiClient.get(`/api/organizations/${orgId}/members`);
  return response.data.data;
}
```

---

## **After Making Changes**

1. **Test locally:**
   ```bash
   npm run dev
   ```
   - Try registering
   - Try logging in
   - Try viewing members
   - Try creating tasks

2. **Deploy to Vercel:**
   ```bash
   git add .
   git commit -m "Fix: Update API endpoints and production URL"
   git push origin main
   ```

3. **Test in production:**
   - Registration should work
   - Login should work
   - Members should show
   - Tasks should work

---

## **What's Actually Happening**

**Backend has:**
```
Swagger UI: https://teamflow-gpasggaxb5cxhmc5.polandcentral-01.azurewebsites.net/swagger
```

Check it! All endpoints are listed there with exact paths.

**Your frontend is:**
- Using wrong endpoint paths
- Using local dev server URL instead of production

**Result:**
- 404 errors when trying to call endpoints
- CORS errors because of wrong URL

---

## **Why This Happened**

You built your frontend before finalizing the backend API contract. Common in distributed teams. No worries - it's a 5-minute fix.

---

## **Complete Endpoint List**

These are the ACTUAL endpoints (from Swagger):

```
✅ POST   /api/auth/register-organization
✅ POST   /api/auth/login  
✅ GET    /api/tasks
✅ POST   /api/tasks
✅ PUT    /api/tasks/{id}
✅ DELETE /api/tasks/{id}
✅ GET    /api/organizations/{orgId}/members        ← NEW!
✅ POST   /api/invites/organizations/{orgId}        ← Check path!
✅ POST   /api/invites/accept
✅ GET    /api/invites/pending
✅ GET    /api/health
```

---

## **Common Questions**

### **Q: Why is the members endpoint different than I expected?**
A: It is now - we just added it. Use: `/api/organizations/{orgId}/members`

### **Q: Why is the invite endpoint backwards?**
A: Backend designed it that way. Frontend needs to match: `/api/invites/organizations/{orgId}`

### **Q: Do I need to restart the backend?**
A: No. We redeployed it with the new Members endpoint. Just update your frontend.

### **Q: Will this really take 5 minutes?**
A: Yes. 3 file edits, 5 minutes tops.

### **Q: What if there are still errors after this?**
A: Check browser console (F12) and copy the exact error. Let us know.

---

## **Success Indicators**

After applying these fixes, you should see:

- ✅ Registration page works
- ✅ Can create organization
- ✅ Can login
- ✅ Dashboard loads
- ✅ Members page shows people
- ✅ Can create tasks
- ✅ Can invite members
- ✅ Can accept invites

---

## **Questions?**

If something doesn't work:
1. Check the exact error message
2. Clear browser cache
3. Restart `npm run dev`
4. Try again

Still stuck? Let me know the exact error and I'll help.

---

**That's it! Go fix those 3 endpoints and you're done.** 🚀

---

**P.S.** - Backend team did great work. This is just a path alignment issue. Not their fault. Not your fault. Standard integration hiccup. All fixed now! ✨
