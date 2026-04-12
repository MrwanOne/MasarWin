# MasarApp

تطبيق مكتبي (WPF) لإدارة مشاريع التخرج، مبني بأسلوب Clean Architecture باستخدام .NET 8 وEntity Framework Core مع Oracle.

## المتطلبات

- نظام تشغيل: Windows
- .NET SDK 8.0 أو أحدث
- Oracle Database (مثل Oracle XE/Free)
- (اختياري) Visual Studio 2022 للتطوير والتشغيل من خلال الواجهة

## تشغيل المشروع

1. افتح الطرفية داخل مجلد المشروع:

```bash
cd MasarApp
```

2. استرجع الحزم:

```bash
dotnet restore Masar.sln
```

3. ابنِ المشروع:

```bash
dotnet build Masar.sln -c Debug
```

4. شغّل التطبيق:

```bash
dotnet run --project Masar.UI/Masar.UI.csproj
```

عند الإقلاع، التطبيق يقوم تلقائيا بـ:

- تطبيق الـ migrations
- إنشاء/تحديث Views وProcedures وFunctions وTriggers
- إنشاء حساب المسؤول الافتراضي (إذا لم يكن موجودا)

## تغيير اسم ورمز قاعدة البيانات (بيانات الاتصال)

يوجد مكانان مهمان يجب تحديثهما:

### 1) إعداد التشغيل الفعلي للتطبيق (Runtime)

الملف: `Masar.UI/appsettings.json`

عدّل قيمة `MasarDb` داخل `ConnectionStrings`:

```json
{
  "ConnectionStrings": {
    "MasarDb": "User Id=MASAR;Password=masar;Data Source=192.168.56.101:1521/freepdb1"
  }
}
```

### 2) إعداد أدوات EF Core وقت التصميم (Design-Time)

الملف: `Masar.Infrastructure/DesignTimeDbContextFactory.cs`

عدّل سطر الاتصال داخل `UseOracle(...)`:

```csharp
optionsBuilder.UseOracle(@"User Id=masar;Password=masar;Data Source=192.168.56.101:1521/FREEPDB1");
```

## شرح مكونات Connection String

- `User Id`: اسم مستخدم/Schema قاعدة البيانات
- `Password`: رمز (كلمة مرور) مستخدم قاعدة البيانات
- `Data Source`: عنوان Oracle بصيغة `HOST:PORT/SERVICE_NAME`

مثال:

```text
User Id=NEW_SCHEMA;Password=NEW_PASSWORD;Data Source=127.0.0.1:1521/FREEPDB1
```

## ملاحظات مهمة

- بعد تعديل بيانات الاتصال، أعد تشغيل التطبيق.
- عند استخدام أوامر `dotnet ef` يجب أن تكون بيانات الاتصال في `DesignTimeDbContextFactory` صحيحة.
- يفضل عدم رفع كلمات المرور الحقيقية إلى Git؛ استخدم قيما محلية أو أسرارا بيئية حسب بيئة العمل.
