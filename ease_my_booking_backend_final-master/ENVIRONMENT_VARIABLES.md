# Environment Variables for Ease My Booking API

This document lists all environment variables needed to run the application in different environments.

## Local Development

Create `appsettings.Development.json` (add to .gitignore):

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "http://127.0.0.1:3000",
      "http://127.0.0.1:5173"
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EaseMyBookingDB_Local;Integrated Security=true;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "base64:F3se3HmayMxiwReecjtpN2iF4a9tv2DYDpMAwVPFSIM="
  },
  "MailSettings": {
    "Username": "your-gmail@gmail.com",
    "Password": "your-app-password"
  },
  "PaymentSettings": {
    "RazorpayKey": "rzp_test_xxxxx",
    "RazorpaySecret": "xxxxx"
  },
  "CloudSettings": {
    "CloudName": "your-cloudinary-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

## Production (Render.com)

Set these environment variables in Render dashboard:

### Database
```
DATABASE_URL=postgresql://user:password@host:5432/database_name
```

### CORS
```
ASPNETCORE_CORS_ORIGINS=https://your-frontend-domain.com
```

### JWT
```
JWT_SECRET_KEY=base64:your-base64-encoded-secret-key
ASPNETCORE_ENVIRONMENT=Production
```

### Email (Gmail)
```
MAIL_HOST=smtp.gmail.com
MAIL_USERNAME=your-email@gmail.com
MAIL_PASSWORD=your-app-specific-password
```

### Payment (Razorpay)
```
RAZORPAY_KEY=rzp_live_xxxxx
RAZORPAY_SECRET=your-api-secret
```

### Cloud Storage (Cloudinary)
```
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret
```

## Getting Values

### Gmail App Password
1. Enable 2FA on your Gmail account
2. Go to [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
3. Select "Mail" and "Windows Computer"
4. Copy the generated 16-character password

### Razorpay Keys
1. Sign up at [razorpay.com](https://razorpay.com)
2. Go to Dashboard → Settings → API Keys
3. Copy Test Key ID and Secret (for testing)
4. Use Live keys for production

### Cloudinary Keys
1. Sign up at [cloudinary.com](https://cloudinary.com)
2. Go to Dashboard → Settings → API
3. Copy Cloud Name, API Key, and API Secret

### JWT Secret Key
Generate a strong base64-encoded key:
```bash
# PowerShell
[Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes('your-secret-key-here')) | Out-Host
```
