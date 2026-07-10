/*
Seeds professional demo employees for README screenshots.

Run scripts/seed-professional-employee-types.sql first so the three employee
types exist. This script is rerunnable and upserts by email address.
*/

SET NOCOUNT ON;

DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @softwareEngineerTypeId uniqueidentifier;
DECLARE @projectManagerTypeId uniqueidentifier;
DECLARE @businessAnalystTypeId uniqueidentifier;

SELECT @softwareEngineerTypeId = [Id]
FROM [EmployeeType]
WHERE [Name] = N'Software Engineer';

SELECT @projectManagerTypeId = [Id]
FROM [EmployeeType]
WHERE [Name] = N'Project Manager';

SELECT @businessAnalystTypeId = [Id]
FROM [EmployeeType]
WHERE [Name] = N'Business Analyst';

IF @softwareEngineerTypeId IS NULL
    THROW 51000, 'Missing employee type: Software Engineer. Run seed-professional-employee-types.sql first.', 1;

IF @projectManagerTypeId IS NULL
    THROW 51000, 'Missing employee type: Project Manager. Run seed-professional-employee-types.sql first.', 1;

IF @businessAnalystTypeId IS NULL
    THROW 51000, 'Missing employee type: Business Analyst. Run seed-professional-employee-types.sql first.', 1;

