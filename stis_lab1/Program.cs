using System;
using System.Collections.Generic;
using System.Linq;


class Concept
{
    public string Name { get; }
    private readonly List<(string Name, Type Type, object? Default)> _slots = new();

    public Concept(string name) => Name = name;

    public void AddSlot(string name, Type type, object? def = null)
        => _slots.Add((name, type, def));

    public IReadOnlyList<(string Name, Type Type, object? Default)> Slots => _slots;

    // проверка на ISA
    public bool IsA(Concept other)
    {
        if (this == other) return true;
        foreach (var bs in other.Slots)
        {
            var own = _slots.FirstOrDefault(s => s.Name == bs.Name);
            if (own.Name == null) return false;            
            if (!bs.Type.IsAssignableFrom(own.Type)) return false; 
        }
        return true;
    }

    // цепочка ISA
    public IEnumerable<Concept> AncestorChain(ConceptRegistry reg) =>
        reg.All.Where(c => this.IsA(c));

    public override string ToString() => Name;
}


class ConceptRegistry
{
    private readonly List<Concept> _concepts = new();
    public IReadOnlyList<Concept> All => _concepts;

    public void Register(Concept c) => _concepts.Add(c);

    // ближайший предок
    public IEnumerable<Concept> DirectParents(Concept c)
    {
        var ancestors = _concepts.Where(a => a != c && c.IsA(a)).ToList();
        return ancestors.Where(a => !ancestors.Any(b => b != a && b.IsA(a)));
    }
}


class PossibleWorld
{
    public string Name { get; }
    private readonly Dictionary<string, (Concept Concept, Dictionary<string, object?> Slots)> _instances = new();

    public PossibleWorld(string name) => Name = name;

    // добавить экземпляр мира
    public void AddInstance(string id, Concept concept)
    {
        var slots = new Dictionary<string, object?>();
        foreach (var s in concept.Slots)
            if (s.Default != null) slots[s.Name] = s.Default;
        _instances[id] = (concept, slots);
    }

    // установить слоты для экземпляра в мире
    public void SetSlot(string id, string slotName, object value)
    {
        if (!_instances.TryGetValue(id, out var inst))
            throw new ArgumentException($"Экземпляр '{id}' не найден в мире '{Name}'");
        var slotDef = inst.Concept.Slots.FirstOrDefault(s => s.Name == slotName);
        if (slotDef.Name == null)
            throw new ArgumentException($"Слот '{slotName}' не найден в концепте '{inst.Concept.Name}'");
        inst.Slots[slotName] = value;
    }

    // получить слот экземпляра
    public object? GetSlot(string id, string slotName) =>
        _instances.TryGetValue(id, out var inst) && inst.Slots.TryGetValue(slotName, out var v) ? v : null;

    // instance-of
    public bool InstanceOf(string id, Concept target) =>
        _instances.TryGetValue(id, out var inst) && inst.Concept.IsA(target);

    public IEnumerable<string> AllInstancesOf(Concept target) =>
        _instances.Keys.Where(id => InstanceOf(id, target));

    public Concept? GetConcept(string id) =>
        _instances.TryGetValue(id, out var inst) ? inst.Concept : null;

    public IReadOnlyCollection<string> InstanceIds => _instances.Keys;

    public override string ToString() => $"World({Name})";
}


class KripkeFrame
{
    private readonly Dictionary<string, PossibleWorld> _worlds = new();
    private readonly Dictionary<string, List<string>> _accessibility = new();

    // добавление мира
    public void AddWorld(PossibleWorld world)
    {
        _worlds[world.Name] = world;
        _accessibility[world.Name] = new List<string>();
    }

    // добавить связь между мирами
    public void AddAccessibility(string from, string to)
    {
        if (_accessibility.TryGetValue(from, out var list))
            list.Add(to);
    }

    public PossibleWorld GetWorld(string name) => _worlds[name];

    // получение доступных миров из данного
    public IEnumerable<PossibleWorld> GetAccessibleWorlds(string from) =>
        _accessibility.TryGetValue(from, out var names)
            ? names.Select(n => _worlds[n])
            : Enumerable.Empty<PossibleWorld>();

    public IReadOnlyCollection<PossibleWorld> Worlds => _worlds.Values;
}

class RelationConcept
{
    public string Name { get; }
    public Concept Domain { get; }
    public Concept Range  { get; }

    public RelationConcept(string name, Concept domain, Concept range)
    {
        Name = name; Domain = domain; Range = range;
    }

    // ISA для связей
    public bool IsA(RelationConcept other) =>
        this == other ||
        (this.Domain.IsA(other.Domain) && this.Range.IsA(other.Range));

