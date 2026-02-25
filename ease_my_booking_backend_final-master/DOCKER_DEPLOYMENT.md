# Deploy .NET Backend to Render Using Docker

## Why Docker?

Render doesn't natively support .NET. We use Docker to containerize the application.

## Step 1: Create Dockerfile & .dockerignore

✅ Already created in the project root:
- `Dockerfile` - Builds and runs your .NET app
- `.dockerignore` - Excludes unnecessary files from image

## Step 2: Push Code to GitHub

```bash
cd e:\RTH
git add Dockerfile .dockerignore
git commit -m "Add Docker configuration for Render deployment"
git push
```

## Step 3: Create PostgreSQL on Render

1. Go to [render.com](https://render.com)
2. Click **"New +"** → **"PostgreSQL"**
3. Configure:
   - **Name**: `resume-template-hub-db`
   - **Database**: `resumetemplatehub`
   - **User**: `postgres`
   - **Plan**: Free
4. Copy the **External Database URL**

## Step 4: Deploy Backend on Render

1. Click **"New +"** → **"Web Service"**
2. **Connect Repository**:
   - Select your GitHub repo
   - Choose branch: `main`
3. **Service Details**:
   - **Name**: `resume-template-hub-api`
   - **Environment**: Select `Docker` (NOT Dot Net)
   - **Region**: Choose closest to you
   - **Plan**: Free
4. Click **"Create Web Service"**

Render will automatically:
- Detect the `Dockerfile`
- Build the Docker image
- Deploy the container

## Step 5: Add Environment Variables

While deploying, add these variables:

1. In Render → Your Service → **Environment**
2. Click **"Add Environment Variable"** for each:

```
DATABASE_URL=postgresql://postgres:PASSWORD@host:5432/resumetemplatehub
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
CORS_ORIGINS=http://localhost:3000
JWT_SECRET_KEY=base64:F3se3HmayMxiwReecjtpN2iF4a9tv2DYDpMAwVPFSIM=
MAIL_USERNAME=stylesaj617@gmail.com
MAIL_PASSWORD=your-gmail-app-password
RAZORPAY_KEY=rzp_test_9ta1IN9vcYLKN5
RAZORPAY_SECRET=XQFVEB284iUGNTvW7yNcjAXG
CLOUDINARY_CLOUD_NAME=ddlblcd4x
CLOUDINARY_API_KEY=235291278438183
CLOUDINARY_API_SECRET=ZJ3tKlyvoksMoeo3RFmd9g0WhDI
```

## Step 6: Wait for Deployment

⏳ First deployment takes 15-30 minutes while Docker builds the image.

Monitor in Render's **Logs** tab:
- Look for `"Server started..."` message
- If errors appear, check DATABASE_URL format

## Step 7: Get Your API URL

Once deployed, you'll have:
```
https://resume-template-hub-api.onrender.com
```

Test it:
```bash
curl https://resume-template-hub-api.onrender.com/swagger
```

You should see Swagger API documentation.

## Step 8: Deploy Frontend

1. Go to [vercel.com](https://vercel.com)
2. Create `.env.production`:
   ```
   REACT_APP_API_URL=https://resume-template-hub-api.onrender.com/
   REACT_APP_RAZORPAY_KEY=rzp_test_9ta1IN9vcYLKN5
   ```
3. Import repo and deploy
4. Copy your Vercel URL (e.g., `https://resume-template-hub.vercel.app`)

## Step 9: Update CORS

1. Go back to Render backend service
2. Update environment variable:
   ```
   CORS_ORIGINS=https://resume-template-hub.vercel.app
   ```
3. Service auto-redeploys

## Debugging Docker Builds

If deployment fails, check logs:

```bash
# View build logs in Render dashboard
Logs → Filter for "ERROR" or "ERROR"

# Common issues:
1. DATABASE_URL format wrong
2. Missing environment variables
3. Port mismatch (should be 10000)
```

## Test Everything

1. Visit frontend URL
2. Sign up / Login
3. Create listings
4. Upload images
5. Make bookings
6. Test payments

---

**Total Deployment Time**: ~30 minutes (first time)
**Cost**: $0/month (free tier)
