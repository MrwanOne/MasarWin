-- Normalize empty emails to NULL to allow optional email with unique constraint
UPDATE doctor SET email = NULL WHERE email IS NOT NULL AND TRIM(email) = '';
UPDATE student SET email = NULL WHERE email IS NOT NULL AND TRIM(email) = '';

-- Ensure the email columns allow NULL
ALTER TABLE doctor MODIFY email VARCHAR(200) NULL;
ALTER TABLE student MODIFY email VARCHAR(200) NULL;

-- Drop existing indexes if present to avoid conflicts
ALTER TABLE doctor DROP INDEX IF EXISTS idx_doctor_email;
ALTER TABLE student DROP INDEX IF EXISTS idx_student_email;

-- Add unique constraints (MySQL allows multiple NULL values)
ALTER TABLE doctor ADD UNIQUE INDEX idx_doctor_email (email);
ALTER TABLE student ADD UNIQUE INDEX idx_student_email (email);