    public IEnumerable<RelationConcept> AncestorChain(RelationRegistry reg) =>
        reg.All.Where(r => this.IsA(r));

    public override string ToString() => $"{Name}({Domain.Name} -> {Range.Name})";
}


class RelationInstance
{
    public string Id           { get; }
    public RelationConcept Rel { get; }
    public string FromId       { get; }
    public string ToId         { get; }

    public RelationInstance(string id, RelationConcept rel, string fromId, string toId)
    {
        Id = id; Rel = rel; FromId = fromId; ToId = toId;
    }

    public override string ToString() =>
        $"Relation({Id}: {FromId} --[{Rel.Name}]--> {ToId})";
}


class RelationRegistry
{
    private readonly Dictionary<string, RelationConcept> _concepts = new();
    private readonly List<RelationInstance> _instances = new();

    public IReadOnlyCollection<RelationConcept> All => _concepts.Values;

    public void Register(RelationConcept r) => _concepts[r.Name] = r;

    public RelationInstance AddInstance(string id, string relName, string fromId, string toId)
    {
        var inst = new RelationInstance(id, _concepts[relName], fromId, toId);
        _instances.Add(inst);
        return inst;
    }

    public bool InstanceOf(RelationInstance inst, RelationConcept target) =>
        inst.Rel.IsA(target);

    public IEnumerable<RelationInstance> AllInstancesOf(RelationConcept target) =>
        _instances.Where(i => InstanceOf(i, target));
}

class Program
{
    static void Header(string s)
    {
        Console.WriteLine();
        Console.WriteLine($"  {s}");
    }

    static void Check(string label, bool result) =>
        Console.WriteLine($"  [{(result ? "✓" : "✗")}] {label}: {result}");

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Header("Система концептов");

        var cЧеловек = new Concept("Человек");
        cЧеловек.AddSlot("Имя",          typeof(string));
        cЧеловек.AddSlot("ДатаРождения", typeof(DateTime));
        cЧеловек.AddSlot("Email",        typeof(string));

        var cСтудент = new Concept("Студент");
        cСтудент.AddSlot("Имя",          typeof(string));
        cСтудент.AddSlot("ДатаРождения", typeof(DateTime));
        cСтудент.AddSlot("Email",        typeof(string));
        cСтудент.AddSlot("НомерЗачётки", typeof(string));
        cСтудент.AddSlot("КурсОбучения", typeof(int));

        var cАспирант = new Concept("Аспирант");
        cАспирант.AddSlot("Имя",                  typeof(string));
        cАспирант.AddSlot("ДатаРождения",          typeof(DateTime));
        cАспирант.AddSlot("Email",                 typeof(string));
        cАспирант.AddSlot("НомерЗачётки",          typeof(string));
        cАспирант.AddSlot("КурсОбучения",          typeof(int));
        cАспирант.AddSlot("НаучныйРуководитель",   typeof(string));
        cАспирант.AddSlot("ТемаДиссертации",       typeof(string));

        var cПреподаватель = new Concept("Преподаватель");
        cПреподаватель.AddSlot("Имя",            typeof(string));
        cПреподаватель.AddSlot("ДатаРождения",   typeof(DateTime));
        cПреподаватель.AddSlot("Email",          typeof(string));
        cПреподаватель.AddSlot("Должность",      typeof(string), def: "Ассистент");
        cПреподаватель.AddSlot("УчёнаяСтепень",  typeof(string));

        var cКурс = new Concept("Курс");
        cКурс.AddSlot("Название", typeof(string));
        cКурс.AddSlot("Часы",     typeof(int));
        cКурс.AddSlot("Семестр",  typeof(string));

        var cКафедра = new Concept("Кафедра");
        cКафедра.AddSlot("Название",     typeof(string));
        cКафедра.AddSlot("Аббревиатура", typeof(string));

        var reg = new ConceptRegistry();
        foreach (var c in new[] { cЧеловек, cСтудент, cАспирант, cПреподаватель, cКурс, cКафедра })
            reg.Register(c);

        Console.WriteLine("  Выведенные ISA-отношения:");
        foreach (var c in reg.All)
        {
            var parents = reg.DirectParents(c).Select(p => p.Name).ToList();
            if (parents.Any())
                Console.WriteLine($"    {c.Name} ISA {string.Join(", ", parents)}");
            else
                Console.WriteLine($"    {c.Name} — корневой концепт");
        }

        Console.WriteLine("\n  Слоты концептов");
        foreach (var c in reg.All)
            Console.WriteLine($"    {c.Name,-18} слоты: {string.Join(", ", c.Slots.Select(s => s.Name))}");

        Header("Шкала Крипке");

