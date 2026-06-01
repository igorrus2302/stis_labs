using System.Collections.Generic;
using System.Linq;

namespace University.stis_lab2;

public sealed class UniversityLabData
{
    private UniversityLabData() { }

    public Concept AcademicParticipant  { get; private init; } = null!;
    public Concept Student              { get; private init; } = null!;
    public Concept GraduateStudent      { get; private init; } = null!;
    public Concept Teacher              { get; private init; } = null!;
    public Concept Professor            { get; private init; } = null!;

    public Concept AcademicEvent        { get; private init; } = null!;
    public Concept Course               { get; private init; } = null!;
    public Concept Seminar              { get; private init; } = null!;
    public Concept LectureCourse        { get; private init; } = null!;

    public RelationConcept EnrolledIn       { get; private init; } = null!;  // обучаться на
    public RelationConcept RegisteredFor    { get; private init; } = null!;  // записан на курс
    public RelationConcept AttendsSeminar   { get; private init; } = null!;  // посещать семинар
    public RelationConcept Teaches          { get; private init; } = null!;  // вести занятие
    public RelationConcept DeliversLectures { get; private init; } = null!;  // читать лекции

    public Frame IvanPetrov       { get; private init; } = null!;  // студент
    public Frame MariaVolkova     { get; private init; } = null!;  // аспирант
    public Frame DrSmirнov        { get; private init; } = null!;  // преподаватель
    public Frame ProfKorolev      { get; private init; } = null!;  // профессор
    public Frame MathAnalysis     { get; private init; } = null!;  // лекционный курс
    public Frame LinearAlgebra    { get; private init; } = null!;  // курс
    public Frame AlgebraSeminar   { get; private init; } = null!;  // семинар

    public RelationFrame PetrovAtAlgebra         { get; private init; } = null!;
    public RelationFrame VolkovaAtSeminar        { get; private init; } = null!;
    public RelationFrame SmirnovTeachesAlgebra   { get; private init; } = null!;
    public RelationFrame KorolevLecturesMath     { get; private init; } = null!;

    public IReadOnlyList<Concept>         Concepts         { get; private init; } = [];
    public IReadOnlyList<Frame>           Frames           { get; private init; } = [];
    public IReadOnlyList<RelationConcept> RelationConcepts { get; private init; } = [];
    public IReadOnlyList<RelationFrame>   RelationFrames   { get; private init; } = [];