MERGE [Employee] AS target
USING (VALUES
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a001',
        N'Avery',
        N'Morgan',
        N'avery.morgan@example.com',
        CONVERT(date, '2021-03-15'),
        NULL,
        N'Platform Engineering',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"csharp","yearsOfExperience":8,"onCallEligible":true,"lastCodeReviewDate":"2026-06-18","githubUsername":"amorgan"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a002',
        N'Jordan',
        N'Lee',
        N'jordan.lee@example.com',
        CONVERT(date, '2022-07-11'),
        NULL,
        N'Product Engineering',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"typescript","yearsOfExperience":5,"onCallEligible":true,"lastCodeReviewDate":"2026-06-24","githubUsername":"jlee"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a003',
        N'Maya',
        N'Chen',
        N'maya.chen@example.com',
        CONVERT(date, '2020-01-06'),
        NULL,
        N'Data Platform',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"python","yearsOfExperience":9,"onCallEligible":false,"lastCodeReviewDate":"2026-05-29","githubUsername":"mchen"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a004',
        N'Carlos',
        N'Rivera',
        N'carlos.rivera@example.com',
        CONVERT(date, '2019-10-21'),
        NULL,
        N'Infrastructure',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"go","yearsOfExperience":11,"onCallEligible":true,"lastCodeReviewDate":"2026-06-11","githubUsername":"crivera"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a005',
        N'Priya',
        N'Shah',
        N'priya.shah@example.com',
        CONVERT(date, '2023-02-13'),
        NULL,
        N'Product Engineering',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"csharp","yearsOfExperience":4,"onCallEligible":false,"lastCodeReviewDate":"2026-06-20","githubUsername":"pshah"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a006',
        N'Ethan',
        N'Brooks',
        N'ethan.brooks@example.com',
        CONVERT(date, '2018-05-07'),
        NULL,
        N'Platform Engineering',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"java","yearsOfExperience":12,"onCallEligible":true,"lastCodeReviewDate":"2026-06-02","githubUsername":"ebrooks"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a007',
        N'Sofia',
        N'Martinez',
        N'sofia.martinez@example.com',
        CONVERT(date, '2024-04-01'),
        NULL,
        N'Internal Tools',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"typescript","yearsOfExperience":3,"onCallEligible":false,"lastCodeReviewDate":"2026-06-27","githubUsername":"smartinez"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a008',
        N'Noah',
        N'Patel',
        N'noah.patel@example.com',
        CONVERT(date, '2021-11-29'),
        NULL,
        N'Data Platform',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"python","yearsOfExperience":6,"onCallEligible":true,"lastCodeReviewDate":"2026-06-14","githubUsername":"npatel"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a009',
        N'Grace',
        N'Kim',
        N'grace.kim@example.com',
        CONVERT(date, '2020-08-17'),
        NULL,
        N'Infrastructure',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"go","yearsOfExperience":7,"onCallEligible":true,"lastCodeReviewDate":"2026-05-31","githubUsername":"gkim"}'
    ),
    (
        '2d44e05c-306f-4a1c-a25a-3cf61a48a010',
        N'Liam',
        N'Nguyen',
        N'liam.nguyen@example.com',
        CONVERT(date, '2022-09-19'),
        NULL,
        N'Internal Tools',
        @softwareEngineerTypeId,
        N'{"primaryLanguage":"csharp","yearsOfExperience":5,"onCallEligible":false,"lastCodeReviewDate":"2026-06-21","githubUsername":"lnguyen"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb001',
        N'Olivia',
        N'Bennett',
        N'olivia.bennett@example.com',
        CONVERT(date, '2017-02-20'),
        NULL,
        N'Enterprise Delivery',
        @projectManagerTypeId,
        N'{"methodology":"agile","activeProjectCount":4,"largestBudgetManaged":1250000,"pmpCertified":true,"lastSteeringReview":"2026-06-25","stakeholderGroup":"Enterprise Operations"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb002',
        N'Marcus',
        N'Johnson',
        N'marcus.johnson@example.com',
        CONVERT(date, '2019-06-03'),
        NULL,
        N'Technology Delivery',
        @projectManagerTypeId,
        N'{"methodology":"agile","activeProjectCount":3,"largestBudgetManaged":850000,"pmpCertified":true,"lastSteeringReview":"2026-06-19","stakeholderGroup":"Product Leadership"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb003',
        N'Natalie',
        N'Wright',
        N'natalie.wright@example.com',
        CONVERT(date, '2021-01-12'),
        NULL,
        N'Compliance Programs',
        @projectManagerTypeId,
        N'{"methodology":"waterfall","activeProjectCount":2,"largestBudgetManaged":620000,"pmpCertified":false,"lastSteeringReview":"2026-05-30","stakeholderGroup":"Compliance Office"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb004',
        N'Daniel',
        N'Foster',
        N'daniel.foster@example.com',
        CONVERT(date, '2018-12-10'),
        NULL,
        N'Enterprise Delivery',
        @projectManagerTypeId,
        N'{"methodology":"agile","activeProjectCount":5,"largestBudgetManaged":1750000,"pmpCertified":true,"lastSteeringReview":"2026-06-12","stakeholderGroup":"Enterprise Operations"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb005',
        N'Isabella',
        N'Rossi',
        N'isabella.rossi@example.com',
        CONVERT(date, '2023-03-27'),
        NULL,
        N'Operations Projects',
        @projectManagerTypeId,
        N'{"methodology":"waterfall","activeProjectCount":3,"largestBudgetManaged":440000,"pmpCertified":false,"lastSteeringReview":"2026-06-03","stakeholderGroup":"Field Operations"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb006',
        N'Benjamin',
        N'Carter',
        N'benjamin.carter@example.com',
        CONVERT(date, '2016-09-06'),
        NULL,
        N'Technology Delivery',
        @projectManagerTypeId,
        N'{"methodology":"agile","activeProjectCount":6,"largestBudgetManaged":2100000,"pmpCertified":true,"lastSteeringReview":"2026-06-28","stakeholderGroup":"Product Leadership"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb007',
        N'Chloe',
        N'Anderson',
        N'chloe.anderson@example.com',
        CONVERT(date, '2020-04-14'),
        NULL,
        N'Compliance Programs',
        @projectManagerTypeId,
        N'{"methodology":"agile","activeProjectCount":2,"largestBudgetManaged":700000,"pmpCertified":true,"lastSteeringReview":"2026-06-07","stakeholderGroup":"Compliance Office"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb008',
        N'Victor',
        N'Hughes',
        N'victor.hughes@example.com',
        CONVERT(date, '2022-10-24'),
        NULL,
        N'Operations Projects',
        @projectManagerTypeId,
        N'{"methodology":"agile","activeProjectCount":4,"largestBudgetManaged":980000,"pmpCertified":false,"lastSteeringReview":"2026-05-24","stakeholderGroup":"Field Operations"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb009',
        N'Hannah',
        N'Price',
        N'hannah.price@example.com',
        CONVERT(date, '2019-11-18'),
        NULL,
        N'Enterprise Delivery',
        @projectManagerTypeId,
        N'{"methodology":"waterfall","activeProjectCount":1,"largestBudgetManaged":1350000,"pmpCertified":true,"lastSteeringReview":"2026-06-17","stakeholderGroup":"Enterprise Operations"}'
    ),
    (
        '3c133dc1-c8de-40f7-8f70-d5891acbb010',
        N'Miles',
        N'Cooper',
        N'miles.cooper@example.com',
        CONVERT(date, '2024-01-08'),
        NULL,
        N'Technology Delivery',
        @projectManagerTypeId,
        N'{"methodology":"waterfall","activeProjectCount":2,"largestBudgetManaged":390000,"pmpCertified":false,"lastSteeringReview":"2026-06-22","stakeholderGroup":"Product Leadership"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb001',
        N'Emma',
        N'Thompson',
        N'emma.thompson@example.com',
        CONVERT(date, '2021-05-10'),
        NULL,
        N'Business Operations',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"process-mapping","requirementsWorkshopsLed":18,"uatCoordinator":true,"lastProcessReview":"2026-06-10","primaryBusinessUnit":"Finance Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb002',
        N'Lucas',
        N'Garcia',
        N'lucas.garcia@example.com',
        CONVERT(date, '2020-02-03'),
        NULL,
        N'Data Insights',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"data-analysis","requirementsWorkshopsLed":24,"uatCoordinator":false,"lastProcessReview":"2026-05-22","primaryBusinessUnit":"Sales Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb003',
        N'Zoe',
        N'Mitchell',
        N'zoe.mitchell@example.com',
        CONVERT(date, '2022-08-15'),
        NULL,
        N'Business Operations',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"requirements","requirementsWorkshopsLed":15,"uatCoordinator":true,"lastProcessReview":"2026-06-01","primaryBusinessUnit":"Finance Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb004',
        N'Adrian',
        N'Nelson',
        N'adrian.nelson@example.com',
        CONVERT(date, '2019-04-29'),
        NULL,
        N'Risk and Compliance',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"compliance","requirementsWorkshopsLed":21,"uatCoordinator":true,"lastProcessReview":"2026-05-18","primaryBusinessUnit":"Compliance Office"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb005',
        N'Layla',
        N'Walker',
        N'layla.walker@example.com',
        CONVERT(date, '2023-09-05'),
        NULL,
        N'Data Insights',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"reporting","requirementsWorkshopsLed":9,"uatCoordinator":false,"lastProcessReview":"2026-06-20","primaryBusinessUnit":"Sales Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb006',
        N'Julian',
        N'Reed',
        N'julian.reed@example.com',
        CONVERT(date, '2018-07-23'),
        NULL,
        N'Process Excellence',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"process-mapping","requirementsWorkshopsLed":31,"uatCoordinator":true,"lastProcessReview":"2026-06-14","primaryBusinessUnit":"Enterprise Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb007',
        N'Nora',
        N'Phillips',
        N'nora.phillips@example.com',
        CONVERT(date, '2024-02-26'),
        NULL,
        N'Business Operations',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"requirements","requirementsWorkshopsLed":7,"uatCoordinator":false,"lastProcessReview":"2026-05-27","primaryBusinessUnit":"Finance Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb008',
        N'Samuel',
        N'Evans',
        N'samuel.evans@example.com',
        CONVERT(date, '2020-10-12'),
        NULL,
        N'Risk and Compliance',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"compliance","requirementsWorkshopsLed":19,"uatCoordinator":true,"lastProcessReview":"2026-06-05","primaryBusinessUnit":"Compliance Office"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb009',
        N'Violet',
        N'Scott',
        N'violet.scott@example.com',
        CONVERT(date, '2021-12-06'),
        NULL,
        N'Process Excellence',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"data-analysis","requirementsWorkshopsLed":14,"uatCoordinator":false,"lastProcessReview":"2026-06-23","primaryBusinessUnit":"Enterprise Operations"}'
    ),
    (
        '9273e295-3342-42c8-b9b0-5a33ca2cb010',
        N'Owen',
        N'Bailey',
        N'owen.bailey@example.com',
        CONVERT(date, '2022-03-21'),
        NULL,
        N'Data Insights',
        @businessAnalystTypeId,
        N'{"analysisSpecialty":"reporting","requirementsWorkshopsLed":12,"uatCoordinator":true,"lastProcessReview":"2026-06-16","primaryBusinessUnit":"Sales Operations"}'
    )
) AS source (
    [Id],
    [FirstName],
    [LastName],
    [Email],
    [HireDate],
    [EndDate],
    [Department],
    [EmployeeTypeId],
    [FieldValuesJson]
)
ON target.[Email] = source.[Email]
WHEN MATCHED THEN
    UPDATE SET
        [FirstName] = source.[FirstName],
        [LastName] = source.[LastName],
        [HireDate] = source.[HireDate],
        [EndDate] = source.[EndDate],
        [Department] = source.[Department],
        [EmployeeTypeId] = source.[EmployeeTypeId],
        [UpdatedDate] = @now,
        [FieldValuesJson] = source.[FieldValuesJson]
WHEN NOT MATCHED THEN
    INSERT (
        [Id],
        [FirstName],
        [LastName],
        [Email],
        [HireDate],
        [EndDate],
        [Department],
        [EmployeeTypeId],
        [CreatedDate],
        [UpdatedDate],
        [FieldValuesJson]
    )
    VALUES (
        source.[Id],
        source.[FirstName],
        source.[LastName],
        source.[Email],
        source.[HireDate],
        source.[EndDate],
        source.[Department],
        source.[EmployeeTypeId],
        @now,
        @now,
        source.[FieldValuesJson]
    );

SELECT
    employeeType.[Name] AS [EmployeeType],
    COUNT(*) AS [EmployeeCount]
FROM [Employee] employee
INNER JOIN [EmployeeType] employeeType
    ON employee.[EmployeeTypeId] = employeeType.[Id]
WHERE employeeType.[Name] IN (
    N'Software Engineer',
    N'Project Manager',
    N'Business Analyst'
)
GROUP BY employeeType.[Name]
ORDER BY employeeType.[Name];
