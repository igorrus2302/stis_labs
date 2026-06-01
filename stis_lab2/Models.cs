using System;
using System.Collections.Generic;
using System.Linq;

namespace University.stis_lab2;

// описание атрибута
public sealed class AttributeDescription
{
    public AttributeDescription(string name, Type range)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Имя атрибута не может быть пустым.", nameof(name))
            : name;
        Range = range ?? throw new ArgumentNullException(nameof(range));
    }

    public string Name { get; }

    public Type Range { get; }
}

// описание концепта
public sealed class Concept
{
    private readonly List<AttributeDescription> _attributes = [];

    public Concept(string name, IEnumerable<AttributeDescription>? attributes = null)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Имя концепта не может быть пустым.", nameof(name))
            : name;

        if (attributes is not null)
            _attributes.AddRange(attributes);
    }

    public string Name { get; }

    public IReadOnlyCollection<AttributeDescription> Attributes => _attributes;

    // проверка на подтип 
    public bool IsSubconceptOf(Concept other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return ReferenceEquals(this, other)
               || Name == other.Name
               || HasAllRequiredAttributesOf(other);
    }

    // получение списка обязательных атрибутов концепта
    public IReadOnlyCollection<AttributeDescription> GetRequiredAttributes()
    {
        return _attributes
            .GroupBy(a => a.Name)
            .Select(g => g.Last())
            .ToList();
    }

    // проверка на содержание всех обязательных атрибутов 
    private bool HasAllRequiredAttributesOf(Concept other)
    {
        var own = GetRequiredAttributes().ToDictionary(a => a.Name);
        return other.GetRequiredAttributes().All(a =>
            own.TryGetValue(a.Name, out var ownAttr) && ownAttr.Range == a.Range);
    }
}

// описание фрейма
public sealed class Frame
{
    public Frame(string name, Concept concept)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Имя фрейма не может быть пустым.", nameof(name))
            : name;
        Concept = concept ?? throw new ArgumentNullException(nameof(concept));
    }

    public string Name { get; }

    public Concept Concept { get; }

    // проверка на то, что фрейм является экземпляром 
    public bool IsInstanceOf(Concept concept)
    {
        ArgumentNullException.ThrowIfNull(concept);
        return Concept.IsSubconceptOf(concept);
    }
}

// описание связи
public sealed class RelationConcept
{
    public RelationConcept(string name, Concept domain, Concept range)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Имя связи не может быть пустым.", nameof(name))
            : name;
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Range  = range  ?? throw new ArgumentNullException(nameof(range));
    }

    public string Name { get; }

    public Concept Domain { get; }

    public Concept Range { get; }

    // проверка на то, что связь является подотношением
    public bool IsSubrelationOf(RelationConcept other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return ReferenceEquals(this, other)
               || Name == other.Name
               || (Domain.IsSubconceptOf(other.Domain) && Range.IsSubconceptOf(other.Range));
    }
}

// конкретный экземпляр
public sealed class RelationFrame
{
    public RelationFrame(RelationConcept relation, Frame subject, Frame obj)
    {
        Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        Subject  = subject  ?? throw new ArgumentNullException(nameof(subject));
        Object   = obj      ?? throw new ArgumentNullException(nameof(obj));
    }

    public RelationConcept Relation { get; }

    public Frame Subject { get; }

    public Frame Object { get; }

    // проверка на то, что является экземпляром
    public bool IsInstanceOf(RelationConcept relation)
    {
        ArgumentNullException.ThrowIfNull(relation);
        return Relation.IsSubrelationOf(relation)
               && Subject.IsInstanceOf(Relation.Domain)
               && Object.IsInstanceOf(Relation.Range)
               && Subject.IsInstanceOf(relation.Domain)
               && Object.IsInstanceOf(relation.Range);
    }
}