        var kripke = new KripkeFrame();
        var wОсень  = new PossibleWorld("Осенний семестр");
        var wВесна  = new PossibleWorld("Весенний семестр");
        var wСессия = new PossibleWorld("Сессия");

        kripke.AddWorld(wОсень);
        kripke.AddWorld(wВесна);
        kripke.AddWorld(wСессия);

        kripke.AddAccessibility("Осенний семестр",  "Сессия");
        kripke.AddAccessibility("Сессия",           "Весенний семестр");
        kripke.AddAccessibility("Весенний семестр", "Сессия");

        foreach (var w in kripke.Worlds)
        {
            var acc = kripke.GetAccessibleWorlds(w.Name).Select(x => x.Name);
            Console.WriteLine($"  {w.Name} -> {string.Join(", ", acc)}");
        }

        Header("Экземпляры концептов в мирах");

        wОсень.AddInstance("ivanov",  cПреподаватель);
        wОсень.SetSlot("ivanov", "Имя",            "Иванов Алексей Петрович");
        wОсень.SetSlot("ivanov", "ДатаРождения",   new DateTime(1975, 4, 10));
        wОсень.SetSlot("ivanov", "Email",          "ivanov@uni.ru");
        wОсень.SetSlot("ivanov", "Должность",      "Доцент");
        wОсень.SetSlot("ivanov", "УчёнаяСтепень",  "Кандидат наук");

        wОсень.AddInstance("smirnov", cСтудент);
        wОсень.SetSlot("smirnov", "Имя",           "Смирнов Дмитрий");
        wОсень.SetSlot("smirnov", "ДатаРождения",  new DateTime(2003, 9, 1));
        wОсень.SetSlot("smirnov", "НомерЗачётки",  "СТ-2021-045");
        wОсень.SetSlot("smirnov", "КурсОбучения",  3);

        wОсень.AddInstance("korolev", cАспирант);
        wОсень.SetSlot("korolev", "Имя",                 "Королёв Никита");
        wОсень.SetSlot("korolev", "ДатаРождения",         new DateTime(1999, 2, 14));
        wОсень.SetSlot("korolev", "НомерЗачётки",         "АСП-2022-007");
        wОсень.SetSlot("korolev", "КурсОбучения",         1);
        wОсень.SetSlot("korolev", "НаучныйРуководитель",  "Иванов А.П.");
        wОсень.SetSlot("korolev", "ТемаДиссертации",      "Методы машинного обучения");

        wОсень.AddInstance("mathinf", cКурс);
        wОсень.SetSlot("mathinf", "Название", "Математическая информатика");
        wОсень.SetSlot("mathinf", "Часы",     72);
        wОсень.SetSlot("mathinf", "Семестр",  "Осенний");

        wОсень.AddInstance("kaf01", cКафедра);
        wОсень.SetSlot("kaf01", "Название",     "Кафедра информатики");
        wОсень.SetSlot("kaf01", "Аббревиатура", "КИ");

        wВесна.AddInstance("petrova", cПреподаватель);
        wВесна.SetSlot("petrova", "Имя",           "Петрова Мария Сергеевна");
        wВесна.SetSlot("petrova", "ДатаРождения",  new DateTime(1980, 11, 3));
        wВесна.SetSlot("petrova", "Должность",     "Профессор");
        wВесна.SetSlot("petrova", "УчёнаяСтепень", "Доктор наук");

        wВесна.AddInstance("nikitin", cСтудент);
        wВесна.SetSlot("nikitin", "Имя",          "Никитин Павел");
        wВесна.SetSlot("nikitin", "ДатаРождения", new DateTime(2002, 6, 22));
        wВесна.SetSlot("nikitin", "НомерЗачётки", "СТ-2020-112");
        wВесна.SetSlot("nikitin", "КурсОбучения", 4);

        wВесна.AddInstance("algos", cКурс);
        wВесна.SetSlot("algos", "Название", "Алгоритмы и структуры данных");
        wВесна.SetSlot("algos", "Часы",     90);
        wВесна.SetSlot("algos", "Семестр",  "Весенний");

        wСессия.AddInstance("orlov", cАспирант);
        wСессия.SetSlot("orlov", "Имя",                "Орлов Степан");
        wСессия.SetSlot("orlov", "ДатаРождения",        new DateTime(1998, 3, 5));
        wСессия.SetSlot("orlov", "НомерЗачётки",        "АСП-2023-002");
        wСессия.SetSlot("orlov", "КурсОбучения",        2);
        wСессия.SetSlot("orlov", "НаучныйРуководитель", "Петрова М.С.");

