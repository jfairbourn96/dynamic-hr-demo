BEGIN;

INSERT INTO "EmployeeType" ("Id", "Name", "Description", "CreatedDate", "UpdatedDate", "FieldsJson")
VALUES (
  '4f4e6f6f-0e76-4b97-a3d4-3d3f22d99b01',
  'Software Engineer',
  'Engineering role with technical specialization, experience, on-call readiness, and review metadata.',
  CURRENT_TIMESTAMP,
  CURRENT_TIMESTAMP,
  '[
    {"Id":"17141c50-6d25-4fd0-b5e2-97c322c4ed01","Name":"primaryLanguage","Label":"Primary Language","FieldType":4,"Required":true,"Options":[{"Label":"C#","Value":"csharp"},{"Label":"TypeScript","Value":"typescript"},{"Label":"Python","Value":"python"},{"Label":"Go","Value":"go"}],"Order":1},
    {"Id":"62ece2a1-b6a1-4934-a92a-7b2939db5703","Name":"yearsOfExperience","Label":"Years of Experience","FieldType":1,"Required":true,"Options":[],"Order":2},
    {"Id":"ba52209e-7380-4a5c-ae93-378d8bbf7b04","Name":"onCallEligible","Label":"On-Call Eligible","FieldType":3,"Required":false,"Options":[],"Order":3},
    {"Id":"7dbebed2-cf31-4f64-99f9-9281c818b505","Name":"lastCodeReviewDate","Label":"Last Code Review Date","FieldType":2,"Required":false,"Options":[],"Order":4},
    {"Id":"8d314b83-9d6f-46e1-90ac-df0735d5e806","Name":"githubUsername","Label":"GitHub Username","FieldType":0,"Required":false,"Options":[],"Order":5}
  ]'::jsonb
);

INSERT INTO "Employee" ("Id", "FirstName", "LastName", "Email", "HireDate", "EndDate", "Department", "EmployeeTypeId", "CreatedDate", "UpdatedDate", "FieldValuesJson")
VALUES
  ('2d44e05c-306f-4a1c-a25a-3cf61a48a001', 'Avery', 'Morgan', 'avery.morgan@example.com', DATE '2021-03-15', NULL, 'Platform Engineering', '4f4e6f6f-0e76-4b97-a3d4-3d3f22d99b01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, '{"primaryLanguage":"csharp","yearsOfExperience":8,"onCallEligible":true,"lastCodeReviewDate":"2026-06-18","githubUsername":"amorgan"}'::jsonb),
  ('2d44e05c-306f-4a1c-a25a-3cf61a48a002', 'Jordan', 'Lee', 'jordan.lee@example.com', DATE '2022-07-11', NULL, 'Product Engineering', '4f4e6f6f-0e76-4b97-a3d4-3d3f22d99b01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, '{"primaryLanguage":"typescript","yearsOfExperience":5,"onCallEligible":true,"lastCodeReviewDate":"2026-06-24","githubUsername":"jlee"}'::jsonb),
  ('2d44e05c-306f-4a1c-a25a-3cf61a48a003', 'Maya', 'Chen', 'maya.chen@example.com', DATE '2020-01-06', NULL, 'Data Platform', '4f4e6f6f-0e76-4b97-a3d4-3d3f22d99b01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, '{"primaryLanguage":"python","yearsOfExperience":9,"onCallEligible":false,"lastCodeReviewDate":"2026-05-29","githubUsername":"mchen"}'::jsonb);

COMMIT;
