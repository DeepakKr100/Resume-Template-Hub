# QUICK START - Render.com Deployment (5 Steps)

## Step 1: Create Database (2 minutes)
```
1. Go to render.com
2. New → PostgreSQL
3. Name: resume-template-hub-db
4. Plan: Free
5. Click Create
6. Copy the DATABASE_URL
```

## Step 2: Deploy Backend (5 minutes)
```
1. New → Web Service
2. Connect your GitHub backend repo
3. Name: resume-template-hub-api
4. Environment: Dot Net
5. Build: dotnet publish -c Release -o out
6. Start: cd out && ./ResumeTemplateHub.Api
7. Plan: Free
```

## Step 3: Add Environment Variables to Backend (2 minutes)
```
Click "Add Environment Variable" and paste:

DATABASE_URL=postgresql://user:pass@host:5432/db
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
CORS_ORIGINS=https://your-vercel.vercel.app
JWT_SECRET_KEY=base64:YOUR_ENCODED_SECRET
MAIL_USERNAME=your-email@gmail.com
MAIL_PASSWORD=your-app-password
RAZORPAY_KEY=rzp_live_xxxxx
RAZORPAY_SECRET=xxxxx
CLOUDINARY_CLOUD_NAME=xxxxx
CLOUDINARY_API_KEY=xxxxx
CLOUDINARY_API_SECRET=xxxxx
```

## Step 4: Deploy Frontend (1 minute)
```
1. Go to vercel.com
2. Import resume_template_hub_frontend repo
3. Set REACT_APP_API_URL=https://resume-template-hub-api.onrender.com/
4. Set REACT_APP_RAZORPAY_KEY=rzp_live_xxxxx
5. Deploy
```

## Step 5: Test (5 minutes)
```
1. Visit your Vercel URL
2. Create account and login
3. Upload place with images
4. Create booking
5. Test payment
```

## Common Issues & Fixes

| Issue | Fix |
|-------|-----|
| Backend won't deploy | Check logs in Render, verify env vars are set |
| Frontend can't connect | Update CORS_ORIGINS on backend, check API URL |
| Database error | Verify CONNECTION_STRING format |
| Images not uploading | Check Cloudinary credentials |
| Payments failing | Use Razorpay test keys first, then live |

## Cost
**$0/month** - Everything is free tier

## Help
- [Full Deployment Guide](DEPLOYMENT_GUIDE.md)
- [Environment Variables](ENVIRONMENT_VARIABLES.md)
- [Database Migration](DATABASE_MIGRATION.md)

---

**Total Time**: ~15 minutes
**Cost**: $0

