# FREE DEPLOYMENT GUIDE - Render.com

Complete step-by-step guide to deploy **Resume Template Hub** for FREE on Render.com.

## Cost Breakdown

| Service | Cost | Why Free |
|---------|------|----------|
| PostgreSQL Database | FREE | 1 free tier database included |
| Backend (Node.js/Go) | FREE | 750 free hours/month |
| Frontend (Vercel) | FREE | Unlimited free deployments |
| Email (Gmail SMTP) | FREE | Using personal Gmail account |
| Image Storage (Cloudinary) | FREE | 25GB free tier |
| Payments (Razorpay) | FREE | No setup cost, commission on txns |
| **TOTAL** | **$0/month** | ✓ Completely Free |

---

## Prerequisites

1. GitHub account (to push your code)
2. Render.com account
3. Vercel account (already have it)
4. Gmail account with app password
5. Razorpay, Cloudinary accounts (free)

---

## Step 1: Prepare Your Repositories

### Backend Repository

```bash
cd ease_my_booking_backend_final-master

# Initialize git if not present
git init

# Create .gitignore and add sensitive files
# (Already done in this project)

# Commit code
git add .
git commit -m "Initial commit - ready for Render deployment"

# Create repo on GitHub and push
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/ease_my_booking_backend.git
git push -u origin main
```

### Frontend Repository

```bash
cd ease_my_booking_frontend_final-main

# Create .env.production
cat > .env.production << EOF
REACT_APP_API_URL=https://your-render-app.onrender.com/
REACT_APP_RAZORPAY_KEY=rzp_live_xxxxx
EOF

# Add to git
git add .env.production
git commit -m "Add production environment config"

# Push to GitHub
git push
```

---

## Step 2: Create PostgreSQL Database on Render

1. Go to [render.com](https://render.com)
2. Click **"New +"** → **"PostgreSQL"**
3. Fill in details:
   - **Name**: `resume-template-hub-db`
   - **Database**: `resumetemplatehub`
   - **User**: `postgres`
   - **Plan**: Free
4. Click **"Create Database"**
5. Wait 2-3 minutes for creation
6. Copy the **External Database URL** (looks like: `postgresql://user:pass@host:5432/db`)

---

## Step 3: Deploy Backend on Render

### Create Web Service

1. Go to [render.com](https://render.com)
2. Click **"New +"** → **"Web Service"**
3. Connect your GitHub account
4. Select `ease_my_booking_backend` repository
5. Fill in:
   - **Name**: `resume-template-hub-api`
   - **Environment**: `Dot Net`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `cd out && ./ResumeTemplateHub.Api`
   - **Plan**: Free

### Add Environment Variables

Click **"Add Environment Variable"** and set each:

```
DATABASE_URL=postgresql://user:password@host:5432/resumetemplatehub
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
CORS_ORIGINS=https://your-vercel-frontend.vercel.app
JWT_SECRET_KEY=base64:YOUR_KEY_HERE
MAIL_USERNAME=your-email@gmail.com
MAIL_PASSWORD=your-app-password
RAZORPAY_KEY=rzp_live_xxxxx
RAZORPAY_SECRET=your-secret
CLOUDINARY_CLOUD_NAME=your-cloud
CLOUDINARY_API_KEY=your-key
CLOUDINARY_API_SECRET=your-secret
```

6. Click **"Create Web Service"**
7. Wait 5-10 minutes for deployment
8. Copy your API URL (e.g., `https://resume-template-hub-api.onrender.com`)

---

## Step 4: Update Frontend Environment

Create `.env.production` in frontend:

```bash
REACT_APP_API_URL=https://ease-my-booking-api.onrender.com/
REACT_APP_RAZORPAY_KEY=rzp_live_xxxxx
```

Push to GitHub:

```bash
git add .env.production
git commit -m "Update API URL for production"
git push
```

---

## Step 5: Deploy Frontend on Vercel

1. Go to [vercel.com](https://vercel.com)
2. Click **"New Project"**
3. Import `ease_my_booking_frontend`
4. Set **Environment Variables**:
   - `REACT_APP_API_URL`: (your Render backend URL)
   - `REACT_APP_RAZORPAY_KEY`: (your test key)
5. Click **"Deploy"**
6. Note your Vercel URL (e.g., `https://ease-my-booking.vercel.app`)

---

## Step 6: Final Configuration

### Update Backend CORS

Update **appsettings.Production.json** with your Vercel URL:

```json
"Cors": {
  "AllowedOrigins": ["https://resume-template-hub.vercel.app"]
}
```

### Update Render Environment

In Render dashboard → Your backend service → Settings:
- Update `CORS_ORIGINS` to: `https://resume-template-hub.vercel.app`

---

## Step 7: Database Migrations

The first deployment will fail because the database schema doesn't exist.

### Option A: Manual Migration (Recommended)

1. Connect via SSH to your Render service
2. Run:
```bash
dotnet ef database update
```

### Option B: Auto-Migration in Program.cs

Add this to `Program.cs` after DbContext setup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
}
```

Then redeploy.

---

## Step 8: Test Everything

### Backend Health Check
```bash
curl https://resume-template-hub-api.onrender.com/health
```

### Frontend
Visit: `https://resume-template-hub.vercel.app`
- Test login
- Test place creation
- Test image upload
- Test booking

---

## Troubleshooting

### Backend Won't Deploy
1. Check **Logs** in Render dashboard
2. Verify all **Environment Variables** are set
3. Check PostgreSQL connection string is correct

### Frontend Can't Connect to API
1. Verify CORS is enabled for your Vercel domain
2. Check Render backend is running (check logs)
3. Verify frontend `.env.production` has correct API URL

### Database Migration Errors
1. Check PostgreSQL is accessible
2. Verify connection string format: `postgresql://user:pass@host:5432/db`
3. Manually run `dotnet ef database update` locally first

### Email Not Sending
1. Verify Gmail app password (16 chars from Google)
2. Enable "Less secure app access" if using regular password
3. Check MAIL_USERNAME and MAIL_PASSWORD are correct

---

## Monitoring & Logs

### View Backend Logs
1. Render dashboard → Your service → Logs
2. Useful commands:
   - Search for "ERROR" to find issues
   - Check database connection strings

### View Frontend Logs
1. Vercel dashboard → Your project → Deployments
2. Click deployment → Logs

---

## Cost After Free Tier Expiration

| Service | Monthly Cost |
|---------|--------------|
| Render PostgreSQL | $7/month (if upgraded) |
| Render Backend | $7/month (if upgraded) |
| Vercel | FREE |
| Others | FREE |
| **MINIMUM** | **$0** (limit to free tier) |

**To keep costs at zero**: Ensure free tier resources don't auto-upgrade.

---

## Next Steps

1. ✅ Push code to GitHub
2. ✅ Create PostgreSQL on Render
3. ✅ Deploy backend on Render
4. ✅ Deploy frontend on Vercel
5. ✅ Set environment variables
6. ✅ Test all features
7. ✅ Monitor logs for issues

**Your app is now live for FREE!**

