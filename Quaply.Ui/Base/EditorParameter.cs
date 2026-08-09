namespace Quaply.Ui.Base;

public abstract record EditorParameter<TEntity>
{
    private protected EditorParameter() { }

    public sealed record Add : EditorParameter<TEntity>;

    public sealed record Edit(int Id) : EditorParameter<TEntity>;

    public sealed record Duplicate(int SourceId) : EditorParameter<TEntity>;

    public static EditorParameter<TEntity> ForAdd()
    {
        return new Add();
    }

    public static EditorParameter<TEntity> ForEdit(int id)
    {
        return new Edit(id);
    }

    public static EditorParameter<TEntity> ForDuplicate(int sourceId)
    {
        return new Duplicate(sourceId);
    }

    /*
    public TResult Match<TResult>(
        Func<TResult> onAdd,
        Func<int, TResult> onEdit,
        Func<int, TResult> onDuplicate
    )
    {
        return this switch
        {
            Add => onAdd(),
            Edit e => onEdit(e.Id),
            Duplicate d => onDuplicate(d.SourceId),
            _ => throw new NotSupportedException(
                $"Unhandled case: {GetType().Name}"
            ),
        };
    }
    */
}
