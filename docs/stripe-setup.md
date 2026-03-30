# Stripe Integration Setup — VatBe

This guide explains how to configure Stripe billing for the VatBe production environment.

## Prerequisites

1. A Stripe account (https://stripe.com) — test mode is fine for local dev
2. Stripe CLI installed locally for webhook forwarding during development

---

## 1. Create Products & Prices in Stripe Dashboard

Go to **Products** → **Add product** and create:

| Product | Price (monthly) | Price (annual) |
|---------|----------------|----------------|
| VatBe Pro | €15 / month | €144 / year |
| VatBe Enterprise | €99 / month | €948 / year |

Note down the **Price IDs** (`price_...`) — you'll need them in step 3.

---

## 2. Get Your API Keys

In the Stripe Dashboard → **Developers** → **API keys**:

- **Publishable key**: `pk_live_...` (or `pk_test_...` for test mode)
- **Secret key**: `sk_live_...` (or `sk_test_...` for test mode)

---

## 3. Configure appsettings (Production)

Set these as **environment variables** or **Kubernetes secrets** — never commit real keys to git.

### Kubernetes secret (recommended)

```bash
kubectl create secret generic vatbe-stripe \
  --namespace vatbe \
  --from-literal=SecretKey="sk_live_..." \
  --from-literal=WebhookSecret="whsec_..." \
  --from-literal=ProMonthly="price_..." \
  --from-literal=ProAnnual="price_..." \
  --from-literal=EnterpriseMonthly="price_..." \
  --from-literal=EnterpriseAnnual="price_..."
```

### Environment variable mapping (ASP.NET Core)

```
Stripe__PublishableKey=pk_live_...
Stripe__SecretKey=sk_live_...
Stripe__WebhookSecret=whsec_...
Stripe__BaseUrl=https://vatbe.garry-ai.cloud
Stripe__Prices__ProMonthly=price_...
Stripe__Prices__ProAnnual=price_...
Stripe__Prices__EnterpriseMonthly=price_...
Stripe__Prices__EnterpriseAnnual=price_...
```

---

## 4. Register the Webhook in Stripe

Go to **Developers** → **Webhooks** → **Add endpoint**:

- **URL**: `https://vatbe.garry-ai.cloud/api/stripe/webhook`
- **Events to listen for**:
  - `checkout.session.completed`
  - `customer.subscription.created`
  - `customer.subscription.updated`
  - `customer.subscription.deleted`
  - `invoice.payment_failed`

Copy the **Signing secret** (`whsec_...`) → set as `Stripe__WebhookSecret`.

---

## 5. Local Development

Use the Stripe CLI to forward webhooks to localhost:

```bash
stripe listen --forward-to https://localhost:5001/api/stripe/webhook
```

Copy the local signing secret displayed by the CLI into your `appsettings.Development.json`.

---

## 6. Test Stripe Checkout

1. Visit `/pricing` on the local or staging site
2. Click **Get Started** on the Pro plan
3. Use test card `4242 4242 4242 4242`, any future expiry, any CVC
4. Confirm the webhook fires at `/api/stripe/webhook`
5. Check logs for `Checkout completed:` entry

---

## Sprint 2 TODOs (post-groundwork)

- [ ] Provision API keys on `checkout.session.completed`
- [ ] Store subscription status in database
- [ ] Revoke API keys on `customer.subscription.deleted`
- [ ] Dunning emails on `invoice.payment_failed`
- [ ] Customer portal (`/billing`) for self-service subscription management
- [ ] Stripe Tax integration (once Stripe Tax is enabled on account)