    public static UniversityLabData Create()
    {
        var academicParticipant = new Concept("Участник учебного процесса", [
            new AttributeDescription("fullName",   typeof(string)),
            new AttributeDescription("email",      typeof(string))
        ]);

        var student = new Concept("Студент", [
            new AttributeDescription("fullName",   typeof(string)),
            new AttributeDescription("email",      typeof(string)),
            new AttributeDescription("studentId",  typeof(string)),
            new AttributeDescription("groupName",  typeof(string))
        ]);

        var graduateStudent = new Concept("Аспирант", [
            new AttributeDescription("fullName",       typeof(string)),
            new AttributeDescription("email",          typeof(string)),
            new AttributeDescription("studentId",      typeof(string)),
            new AttributeDescription("groupName",      typeof(string)),
            new AttributeDescription("supervisorName", typeof(string))
        ]);

        var teacher = new Concept("Преподаватель", [
            new AttributeDescription("fullName",     typeof(string)),
            new AttributeDescription("email",        typeof(string)),
            new AttributeDescription("department",   typeof(string)),
            new AttributeDescription("academicRank", typeof(string))
        ]);

        var professor = new Concept("Профессор", [
            new AttributeDescription("fullName",           typeof(string)),
            new AttributeDescription("email",              typeof(string)),
            new AttributeDescription("department",         typeof(string)),
            new AttributeDescription("academicRank",       typeof(string)),
            new AttributeDescription("dissertationsCount", typeof(int))
        ]);

        var academicEvent = new Concept("Учебное мероприятие", [
            new AttributeDescription("title",    typeof(string)),
            new AttributeDescription("semester", typeof(string))
        ]);

        var course = new Concept("Курс", [
            new AttributeDescription("title",      typeof(string)),
            new AttributeDescription("semester",   typeof(string)),
            new AttributeDescription("courseCode", typeof(string)),
            new AttributeDescription("credits",    typeof(int))
        ]);

        var seminar = new Concept("Семинар", [
            new AttributeDescription("title",           typeof(string)),
            new AttributeDescription("semester",        typeof(string)),
            new AttributeDescription("courseCode",      typeof(string)),
            new AttributeDescription("credits",         typeof(int)),
            new AttributeDescription("maxParticipants", typeof(int))
        ]);

        var lectureCourse = new Concept("Лекционный курс", [
            new AttributeDescription("title",        typeof(string)),
            new AttributeDescription("semester",     typeof(string)),
            new AttributeDescription("courseCode",   typeof(string)),
            new AttributeDescription("credits",      typeof(int)),
            new AttributeDescription("lectureHours", typeof(int))
        ]);

        var enrolledIn = new RelationConcept(
            "обучается на", academicParticipant, academicEvent);

        var registeredFor = new RelationConcept(
            "записан на курс", student, course);

        var attendsSeminar = new RelationConcept(
            "посещает семинар", student, seminar);

        var teaches = new RelationConcept(
            "ведёт занятие", teacher, academicEvent);

        var deliversLectures = new RelationConcept(
            "читает лекции", professor, lectureCourse);

        var ivanPetrov    = new Frame("Иван Петров",     student);
        var mariaVolkova  = new Frame("Мария Волкова",   graduateStudent);
        var drSmirnov     = new Frame("Доцент Смирнов",  teacher);
        var profKorolev   = new Frame("Профессор Королёв", professor);
        var mathAnalysis  = new Frame("Математический анализ", lectureCourse);
        var linearAlgebra = new Frame("Линейная алгебра", course);
        var algebraSem    = new Frame("Семинар по алгебре", seminar);

        var petrovAtAlgebra =
            new RelationFrame(registeredFor, ivanPetrov, linearAlgebra);

        var volkovaAtSeminar =
            new RelationFrame(attendsSeminar, mariaVolkova, algebraSem);

        var smirnovTeachesAlgebra =
            new RelationFrame(teaches, drSmirnov, linearAlgebra);

        var korolevLecturesMath =
            new RelationFrame(deliversLectures, profKorolev, mathAnalysis);

        return new UniversityLabData
        {
            AcademicParticipant  = academicParticipant,
            Student              = student,
            GraduateStudent      = graduateStudent,
            Teacher              = teacher,
            Professor            = professor,
            AcademicEvent        = academicEvent,
            Course               = course,
            Seminar              = seminar,
            LectureCourse        = lectureCourse,

            EnrolledIn       = enrolledIn,
            RegisteredFor    = registeredFor,
            AttendsSeminar   = attendsSeminar,
            Teaches          = teaches,
            DeliversLectures = deliversLectures,

            IvanPetrov      = ivanPetrov,
            MariaVolkova    = mariaVolkova,
            DrSmirнov       = drSmirnov,
            ProfKorolev     = profKorolev,
            MathAnalysis    = mathAnalysis,
            LinearAlgebra   = linearAlgebra,
            AlgebraSeminar  = algebraSem,

            PetrovAtAlgebra       = petrovAtAlgebra,
            VolkovaAtSeminar      = volkovaAtSeminar,
            SmirnovTeachesAlgebra = smirnovTeachesAlgebra,
            KorolevLecturesMath   = korolevLecturesMath,

            Concepts = [
                academicParticipant, student, graduateStudent,
                teacher, professor,
                academicEvent, course, seminar, lectureCourse
            ],
            Frames = [
                ivanPetrov, mariaVolkova, drSmirnov, profKorolev,
                mathAnalysis, linearAlgebra, algebraSem
            ],
            RelationConcepts = [
                enrolledIn, registeredFor, attendsSeminar,
                teaches, deliversLectures
            ],
            RelationFrames = [
                petrovAtAlgebra, volkovaAtSeminar,
                smirnovTeachesAlgebra, korolevLecturesMath
            ]
        };
    }

