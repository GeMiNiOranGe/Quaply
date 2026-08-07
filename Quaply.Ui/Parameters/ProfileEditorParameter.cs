namespace Quaply.Ui.Parameters;

/// <summary>
/// Discriminated-union-style navigation parameter for <see cref="ProfileEditorViewModel"/>.
/// Each case carries exactly the data it needs — no shared nullable fields,
/// no invalid combinations possible at compile time.
/// </summary>
public abstract record ProfileEditorParameter
{
    private ProfileEditorParameter() { }

    /// <summary>
    /// Open the editor empty, to create a brand-new profile.
    /// </summary>
    public sealed record Add : ProfileEditorParameter;

    /// <summary>
    /// Open the editor pre-filled with an existing profile, saving updates it.
    /// </summary>
    public sealed record Edit(int ProfileId) : ProfileEditorParameter;

    // Reserved for future use — uncomment when duplication is implemented.
    // Pre-fills the form from an existing profile, but Save creates a NEW profile.
    // /// <summary>
    // /// Open the editor pre-filled from an existing profile, to create a copy of it.
    // /// </summary>
    // public sealed record Duplicate(int SourceProfileId) : ProfileEditorParameter;
}
