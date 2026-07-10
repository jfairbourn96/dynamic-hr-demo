/*
Seeds professional demo employee types for README screenshots.

FieldType values:
  0 = Text
  1 = Number
  2 = Date
  3 = Boolean
  4 = Select
  5 = Address
*/

SET NOCOUNT ON;

DECLARE @now datetime2 = SYSUTCDATETIME();

MERGE [EmployeeType] AS target
USING (VALUES
(
    '4f4e6f6f-0e76-4b97-a3d4-3d3f22d99b01',
    N'Software Engineer',
    N'Engineering role with technical specialization, architecture ownership, and on-call readiness metadata.',
    @now,
    @now,
    N'[
      {
        "Id": "17141c50-6d25-4fd0-b5e2-97c322c4ed01",
        "Name": "primaryLanguage",
        "Label": "Primary Language",
        "FieldType": 4,
        "Required": true,
        "Options": [
          { "Label": "C#", "Value": "csharp" },
          { "Label": "TypeScript", "Value": "typescript" },
          { "Label": "Python", "Value": "python" },
          { "Label": "Java", "Value": "java" },
          { "Label": "Go", "Value": "go" }
        ],
        "Order": 1
      },
      {
        "Id": "62ece2a1-b6a1-4934-a92a-7b2939db5703",
        "Name": "yearsOfExperience",
        "Label": "Years of Experience",
        "FieldType": 1,
        "Required": true,
        "Options": [],
        "Order": 2
      },
      {
        "Id": "ba52209e-7380-4a5c-ae93-378d8bbf7b04",
        "Name": "onCallEligible",
        "Label": "On-Call Eligible",
        "FieldType": 3,
        "Required": false,
        "Options": [],
        "Order": 3
      },
      {
        "Id": "7dbebed2-cf31-4f64-99f9-9281c818b505",
        "Name": "lastCodeReviewDate",
        "Label": "Last Code Review Date",
        "FieldType": 2,
        "Required": false,
        "Options": [],
        "Order": 4
      },
      {
        "Id": "8d314b83-9d6f-46e1-90ac-df0735d5e806",
        "Name": "githubUsername",
        "Label": "GitHub Username",
        "FieldType": 0,
        "Required": false,
        "Options": [],
        "Order": 5
      }
    ]'
),
(
    'e037b20b-4c1b-47a3-bc54-8b870ad4cb02',
    N'Project Manager',
    N'Delivery role with methodology, budget, portfolio, and stakeholder management metadata.',
    @now,
    @now,
    N'[
      {
        "Id": "0b437d52-97a9-4799-9581-a49cb18b4001",
        "Name": "methodology",
        "Label": "Methodology",
        "FieldType": 4,
        "Required": true,
        "Options": [
          { "Label": "Agile", "Value": "agile" },
          { "Label": "Waterfall", "Value": "waterfall" }
        ],
        "Order": 1
      },
      {
        "Id": "37eb28de-258e-4427-a2dc-975792c9aa02",
        "Name": "activeProjectCount",
        "Label": "Active Project Count",
        "FieldType": 1,
        "Required": true,
        "Options": [],
        "Order": 2
      },
      {
        "Id": "6666f80d-59cb-40c6-9b5d-5384cd147b03",
        "Name": "largestBudgetManaged",
        "Label": "Largest Budget Managed",
        "FieldType": 1,
        "Required": false,
        "Options": [],
        "Order": 3
      },
      {
        "Id": "b9952f56-f82f-462d-a1f6-58fbcf44a004",
        "Name": "pmpCertified",
        "Label": "PMP Certified",
        "FieldType": 3,
        "Required": false,
        "Options": [],
        "Order": 4
      },
      {
        "Id": "c75621ee-5f6b-4d71-9809-1d204f87f205",
        "Name": "lastSteeringReview",
        "Label": "Last Steering Review",
        "FieldType": 2,
        "Required": false,
        "Options": [],
        "Order": 5
      },
      {
        "Id": "ed752d9e-afb7-455c-974a-dfa6c10d8106",
        "Name": "stakeholderGroup",
        "Label": "Stakeholder Group",
        "FieldType": 0,
        "Required": false,
        "Options": [],
        "Order": 6
      }
    ]'
),
(
    'c98dbd98-3332-4e72-9933-6793fdc4c303',
    N'Business Analyst',
    N'Analysis role with requirements, reporting, process review, and UAT coordination metadata.',
    @now,
    @now,
    N'[
      {
        "Id": "44c8146a-3038-45dc-a004-622f6c541801",
        "Name": "analysisSpecialty",
        "Label": "Analysis Specialty",
        "FieldType": 4,
        "Required": true,
        "Options": [
          { "Label": "Process Mapping", "Value": "process-mapping" },
          { "Label": "Data Analysis", "Value": "data-analysis" },
          { "Label": "Requirements", "Value": "requirements" },
          { "Label": "Reporting", "Value": "reporting" },
          { "Label": "Compliance", "Value": "compliance" }
        ],
        "Order": 1
      },
      {
        "Id": "2070b9e1-ea87-4875-9cc8-e7c8f4cd2603",
        "Name": "requirementsWorkshopsLed",
        "Label": "Requirements Workshops Led",
        "FieldType": 1,
        "Required": true,
        "Options": [],
        "Order": 2
      },
      {
        "Id": "5f6417d8-3557-4e17-907a-3f2616574c04",
        "Name": "uatCoordinator",
        "Label": "UAT Coordinator",
        "FieldType": 3,
        "Required": false,
        "Options": [],
        "Order": 3
      },
      {
        "Id": "d66ce177-7819-467d-b66e-5a1c524c4805",
        "Name": "lastProcessReview",
        "Label": "Last Process Review",
        "FieldType": 2,
        "Required": false,
        "Options": [],
        "Order": 4
      },
      {
        "Id": "f26002b7-03fc-46ca-af7d-5ef77b3e7706",
        "Name": "primaryBusinessUnit",
        "Label": "Primary Business Unit",
        "FieldType": 0,
        "Required": false,
        "Options": [],
        "Order": 5
      }
    ]'
)) AS source ([Id], [Name], [Description], [CreatedDate], [UpdatedDate], [FieldsJson])
ON target.[Name] = source.[Name]
WHEN MATCHED THEN
    UPDATE SET
        [Description] = source.[Description],
        [UpdatedDate] = source.[UpdatedDate],
        [FieldsJson] = source.[FieldsJson]
WHEN NOT MATCHED THEN
    INSERT ([Id], [Name], [Description], [CreatedDate], [UpdatedDate], [FieldsJson])
    VALUES (source.[Id], source.[Name], source.[Description], source.[CreatedDate], source.[UpdatedDate], source.[FieldsJson]);

SELECT
    [Name],
    [Description],
    JSON_VALUE([FieldsJson], '$[0].Label') AS [FirstField]
FROM [EmployeeType]
WHERE [Name] IN (
    N'Software Engineer',
    N'Project Manager',
    N'Business Analyst'
)
ORDER BY [Name];
