namespace ThingsEdge.Common.Utils;

public abstract class ConstantPool
{
    private readonly Dictionary<string, IConstant> constants = new();

    private int _nextId = 1;

    /// <summary>Shortcut of <c>this.ValueOf(firstNameComponent.Name + "#" + secondNameComponent)</c>.</summary>
    public IConstant ValueOf<T>(Type firstNameComponent, string secondNameComponent)
    {
        Contract.Requires(firstNameComponent != null);
        Contract.Requires(secondNameComponent != null);

        return ValueOf<T>($"{firstNameComponent!.Name}'#'{secondNameComponent}");
    }

    /// <summary>
    ///     Returns the <see cref="IConstant" /> which is assigned to the specified <c>name</c>.
    ///     If there's no such <see cref="IConstant" />, a new one will be created and returned.
    ///     Once created, the subsequent calls with the same <c>name</c> will always return the previously created one
    ///     (i.e. singleton.)
    /// </summary>
    /// <param name="name">the name of the <see cref="IConstant" /></param>
    public IConstant ValueOf<T>(string name)
    {
        IConstant c;

        lock (constants)
        {
            if (constants.TryGetValue(name, out c!))
            {
                return c;
            }
            else
            {
                c = NewInstance0<T>(name);
            }
        }

        return c;
    }

    /// <summary>Returns <c>true</c> if a 'AttributeKey' exists for the given <c>name</c>.</summary>
    public bool Exists(string name)
    {
        CheckNotNullAndNotEmpty(name);
        lock (constants)
        {
            return constants.ContainsKey(name);
        }
    }

    /// <summary>
    ///     Creates a new <see cref="IConstant" /> for the given <c>name</c> or fail with an
    ///     <see cref="ArgumentException" /> if a <see cref="IConstant" /> for the given <c>name</c> exists.
    /// </summary>
    public IConstant NewInstance<T>(string name)
    {
        if (Exists(name))
        {
            throw new ArgumentException($"'{name}' is already in use");
        }

        IConstant c = NewInstance0<T>(name);

        return c;
    }

    // Be careful that this dose not check whether the argument is null or empty.
    IConstant NewInstance0<T>(string name)
    {
        lock (constants)
        {
            IConstant c = NewConstant<T>(_nextId, name);
            constants[name] = c;
            _nextId++;
            return c;
        }
    }

    static void CheckNotNullAndNotEmpty(string name) => Contract.Requires(!string.IsNullOrEmpty(name));

    protected abstract IConstant NewConstant<T>(int id, string name);
}