        foreach (var world in kripke.Worlds)
        {
            Console.WriteLine($"\n  [{world}]");
            foreach (var id in world.InstanceIds)
            {
                var c = world.GetConcept(id)!;
                Console.WriteLine($"    {id} : {c.Name}");
                foreach (var s in c.Slots)
                    Console.WriteLine($"      {s.Name} = {world.GetSlot(id, s.Name) ?? "<не задано>"}");
            }
        }

        Header("instance-of");

        Console.WriteLine("\n Осенний семестр");
        Check("ivanov     instance-of Преподаватель", wОсень.InstanceOf("ivanov",   cПреподаватель));
        Check("ivanov     instance-of Человек",       wОсень.InstanceOf("ivanov",   cЧеловек));       
        Check("ivanov     instance-of Студент",       wОсень.InstanceOf("ivanov",   cСтудент));       

        Check("smirnov    instance-of Студент",       wОсень.InstanceOf("smirnov",  cСтудент));
        Check("smirnov    instance-of Человек",       wОсень.InstanceOf("smirnov",  cЧеловек));       
        Check("smirnov    instance-of Аспирант",      wОсень.InstanceOf("smirnov",  cАспирант));     

        Check("korolev    instance-of Аспирант",      wОсень.InstanceOf("korolev",  cАспирант));
        Check("korolev    instance-of Студент",       wОсень.InstanceOf("korolev",  cСтудент));        
        Check("korolev    instance-of Человек",       wОсень.InstanceOf("korolev",  cЧеловек));        
        Check("korolev    instance-of Преподаватель", wОсень.InstanceOf("korolev",  cПреподаватель));  

        Console.WriteLine("\n Все Студент в осеннем семестре");
        foreach (var id in wОсень.AllInstancesOf(cСтудент))
            Console.WriteLine($"    {id} ({wОсень.GetConcept(id)!.Name})");

        Console.WriteLine("\n Все Человек в осеннем семестре");
        foreach (var id in wОсень.AllInstancesOf(cЧеловек))
            Console.WriteLine($"    {id} ({wОсень.GetConcept(id)!.Name})");

        Console.WriteLine("\n Все Аспирант во всех мирах");
        foreach (var world in kripke.Worlds)
            foreach (var id in world.AllInstancesOf(cАспирант))
                Console.WriteLine($"    [{world.Name}] {id} ({world.GetConcept(id)!.Name})");

        Header("Связи между концептами");

        var relReg = new RelationRegistry();

        var relВедёт     = new RelationConcept("ВедётКурс",     cПреподаватель, cКурс);
        var relВедётСпец = new RelationConcept("ВедётСпецкурс", cАспирант, cКурс);
        var relЗаписан   = new RelationConcept("ЗаписанНа",     cСтудент,  cКурс);

        relReg.Register(relВедёт);
        relReg.Register(relВедётСпец);
        relReg.Register(relЗаписан);

        Console.WriteLine("  Концепты связей");
        foreach (var r in relReg.All)
            Console.WriteLine($"    {r}");

        Console.WriteLine("\n ISA между связями");
        Check("ВедётСпецкурс ISA ВедётКурс",     relВедётСпец.IsA(relВедёт));   
        Check("ВедётКурс     ISA ВедётСпецкурс", relВедёт.IsA(relВедётСпец));   
        Check("ЗаписанНа     ISA ВедётКурс",     relЗаписан.IsA(relВедёт));     

        Console.WriteLine("\n Экземпляры связей");
        var ri1 = relReg.AddInstance("ri1", "ВедётКурс",     "ivanov",   "mathinf");
        var ri2 = relReg.AddInstance("ri2", "ВедётКурс",     "petrova",  "algos");
        var ri3 = relReg.AddInstance("ri3", "ВедётСпецкурс", "korolev",  "mathinf");
        var ri4 = relReg.AddInstance("ri4", "ЗаписанНа",     "smirnov",  "mathinf");
        var ri5 = relReg.AddInstance("ri5", "ЗаписанНа",     "nikitin",  "algos");

        foreach (var ri in new[] { ri1, ri2, ri3, ri4, ri5 })
            Console.WriteLine($"  {ri}");

        Console.WriteLine("\n instance-of для связей");
        Check("ri1 instance-of ВедётКурс",     relReg.InstanceOf(ri1, relВедёт));
        Check("ri3 instance-of ВедётСпецкурс", relReg.InstanceOf(ri3, relВедётСпец));
        Check("ri4 instance-of ЗаписанНа",     relReg.InstanceOf(ri4, relЗаписан));
        Check("ri4 instance-of ВедётКурс",     relReg.InstanceOf(ri4, relВедёт));     

        Console.WriteLine("\n Все экземпляры ВедётКурс");
        foreach (var ri in relReg.AllInstancesOf(relВедёт))
            Console.WriteLine($"    {ri}");
    }
}
