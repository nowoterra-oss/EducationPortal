# Email Notification Setup for EduPortal CI/CD

This document explains how to configure email notifications for the EduPortal CI/CD pipeline.

## Overview

The CI/CD pipeline sends email notifications for:
- **CI Build & Test**: Success/failure notifications after each build
- **CD Deploy Test**: Deployment notifications for test environment
- **CD Deploy Prod**: Deployment notifications for production environment
- **Rollback**: Notifications when rollback operations are executed

## Prerequisites

1. A Gmail account or SMTP server access
2. GitHub repository admin access
3. Ability to create GitHub secrets and variables

## Step 1: Enable Email Notifications

### Create Repository Variable

Go to your GitHub repository:
1. Navigate to **Settings** > **Secrets and variables** > **Actions**
2. Click on **Variables** tab
3. Click **New repository variable**
4. Add:
   - **Name:** `EMAIL_NOTIFICATIONS_ENABLED`
   - **Value:** `true`

> **Note:** Set to `false` to disable email notifications without removing secrets.

## Step 2: Configure SMTP Secrets

### Gmail Setup (Recommended)

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate App Password:**
   - Go to [Google Account Security](https://myaccount.google.com/security)
   - Click "2-Step Verification"
   - Scroll down and click "App passwords"
   - Select "Mail" and "Other (Custom name)"
   - Enter "EduPortal CI/CD" as the name
   - Click "Generate" and copy the 16-character password

3. **Add GitHub Secrets:**

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `EMAIL_HOST` | `smtp.gmail.com` | Gmail SMTP server |
| `EMAIL_PORT` | `587` | SMTP port (TLS) |
| `EMAIL_USERNAME` | `your-email@gmail.com` | Your Gmail address |
| `EMAIL_PASSWORD` | `xxxx xxxx xxxx xxxx` | App password (16 chars) |
| `EMAIL_FROM` | `EduPortal CI/CD <your-email@gmail.com>` | Sender display name |
| `EMAIL_TO` | `team@example.com` | Recipient(s), comma-separated |

### Other SMTP Providers

#### Outlook/Office 365
```
EMAIL_HOST: smtp.office365.com
EMAIL_PORT: 587
```

#### SendGrid
```
EMAIL_HOST: smtp.sendgrid.net
EMAIL_PORT: 587
EMAIL_USERNAME: apikey
EMAIL_PASSWORD: <your-sendgrid-api-key>
```

#### Mailgun
```
EMAIL_HOST: smtp.mailgun.org
EMAIL_PORT: 587
```

## Step 3: Add Secrets to GitHub

1. Go to **Settings** > **Secrets and variables** > **Actions**
2. Click **New repository secret**
3. Add each secret from the table above

### Required Secrets Checklist

```
[ ] EMAIL_HOST
[ ] EMAIL_PORT
[ ] EMAIL_USERNAME
[ ] EMAIL_PASSWORD
[ ] EMAIL_FROM
[ ] EMAIL_TO
```

## Email Templates

The pipeline uses professional HTML email templates with the following color schemes:

| Event | Color | Icon |
|-------|-------|------|
| Build Success | Green (#22c55e) | ✅ |
| Build Failure | Red (#ef4444) | ❌ |
| Test Deploy | Purple (#8b5cf6) | 🧪 |
| Prod Deploy | Green (#10b981) | 🚀 |
| Deploy Failure | Red (#ef4444) | 🚨 |
| Rollback | Orange (#f59e0b) | ⚠️ |

## Testing Email Configuration

### Manual Test

You can test the email configuration by triggering a workflow manually:

1. Go to **Actions** tab
2. Select any workflow (e.g., "CI - Build & Test")
3. Click **Run workflow**
4. Check your email inbox

### Verify Secrets

To verify your secrets are configured:

```bash
# This will show if secrets exist (not their values)
gh secret list
```

## Multiple Recipients

To send emails to multiple recipients, use comma-separated email addresses:

```
EMAIL_TO: dev@example.com,ops@example.com,manager@example.com
```

## Troubleshooting

### Emails Not Sending

1. **Check if notifications are enabled:**
   - Verify `EMAIL_NOTIFICATIONS_ENABLED` variable is set to `true`

2. **Verify secrets exist:**
   - Go to Settings > Secrets and variables > Actions
   - Ensure all EMAIL_* secrets are listed

3. **Check workflow logs:**
   - Go to Actions tab
   - Click on failed workflow
   - Look for email step errors

### Gmail "Less Secure Apps" Error

Gmail requires App Passwords instead of regular passwords:
- You must enable 2-Factor Authentication first
- Generate an App Password specifically for this use

### Authentication Failed

Common causes:
- Incorrect password/app password
- Wrong SMTP port
- Missing TLS/SSL configuration
- Account security restrictions

### Rate Limiting

SMTP providers have rate limits:
- Gmail: ~500 emails/day for personal accounts
- Consider using SendGrid/Mailgun for high-volume needs

## Slack Notifications (Optional)

In addition to email, you can enable Slack notifications:

1. Create a Slack Incoming Webhook
2. Add repository variable: `SLACK_NOTIFICATIONS_ENABLED=true`
3. Add secret: `SLACK_WEBHOOK_URL=<your-webhook-url>`

## Security Best Practices

1. **Never commit secrets** to the repository
2. **Use App Passwords** instead of real passwords
3. **Limit recipient list** to necessary team members
4. **Review access** periodically
5. **Use environment secrets** for production deployments

## Email Content

Emails include:
- Build/deployment status
- Version/tag information
- Branch name
- Commit SHA
- Author
- Recent changelog (last 5 commits)
- Direct link to GitHub Actions run

## Disabling Notifications

To temporarily disable notifications:

1. Set `EMAIL_NOTIFICATIONS_ENABLED` to `false`
2. Or remove the variable entirely

To permanently remove email notifications:
1. Delete all EMAIL_* secrets
2. Delete EMAIL_NOTIFICATIONS_ENABLED variable

---

## Quick Reference

### Gmail Configuration
```
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=<16-char-app-password>
EMAIL_FROM=EduPortal CI/CD <your-email@gmail.com>
EMAIL_TO=team@example.com
```

### Enable/Disable Variable
```
EMAIL_NOTIFICATIONS_ENABLED=true  # Enable
EMAIL_NOTIFICATIONS_ENABLED=false # Disable
```

---

For more information, see the [GitHub Actions documentation](https://docs.github.com/en/actions/security-guides/encrypted-secrets).