    public IEnumerable<string> BuildChecks()
    {
        // ISA между концептами
        yield return Check("Студент ISA Участник учебного процесса",
            Student.IsSubconceptOf(AcademicParticipant));

        yield return Check("Аспирант ISA Участник учебного процесса",
            GraduateStudent.IsSubconceptOf(AcademicParticipant));

        yield return Check("Профессор ISA Преподаватель",
            Professor.IsSubconceptOf(Teacher));

        yield return Check("Профессор ISA Участник учебного процесса",
            Professor.IsSubconceptOf(AcademicParticipant));

        yield return Check("Семинар ISA Курс",
            Seminar.IsSubconceptOf(Course));

        yield return Check("Семинар ISA Учебное мероприятие",
            Seminar.IsSubconceptOf(AcademicEvent));

        yield return Check("Лекционный курс ISA Курс",
            LectureCourse.IsSubconceptOf(Course));

        yield return Check("Учебное мероприятие НЕ является Студентом",
            !AcademicEvent.IsSubconceptOf(Student));

        // instance-of фреймов
        yield return Check("Иван Петров instance-of Студент",
            IvanPetrov.IsInstanceOf(Student));

        yield return Check("Иван Петров instance-of Участник учебного процесса",
            IvanPetrov.IsInstanceOf(AcademicParticipant));

        yield return Check("Мария Волкова instance-of Аспирант",
            MariaVolkova.IsInstanceOf(GraduateStudent));

        yield return Check("Мария Волкова instance-of Студент",
            MariaVolkova.IsInstanceOf(Student));

        yield return Check("Профессор Королёв instance-of Профессор",
            ProfKorolev.IsInstanceOf(Professor));

        yield return Check("Профессор Королёв instance-of Преподаватель",
            ProfKorolev.IsInstanceOf(Teacher));

        yield return Check("Семинар по алгебре instance-of Семинар",
            AlgebraSeminar.IsInstanceOf(Seminar));

        yield return Check("Семинар по алгебре instance-of Курс",
            AlgebraSeminar.IsInstanceOf(Course));

        yield return Check("Иван Петров НЕ является Преподавателем",
            !IvanPetrov.IsInstanceOf(Teacher));

        yield return Check("Семинар по алгебре НЕ является Лекционным курсом",
            !AlgebraSeminar.IsInstanceOf(LectureCourse));

        // ISA между связями
        yield return Check("записан на курс ISA обучается на",
            RegisteredFor.IsSubrelationOf(EnrolledIn));

        yield return Check("посещает семинар ISA записан на курс",
            AttendsSeminar.IsSubrelationOf(RegisteredFor));

        yield return Check("посещает семинар ISA обучается на",
            AttendsSeminar.IsSubrelationOf(EnrolledIn));

        yield return Check("читает лекции ISA ведёт занятие",
            DeliversLectures.IsSubrelationOf(Teaches));

        yield return Check("ведёт занятие НЕ является подотношением обучается на",
            !Teaches.IsSubrelationOf(EnrolledIn));

        // instance-of фреймов-связей
        yield return Check("Иван Петров записан на Линейную алгебру",
            PetrovAtAlgebra.IsInstanceOf(RegisteredFor));

        yield return Check("Иван Петров обучается на Линейной алгебре",
            PetrovAtAlgebra.IsInstanceOf(EnrolledIn));

        yield return Check("Мария Волкова посещает Семинар по алгебре",
            VolkovaAtSeminar.IsInstanceOf(AttendsSeminar));

        yield return Check("Мария Волкова обучается на семинаре",
            VolkovaAtSeminar.IsInstanceOf(EnrolledIn));

        yield return Check("Доцент Смирнов ведёт занятие по Линейной алгебре",
            SmirnovTeachesAlgebra.IsInstanceOf(Teaches));

        yield return Check("Профессор Королёв читает лекции по Мат. анализу",
            KorolevLecturesMath.IsInstanceOf(DeliversLectures));

        yield return Check("Профессор Королёв ведёт занятие",
            KorolevLecturesMath.IsInstanceOf(Teaches));
    }

    public string DescribeConcepts()
    {
        return string.Join("\n", Concepts.Select(c =>
        {
            var parents = GetAttributeBasedParents(c).ToList();
            var isa = parents.Count == 0
                ? "базовое понятие"
                : $"ISA  {string.Join(", ", parents.Select(p => p.Name))}";
            var attrs = string.Join(", ", c.Attributes.Select(a => a.Name));
            return $"  {c.Name}: {isa}\n атрибуты: [{attrs}]";
        }));
    }

    public string DescribeFrames()
    {
        return string.Join("\n",
            Frames.Select(f => $"  {f.Name}: instance-of {f.Concept.Name}"));
    }

    public string DescribeRelations()
    {
        return string.Join("\n", RelationConcepts.Select(r =>
        {
            var parents = GetSignatureBasedParents(r).ToList();
            var isa = parents.Count == 0
                ? string.Empty
                : $"; ISA {string.Join(", ", parents.Select(p => p.Name))}";
            return $"  {r.Name}({r.Domain.Name} → {r.Range.Name}){isa}";
        }));
    }

    public string DescribeRelationFrames()
    {
        return string.Join("\n",
            RelationFrames.Select(rf =>
                $"  {rf.Relation.Name}({rf.Subject.Name}, {rf.Object.Name})"));
    }

    public string DescribeSubjectArea() =>
        "Фрагмент предметной области: учебный процесс в университете.\n"
      + "В модели выделены участники процесса (студент, аспирант, преподаватель, профессор)\n"
      + "и учебные мероприятия (курс, семинар, лекционный курс).\n";


    private static string Check(string title, bool result) =>
        $"{(result ? "OK  " : "FAIL")} | {title}";

    private IEnumerable<Concept> GetAttributeBasedParents(Concept concept)
    {
        var candidates = Concepts
            .Where(c => !ReferenceEquals(c, concept))
            .Where(concept.IsSubconceptOf)
            .ToList();

        return candidates.Where(c =>
            !candidates.Any(m => !ReferenceEquals(m, c) && m.IsSubconceptOf(c)));
    }

    private IEnumerable<RelationConcept> GetSignatureBasedParents(RelationConcept relation)
    {
        var candidates = RelationConcepts
            .Where(r => !ReferenceEquals(r, relation))
            .Where(relation.IsSubrelationOf)
            .ToList();

        return candidates.Where(c =>
            !candidates.Any(m => !ReferenceEquals(m, c) && m.IsSubrelationOf(c)));
    }
}