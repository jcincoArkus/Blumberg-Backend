---
name: strategy-pattern
description: Use when you have multiple interchangeable implementations (e.g. email via SMTP vs SES, different notification channels). Defines where to put the abstraction and how to keep callers independent of concrete backends.
---

# Strategy pattern (swappable implementations)

**Pattern type:** **Behavioral** (GoF). Strategy encapsulates a family of algorithms/behaviours and makes them interchangeable. The caller depends on an abstraction; the concrete implementation is chosen at runtime (e.g. by DI/config).

Use this when **the same action can be done in different ways** (e.g. send email via SMTP, SES, or testmail.app; send notification via email, SMS, or push) and you want to **swap the implementation** by configuration or DI without changing callers.

## When to use

- **Notification delivery**: email (SMTP, AWS SES, testmail.app), SMS (Twilio, SNS), push (FCM, APNs) — one interface, multiple transports.
- **Storage**: local disk vs S3 vs Azure Blob (`IFileStorage` / `IBlobStorage`).
- **Cache**: in-memory vs Redis vs distributed (`ICache`).
- **Messaging / queue**: in-memory vs SQS vs RabbitMQ (`IMessageSender`).
- **Secrets**: env vars vs AWS Secrets Manager vs Vault (`ISecretsProvider`).
- **Feature flags**: in-memory vs database vs LaunchDarkly (`IFeatureFlagProvider`).
- **Auth token issuance**: JWT (current) vs another scheme — if you had multiple, you’d use a strategy interface (we currently have `IJwtService` in Adapters; for cross-cutting “how we issue tokens” you could put the abstraction in Shared and keep Strategy + Adapter consistent).
- **Test doubles**: no-op or fake implementation for the same interface in tests.

Any place where **more than one concrete implementation** will exist and the choice is **environment or config-driven**.

## Relation to other GoF patterns

- **Adapter (Structural):** Each strategy implementation often **adapts** an external system to our interface (e.g. `SmtpEmailSender` adapts SmtpClient to `IEmailSender`). So we use **Strategy** for “which implementation” and **Adapter** inside each implementation. No separate Adapter skill needed unless we want to document “wrap third-party APIs behind our interface” explicitly.
- **Facade (Structural):** A service that coordinates several dependencies (e.g. AuthService over AdminRepo + JwtService) is a facade. We already do this via the service-pattern; we don’t name it Facade. No extra skill needed.
- **Bridge (Structural):** Decouples abstraction from implementation so both can vary. Less common in app code; Strategy + interface in Shared gives us enough. Skip unless we have two orthogonal hierarchies.
- **Mediator (Behavioral):** One object coordinates many (e.g. “notification mediator” that decides email vs SMS vs push). Use when you have **multiple channels** and one coordinator; Strategy is when you **swap one** implementation (e.g. “email via SMTP or SES”). Different use case.
- **Chain of Responsibility (Behavioral):** Request passes along a chain (e.g. validation pipeline, middleware). Use for pipelines/handlers, not for “pick one of N backends.” Different use case.

## Rules (aligned with module-pattern)

1. **Define the abstraction in Shared/**  
   Put the interface in `Shared/` (e.g. `Shared/Notifications/IAlertNotificationSender.cs` or `Shared/Email/IEmailSender.cs`) so that **no module or adapter is required** by the caller. Callers (e.g. Ingestion, Alerts) depend only on the interface.

2. **Implement in Adapters or a dedicated module**  
   Implementations (e.g. `SmtpEmailSender`, `SesEmailSender`, `NoOpEmailSender`) live in an Adapter (e.g. `Adapters.Email`) or a thin module that references the adapter/SDK. Register the **chosen** implementation in DI (e.g. from config: `Email:Provider` = `Smtp` | `Ses` | `None`).

3. **Callers depend on the interface only**  
   The service that orchestrates the action (e.g. “when alert becomes Active, send email”) takes `IAlertNotificationSender` (or `IEmailSender`) in the constructor and calls it. It does **not** reference SmtpClient, AWS SDK, or specific adapters.

4. **Use the same pattern for tests**  
   In tests, inject a no-op or fake implementation so you don’t send real emails or call real APIs.

## Example shape (alert email — implemented)

- **Shared:** Define the delivery abstraction and a DTO; optionally an orchestrator interface so the trigger (e.g. Ingestion) stays decoupled from the module that owns the flow (e.g. Alerts).
  - `IAlertNotificationSender` — `Task SendAsync(AlertNotificationMessage message, CancellationToken ct = default);`
  - `AlertNotificationMessage` — DTO with AlertId, Severity, SensorName, EquipmentName, DetectedValue, ThresholdMin/Max, TriggeredAtUtc, RecipientEmails.
  - `IAlertRecipientResolver` — `Task<IReadOnlyList<string>> GetEmailsForOrganizationAsync(Guid organizationId, CancellationToken ct);` (recipients by org; implemented in Auth).
  - `IAlertTriggeredNotifier` — orchestrator: `Task NotifyTriggeredAsync(Alert alert, CancellationToken ct);` (load alert, resolve recipients, build message, call sender). Implemented in Alerts; Ingestion depends only on this.
- **Adapters.Email:** `SmtpAlertNotificationSender`, `NoOpAlertNotificationSender` implementing `IAlertNotificationSender`. Config (e.g. `EmailConfig` from env: `ALERT_EMAIL_PROVIDER`, `SMTP_*`). Register `EmailConfig` + chosen sender in DI (e.g. `AddAlertEmailNotifications(services, config)` before modules).
- **Orchestrator (Alerts):** `AlertTriggeredNotifier` implements `IAlertTriggeredNotifier`: uses `IAlertRepository`, `IAlertRecipientResolver`, `IAlertNotificationSender`; after successful send, append a `NotificationSent` alert event for audit.
- **Trigger (Ingestion):** After persisting new alerts, loop over `newAlerts` and call `IAlertTriggeredNotifier.NotifyTriggeredAsync(alert, ct)`. Catch and log notification failures so ingestion does not fail.
- **DI order:** Register sender (and config) first, then modules (Auth → Alerts → Ingestion) so `IAlertTriggeredNotifier` and its dependencies resolve.

## Do we already use this?

- **JWT:** We have `IJwtService` in Adapters.Jwt; AuthService depends on the interface. That’s dependency inversion. The interface lives in the Adapter project, not Shared. For new cross-cutting “pluggable” behaviour (email, storage, etc.), **put the interface in Shared** so no module references an Adapter by name; then Strategy + module-pattern align.
- **Alert email (reference):** Implemented per this skill: `Shared/Notifications` (IAlertNotificationSender, AlertNotificationMessage, IAlertRecipientResolver, IAlertTriggeredNotifier), `Adapters.Email` (Smtp + NoOp senders, EmailConfig, AddAlertEmailNotifications), `Modules.Alerts` (AlertTriggeredNotifier), `Modules.Auth` (AlertRecipientResolver). Ingestion calls only `IAlertTriggeredNotifier`; notification failures are caught so ingestion still succeeds.
- **Other (storage / cache / queue):** When adding them, follow the same shape: interface in Shared, implementations in Adapters, one chosen in DI from config.

## Relation to other skills

- **module-pattern**: “Don’t cross-reference modules directly — use shared abstractions in Shared/.” The strategy interface is that shared abstraction; implementations live in adapters or a small module.
- **service-pattern**: The orchestrating service (e.g. alert notification service) is a normal service with an interface; it uses the strategy interface for the actual send.
