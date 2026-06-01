using System;
using University.stis_lab2;

var data = UniversityLabData.Create();

Console.WriteLine("Предметная область");
Console.WriteLine(data.DescribeSubjectArea());

Console.WriteLine("\nСистема понятий");
Console.WriteLine(data.DescribeConcepts());

Console.WriteLine("\nЭкземпляры");
Console.WriteLine(data.DescribeFrames());

Console.WriteLine("\nСвязи предметной области");
Console.WriteLine(data.DescribeRelations());

Console.WriteLine("\nЭкземпляры связей");
Console.WriteLine(data.DescribeRelationFrames());

Console.WriteLine("\nПроверки корректности");
foreach (var line in data.BuildChecks())
    Console.WriteLine(line);