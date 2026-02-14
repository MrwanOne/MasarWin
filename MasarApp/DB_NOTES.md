# Masar DbContext lifetime and email rules

- DbContext lifetime: the app now uses `AddDbContextFactory<MasarDbContext>`; repositories request the factory and create/dispose a fresh context per call with `await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);`. No DbContext instances are stored on services, view models, or singletons.
- Service layer: services depend on repositories only; `IUnitOfWork` was removed. If you add a new repository/service, inject `IDbContextFactory<MasarDbContext>` into the repository and scope any data operation to a single method call.
- Concurrency safety: avoid sharing DbContext across async flows or threads; always await DB calls before starting another that relies on the same data.
- Email optional + unique: run `Scripts/email_unique.sql` on existing databases to normalize empty emails to NULL, relax the column to allow NULL, and add unique indexes that permit multiple NULL values.
- Validation UX: Doctor/Student email is optional; duplicate non-empty emails are blocked with a bilingual message ("Email already exists / البريد الإلكتروني مستخدم بالفعل.").
